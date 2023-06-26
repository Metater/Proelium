using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Proelium.Ecs;

internal class EntityIdGen
{
    private ulong next = 0;
    internal ulong Next => next++;
}

public readonly struct Entity
{
    public readonly ulong entityId;
    internal readonly List<Component> components;
    public readonly IReadOnlyList<Component> Components => components;

    internal Entity(ulong entityId, List<Component> components)
    {
        this.entityId = entityId;
        this.components = components;
    }
}

public abstract class Component
{
    public ulong EntityId { get; internal set; }
}

public class EntityManager
{
    internal readonly EntityIdGen entityIdGen = new();
    // <entity id, Entity>
    private readonly Dictionary<ulong, Entity> entities;
    private readonly QueryManager queryManager;

    private readonly Stack<List<Component>> componentListPool = new();

    public EntityManager(int entitiesCapacity = 0)
    {
        entities = new(entitiesCapacity);
        queryManager = new() { EntityManager = this };
    }

    #region Entities
    public ulong CreateEntity()
    {
        if (!componentListPool.TryPop(out var components))
        {
            components = new();
        }

        ulong entityId = entityIdGen.Next;
        entities[entityId] = new Entity(entityId, components);
        return entityId;
    }

    public bool DestroyEntity(ulong entityId)
    {
        if (!entities.TryGetValue(entityId, out var entity))
        {
            return false;
        }

        for (int i = entity.components.Count - 1; i >= 0; i--)
        {
            Component removed = entity.components[i];
            entity.components.RemoveAt(i);
            queryManager.OnRemoveComponent(removed, entity);
        }

        Debug.Assert(entity.components.Count == 0);

        componentListPool.Push(entity.components);
        return true;
    }
    #endregion

    #region Components
    public void AddComponent<T>(ulong entityId, T component) where T : Component
    {
        component.EntityId = entityId;

        if (!entities.TryGetValue(entityId, out var entity))
        {
            throw new Exception($"Entity with id {entityId} not found while adding component {component}.");
        }

        #if !OPTIMIZE
        Type type = typeof(T);
        foreach (var comp in entity.components)
        {
            if (comp.GetType() == type)
            {
                throw new Exception($"Found duplicate component type {type} while adding component {component} to entity {entityId}.");
            }
        }
        #endif

        entity.components.Add(component);
    }

    public bool RemoveComponent<T>(ulong entityId) where T : Component
    {
        if (!entities.TryGetValue(entityId, out var entity))
        {
            return false;
        }

        Type type = typeof(T);

        for (int i = 0; i < entity.components.Count; i++)
        {
            if (entity.components[i].GetType() == type)
            {
                entity.components.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    public T GetComponent<T>(ulong entityId) where T : Component
    {
        Type type = typeof(T);

        if (!entities.TryGetValue(entityId, out var components))
        {
            throw new Exception($"Could not get component of type {type} from entity {entityId}; entity doesn't exist.");
        }

        for (int i = 0; i < components.Count; i++)
        {
            Component comp = components[i];
            if (comp.GetType() == type)
            {
                return (T)comp;
            }
        }

        throw new Exception($"Could not get component of type {type} from entity {entityId}; component doesn't exist on entity.");
    }

    public bool TryGetComponent<T>(ulong entityId, [MaybeNullWhen(false)] out T component) where T : Component
    {
        if (!entities.TryGetValue(entityId, out var components))
        {
            component = default;
            return false;
        }

        Type type = typeof(T);

        for (int i = 0; i < components.Count; i++)
        {
            Component comp = components[i];
            if (comp.GetType() == type)
            {
                component = (T)comp;
                return true;
            }
        }

        component = default;
        return false;
    }

    public IReadOnlyList<Component> GetComponents(ulong entityId)
    {
        if (!entities.TryGetValue(entityId, out var components))
        {
            throw new Exception($"Could not get components from entity {entityId}; entity doesn't exist.");
        }

        return components;
    }

    public bool TryGetComponents(ulong entityId, [MaybeNullWhen(false)] out IReadOnlyList<Component> components)
    {
        if (!entities.TryGetValue(entityId, out var comps))
        {
            components = default;
            return false;
        }

        components = comps;
        return true;
    }

    public bool ContainsComponent<T>(ulong entityId) where T : Component
    {
        if (!entities.TryGetValue(entityId, out var components))
        {
            return false;
        }

        Type type = typeof(T);

        for (int i = 0; i < components.Count; i++)
        {
            if (components[i].GetType() == type)
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    public T Query<T>() where T : Query, new()
    {
        return queryManager.Get<T>();
    }
}