using System.Diagnostics.CodeAnalysis;

namespace Proelium.Ecs;

internal class EntityIdGen
{
    private ulong next = 0;
    internal ulong Next => next++;
}

public abstract class Component
{
    public ulong EntityId { get; internal set; }
}

public class EntityManager
{
    internal readonly EntityIdGen entityIdGen = new();
    // <entity id, List<Component>>
    private readonly Dictionary<ulong, List<Component>> entities;
    private readonly QueryManager queryManager;

    private readonly Stack<List<Component>> componentListPool = new();

    #if DEBUG
    private readonly HashSet<Type> typesCache = new();
    #endif

    public EntityManager(int entitiesCapacity = 0)
    {
        entities = new(entitiesCapacity);
        queryManager = new() { EntityManager = this };
    }

    #region Entities
    public ulong CreateEntity()
    {
        ulong entityId = entityIdGen.Next;
        if (!componentListPool.TryPop(out var components))
        {
            components = new();
        }
        entities[entityId] = components;
        return entityId;
    }

    public bool DestroyEntity(ulong entityId)
    {
        if (!entities.TryGetValue(entityId, out var components))
        {
            return false;
        }

        foreach (var component in components)
        {
            queryManager.OnRemoveComponent(component);
        }
        components.Clear();

        componentListPool.Push(components);
        return true;
    }
    #endregion

    #region Components
    public void AddComponent<T>(ulong entityId, T component) where T : Component
    {
        component.EntityId = entityId;

        if (!entities.TryGetValue(entityId, out var components))
        {
            components = new();
            entities[entityId] = components;
        }

        components.Add(component);

        #if !OPTIMIZE
        typesCache.Clear();
        foreach (var comp in components)
        {
            Type type = comp.GetType();
            bool unique = typesCache.Add(type);
            if (!unique)
            {
                throw new Exception($"Found duplicate component type {type} while adding component {component} to entity {entityId}.");
            }
        }
        #endif
    }

    public bool RemoveComponent<T>(ulong entityId) where T : Component
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
                components.RemoveAt(i);
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