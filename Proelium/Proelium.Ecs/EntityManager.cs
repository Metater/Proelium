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

    public T Get<T>() where T : Component
    {
        Type type = typeof(T);

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

    public bool TryGet<T>([MaybeNullWhen(false)] out T component) where T : Component
    {
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

    public bool Contains<T>() where T : Component
    {
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
}

public abstract class Component // : IAspect
{
    public ulong EntityId { get; internal set; }

    //public void GetComponentTypes(List<Type> componentTypes)
    //{
    //    componentTypes.Add(GetType());
    //}
}

//public interface IAspect
//{
//    public void GetComponentTypes(List<Type> componentTypes);
//}

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
    public Entity CreateEntity()
    {
        if (!componentListPool.TryPop(out var components))
        {
            components = new();
        }

        ulong entityId = entityIdGen.Next;
        Entity entity = new(entityId, components);
        entities[entityId] = entity;
        return entity;
    }

    public bool DestroyEntity(ulong entityId)
    {
        if (!entities.TryGetValue(entityId, out var entity))
        {
            return false;
        }

        for (int i = entity.components.Count - 1; i >= 0; i--)
        {
            Component component = entity.components[i];
            entity.components.RemoveAt(i);
            queryManager.OnRemoveComponent(component, entity);
        }

        Trace.Assert(entity.components.Count == 0);

        componentListPool.Push(entity.components);
        return true;
    }

    public Entity GetEntity(ulong entityId)
    {
        if (!entities.TryGetValue(entityId, out var entity))
        {
            throw new Exception($"Could not get entity {entityId}; entity doesn't exist.");
        }

        return entity;
    }

    public bool TryGetEntity(ulong entityId, [MaybeNullWhen(false)] out Entity entity)
    {
        return entities.TryGetValue(entityId, out entity);
    }
    #endregion

    #region Components
    public void AddComponent<T>(ulong entityId, T component) where T : Component
    {
        component.EntityId = entityId;

        Type type = typeof(T);

        if (!entities.TryGetValue(entityId, out var entity))
        {
            throw new Exception($"Entity with id {entityId} not found while adding component {component} with type {type}.");
        }

        foreach (var comp in entity.components)
        {
            if (comp.GetType() == type)
            {
                throw new Exception($"Found duplicate component {comp} of type {type} while adding component {component} to entity {entityId}.");
            }
        }

        entity.components.Add(component);
        queryManager.OnAddComponent(component, entity);
    }

    public bool TryAddComponent<T>(ulong entityId, T component) where T : Component
    {
        component.EntityId = entityId;

        if (!entities.TryGetValue(entityId, out var entity))
        {
            return false;
        }

        Type type = typeof(T);

        foreach (var comp in entity.components)
        {
            if (comp.GetType() == type)
            {
                return false;
            }
        }

        entity.components.Add(component);
        queryManager.OnAddComponent(component, entity);
        return true;
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
            Component component = entity.components[i];
            if (component.GetType() == type)
            {
                entity.components.RemoveAt(i);
                queryManager.OnRemoveComponent(component, entity);
                return true;
            }
        }

        return false;
    }
    #endregion

    public T GetQuery<T>() where T : Query, new()
    {
        return queryManager.Get<T>();
    }
}