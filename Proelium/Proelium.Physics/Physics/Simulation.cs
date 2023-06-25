using Microsoft.VisualBasic;
using System.Numerics;

namespace Proelium.Server.Physics;

public class Simulation
{
    private readonly Partitioner partitioner;
    private readonly ISimulationListener listener;

    public readonly HashSet<Entity> entities = new();
    public readonly Dictionary<Cell, List<Entity>> cells = new();

    private readonly List<Cell> cellsCache = new();
    private readonly Stack<List<Entity>> entityListPool = new();

    public Simulation(Partitioner partitioner, ISimulationListener listener)
    {
        this.partitioner = partitioner;
        this.listener = listener;
    }

    public void Spawn(Entity entity)
    {
        entity.Cell = partitioner.GetCell(entity.Position);

        entities.Add(entity);
        AddEntityToCell(entity.Cell, entity);
    }
    public void Despawn(Entity entity)
    {
        if (entities.Remove(entity))
        {
            RemoveEntityFromCell(entity.Cell, entity);
        }
    }
    public bool Contains(Entity entity)
    {
        return entities.Contains(entity);
    }
    public void Clear()
    {
        entities.Clear();

        foreach (var cell in cells.Values)
        {
            cell.Clear();
            entityListPool.Push(cell);
        }
        cells.Clear();
    }


    public void Tick(float deltaTime)
    {
        foreach (var entity in entities)
        {
            // Skip static entities
            {
                bool hasStatic = entity.Flags.Has(EntityFlags.Static);
                if (hasStatic)
                {
                    continue;
                }
            }

            // Move
            entity.Position += entity.Velocity * deltaTime;

            // Interact
            {
                bool canBeRepulsed = entity.Flags.Has(EntityFlags.CanBeRepulsed);
                bool hasCollider = entity.Flags.Has(EntityFlags.Collider);
                bool hasTriggerCollider = entity.Flags.Has(EntityFlags.TriggerCollider);

                if (canBeRepulsed || (hasCollider && entity.Velocity != Vector2.Zero) || (hasCollider && hasTriggerCollider))
                {
                    cellsCache.Clear();
                    entity.Cell.GetSurrounding(cellsCache);
                    foreach (var cell in cellsCache)
                    {
                        if (!cells.TryGetValue(cell, out var surrounding))
                        {
                            continue;
                        }

                        foreach (var surrounder in surrounding)
                        {
                            if (surrounder == entity)
                            {
                                continue;
                            }

                            if (!Entity.LayersOverlap(surrounder, entity))
                            {
                                continue;
                            }

                            Interact(deltaTime, entity, surrounder, canBeRepulsed, hasCollider, hasTriggerCollider);
                        }
                    }
                }
            }

            if (entity.Velocity != Vector2.Zero)
            {
                // Apply drag
                {
                    bool hasDrag = entity.Flags.Has(EntityFlags.Drag);
                    if (hasDrag)
                    {
                        entity.Velocity *= 1.0f - (entity.DragForce * deltaTime);
                    }
                }

                // Apply velocity epsilon
                {
                    bool hasVelocityEpsilon = entity.Flags.Has(EntityFlags.VelocityEpsilon);
                    if (hasVelocityEpsilon)
                    {
                        if (entity.Velocity.X != 0 && MathF.Abs(entity.Velocity.X) < entity.VelocityEpsilon)
                        {
                            entity.Velocity = new(0, entity.Velocity.Y);
                        }

                        if (entity.Velocity.Y != 0 && MathF.Abs(entity.Velocity.Y) < entity.VelocityEpsilon)
                        {
                            entity.Velocity = new(entity.Velocity.X, 0);
                        }
                    }
                }
            }

            // Apply bounds
            if (entity.Velocity != Vector2.Zero)
            {
                bool hasBounds = entity.Flags.Has(EntityFlags.Bounds);
                if (hasBounds)
                {
                    Direction direction = Direction.None;

                    if (entity.Position.Y > entity.Bounds.North)
                    {
                        direction |= Direction.North;
                        entity.Position.Y = entity.Bounds.North;
                        entity.Velocity = new(entity.Velocity.X, 0);
                    }
                    else if (entity.Position.Y < entity.Bounds.South)
                    {
                        direction |= Direction.South;
                        entity.Position.Y = entity.Bounds.South;
                        entity.Velocity = new(entity.Velocity.X, 0);
                    }

                    if (entity.Position.X > entity.Bounds.East)
                    {
                        direction |= Direction.East;
                        entity.Position.X = entity.Bounds.East;
                        entity.Velocity = new(0, entity.Velocity.Y);
                    }
                    else if (entity.Position.X < entity.Bounds.West)
                    {
                        direction |= Direction.West;
                        entity.Position.X = entity.Bounds.West;
                        entity.Velocity = new(0, entity.Velocity.Y);
                    }

                    if (direction != Direction.None)
                    {
                        listener.OnBoundsStop(entity, direction);
                    }
                }
            }

            // Update cell
            {
                Cell initialCell = entity.Cell;
                entity.Cell = partitioner.GetCell(entity.Position);
                if (entity.Cell != initialCell)
                {
                    RemoveEntityFromCell(initialCell, entity);
                    AddEntityToCell(entity.Cell, entity);
                }
            }
        }
    }

    private void Interact(float deltaTime, Entity entity, Entity surrounder, bool canBeRepulsed, bool hasCollider, bool hasTriggerCollider)
    {
        // Apply repulsion
        {
            bool surrounderCanRepulseOthers = surrounder.Flags.Has(EntityFlags.CanRepulseOthers);

            if (surrounderCanRepulseOthers && canBeRepulsed)
            {
                float distanceX = surrounder.Position.X - entity.Position.X;
                float distanceY = surrounder.Position.Y - entity.Position.Y;
                float squareDistance = distanceX * distanceX + distanceY * distanceY;
                float minDistance = entity.RepulsionRadius + surrounder.RepulsionRadius;

                bool withinRadius = minDistance * minDistance > squareDistance;
                if (withinRadius)
                {
                    float minMaxRepulsionForce = Math.Min(entity.RepulsionMaxMagnitude, surrounder.RepulsionMaxMagnitude);
                    Vector2 repulsionForce;

                    if (squareDistance != 0)
                    {
                        float repulsionMagnitude = (entity.RepulsionForce + surrounder.RepulsionForce) * (1f / squareDistance);
                        repulsionMagnitude = Math.Clamp(repulsionMagnitude, -minMaxRepulsionForce, minMaxRepulsionForce);
                        float reciprocalDistance = MathF.ReciprocalSqrtEstimate(squareDistance);
                        repulsionForce = new Vector2(distanceX * reciprocalDistance, distanceY * reciprocalDistance) * -repulsionMagnitude;
                    }
                    else
                    {
                        repulsionForce = new Vector2(0, 1) * -minMaxRepulsionForce;
                    }

                    entity.Velocity += repulsionForce * deltaTime;
                    listener.OnRepulsion(entity, surrounder);
                }
            }
        }

        // AABB checks
        {
            bool surrounderHasCollider = surrounder.Flags.Has(EntityFlags.Collider);
            if (surrounderHasCollider && hasCollider)
            {
                bool surrounderHasStaticCollider = surrounder.Flags.Has(EntityFlags.Static);
                // Trigger
                if (hasTriggerCollider)
                {
                    if (entity.Collider.Overlaps(surrounder.Collider))
                    {
                        listener.OnTrigger(entity, surrounder);
                    }
                }
                // Collide
                else if (surrounderHasStaticCollider)
                {
                    Collide(entity, surrounder);
                }
            }
        }
    }

    // TODO CONTINUE HERE A LOT WILL CHANGE
    private void Collide(Entity entity, Entity surrounder)
    {
        AABB entityCollider = entity.Collider.Offset(entity.Position);
        AABB surrounderCollider = surrounder.Collider.Offset(surrounder.Position);
        Direction direction = Direction.None;

        if (entityCollider.North > surrounderCollider.North)
        {
            direction |= Direction.North;
            entity.Position.Y = surrounderCollider.North;
            entity.Velocity = new(entity.Velocity.X, 0);
        }
        else if (entityCollider.South < surrounderCollider.South)
        {
            direction |= Direction.South;
            entity.Position.Y = surrounderCollider.South;
            entity.Velocity = new(entity.Velocity.X, 0);
        }

        if (entityCollider.East > surrounderCollider.East)
        {
            direction |= Direction.East;
            entity.Position.X = surrounderCollider.East;
            entity.Velocity = new(0, entity.Velocity.Y);
        }
        else if (entityCollider.West < surrounderCollider.West)
        {
            direction |= Direction.West;
            entity.Position.X = surrounderCollider.West;
            entity.Velocity = new(0, entity.Velocity.Y);
        }

        if (direction != Direction.None)
        {
            listener.OnStop(entity, surrounder, direction);
        }
    }

    public void GetEntitiesInAABB(AABB aabb, ICollection<Entity> output)
    {
        cellsCache.Clear();
        partitioner.GetCellsInAABB(aabb, cellsCache);
        foreach (var cell in cellsCache)
        {
            if (cells.TryGetValue(cell, out var cellEntities))
            {
                foreach (var entity in cellEntities)
                {
                    output.Add(entity);
                }
            }
        }
    }

    private void AddEntityToCell(Cell cell, Entity entity)
    {
        if (!cells.TryGetValue(cell, out var cellEntities))
        {
            if (!entityListPool.TryPop(out cellEntities))
            {
                cellEntities = new();
            }
            cells[cell] = cellEntities;
        }

        cellEntities.Add(entity);
    }
    private void RemoveEntityFromCell(Cell cell, Entity entity)
    {
        List<Entity> cellEntities = cells[cell];
        cellEntities.Remove(entity);

        if (cellEntities.Count == 0)
        {
            cells.Remove(cell);
            entityListPool.Push(cellEntities);
        }
    }
}