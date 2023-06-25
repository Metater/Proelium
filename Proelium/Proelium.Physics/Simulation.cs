using System.Numerics;

namespace Proelium.Physics;

public interface ISimulationListener
{
    public void OnRepulsion(Entity repulsee, Entity repulsor);
    public void OnTrigger(Entity triggeree, Entity triggerer);
    public void OnStop(Entity stopee, Entity stopper, Direction direction);
    public void OnBoundsStop(Entity entity, Direction direction);
}

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
        entity.cell = partitioner.GetCell(entity.position);

        entities.Add(entity);
        AddEntityToCell(entity.cell, entity);
    }
    public void Despawn(Entity entity)
    {
        if (entities.Remove(entity))
        {
            RemoveEntityFromCell(entity.cell, entity);
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
                bool hasStatic = entity.flags.Has(EntityFlags.Static);
                if (hasStatic)
                {
                    continue;
                }
            }

            // Move
            entity.position += entity.velocity * deltaTime;

            // Interact
            {
                bool canBeRepulsed = entity.flags.Has(EntityFlags.CanBeRepulsed);
                bool hasCollider = entity.flags.Has(EntityFlags.Collider);
                bool hasTriggerCollider = entity.flags.Has(EntityFlags.TriggerCollider);

                if (canBeRepulsed || hasCollider && entity.velocity != Vector2.Zero || hasCollider && hasTriggerCollider)
                {
                    cellsCache.Clear();
                    entity.cell.GetSurrounding(cellsCache);
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

            if (entity.velocity != Vector2.Zero)
            {
                // Apply drag
                {
                    bool hasDrag = entity.flags.Has(EntityFlags.Drag);
                    if (hasDrag)
                    {
                        entity.velocity *= 1.0f - entity.dragForce * deltaTime;
                    }
                }

                // Apply velocity epsilon
                {
                    bool hasVelocityEpsilon = entity.flags.Has(EntityFlags.VelocityEpsilon);
                    if (hasVelocityEpsilon)
                    {
                        if (entity.velocity.X != 0 && MathF.Abs(entity.velocity.X) < entity.velocityEpsilon)
                        {
                            entity.velocity = new(0, entity.velocity.Y);
                        }

                        if (entity.velocity.Y != 0 && MathF.Abs(entity.velocity.Y) < entity.velocityEpsilon)
                        {
                            entity.velocity = new(entity.velocity.X, 0);
                        }
                    }
                }
            }

            // Apply bounds
            if (entity.velocity != Vector2.Zero)
            {
                bool hasBounds = entity.flags.Has(EntityFlags.Bounds);
                if (hasBounds)
                {
                    Direction direction = Direction.None;

                    if (entity.position.Y > entity.bounds.North)
                    {
                        direction |= Direction.North;
                        entity.position.Y = entity.bounds.North;
                        entity.velocity = new(entity.velocity.X, 0);
                    }
                    else if (entity.position.Y < entity.bounds.South)
                    {
                        direction |= Direction.South;
                        entity.position.Y = entity.bounds.South;
                        entity.velocity = new(entity.velocity.X, 0);
                    }

                    if (entity.position.X > entity.bounds.East)
                    {
                        direction |= Direction.East;
                        entity.position.X = entity.bounds.East;
                        entity.velocity = new(0, entity.velocity.Y);
                    }
                    else if (entity.position.X < entity.bounds.West)
                    {
                        direction |= Direction.West;
                        entity.position.X = entity.bounds.West;
                        entity.velocity = new(0, entity.velocity.Y);
                    }

                    if (direction != Direction.None)
                    {
                        listener.OnBoundsStop(entity, direction);
                    }
                }
            }

            // Update cell
            {
                Cell initialCell = entity.cell;
                entity.cell = partitioner.GetCell(entity.position);
                if (entity.cell != initialCell)
                {
                    RemoveEntityFromCell(initialCell, entity);
                    AddEntityToCell(entity.cell, entity);
                }
            }
        }
    }

    private void Interact(float deltaTime, Entity entity, Entity surrounder, bool canBeRepulsed, bool hasCollider, bool hasTriggerCollider)
    {
        // Apply repulsion
        {
            bool surrounderCanRepulseOthers = surrounder.flags.Has(EntityFlags.CanRepulseOthers);

            if (surrounderCanRepulseOthers && canBeRepulsed)
            {
                float distanceX = surrounder.position.X - entity.position.X;
                float distanceY = surrounder.position.Y - entity.position.Y;
                float squareDistance = distanceX * distanceX + distanceY * distanceY;
                float minDistance = entity.repulsionRadius + surrounder.repulsionRadius;

                bool withinRadius = minDistance * minDistance > squareDistance;
                if (withinRadius)
                {
                    float minMaxRepulsionForce = Math.Min(entity.repulsionMaxMagnitude, surrounder.repulsionMaxMagnitude);
                    Vector2 repulsionForce;

                    if (squareDistance != 0)
                    {
                        float repulsionMagnitude = (entity.repulsionForce + surrounder.repulsionForce) * (1f / squareDistance);
                        repulsionMagnitude = Math.Clamp(repulsionMagnitude, -minMaxRepulsionForce, minMaxRepulsionForce);
                        float reciprocalDistance = MathF.ReciprocalSqrtEstimate(squareDistance);
                        repulsionForce = new Vector2(distanceX * reciprocalDistance, distanceY * reciprocalDistance) * -repulsionMagnitude;
                    }
                    else
                    {
                        repulsionForce = new Vector2(0, 1) * -minMaxRepulsionForce;
                    }

                    entity.velocity += repulsionForce * deltaTime;
                    listener.OnRepulsion(entity, surrounder);
                }
            }
        }

        // AABB checks
        {
            bool surrounderHasCollider = surrounder.flags.Has(EntityFlags.Collider);
            if (surrounderHasCollider && hasCollider)
            {
                bool surrounderHasStaticCollider = surrounder.flags.Has(EntityFlags.Static);
                // Trigger
                if (hasTriggerCollider)
                {
                    if (entity.collider.Overlaps(surrounder.collider))
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
        AABB entityCollider = entity.collider.Offset(entity.position);
        AABB surrounderCollider = surrounder.collider.Offset(surrounder.position);
        Direction direction = Direction.None;

        if (entityCollider.North > surrounderCollider.North)
        {
            direction |= Direction.North;
            entity.position.Y = surrounderCollider.North;
            entity.velocity = new(entity.velocity.X, 0);
        }
        else if (entityCollider.South < surrounderCollider.South)
        {
            direction |= Direction.South;
            entity.position.Y = surrounderCollider.South;
            entity.velocity = new(entity.velocity.X, 0);
        }

        if (entityCollider.East > surrounderCollider.East)
        {
            direction |= Direction.East;
            entity.position.X = surrounderCollider.East;
            entity.velocity = new(0, entity.velocity.Y);
        }
        else if (entityCollider.West < surrounderCollider.West)
        {
            direction |= Direction.West;
            entity.position.X = surrounderCollider.West;
            entity.velocity = new(0, entity.velocity.Y);
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