﻿namespace Proelium.Ecs;

public abstract class Query
{
    public EntityManager EntityManager { get; internal set; } = null!;

    /// <summary>
    /// The component types this query pertains to.
    /// </summary>
    public abstract IEnumerable<Type> GetComponentTypes();
    /// <summary>
    /// The added component, and the updated entity.
    /// </summary>
    public abstract void OnAddComponent(Component component, Entity entity);
    /// <summary>
    /// The removed component, and the entity with the remaining components.
    /// </summary>
    public abstract void OnRemoveComponent(Component component, Entity entity);
}

internal class QueryManager
{
    internal readonly EntityManager entityManager;
    // <query type, query>
    private readonly Dictionary<Type, Query> queries = new();
    // <component type, List<Query>>
    private readonly Dictionary<Type, List<Query>> subscriptions = new();

    internal QueryManager(EntityManager entityManager)
    {
        this.entityManager = entityManager;
    }

    internal void OnAddComponent(Component component, Entity entity)
    {
        if (subscriptions.TryGetValue(component.GetType(), out var subscribedQueries))
        {
            foreach (var subscribedQuery in subscribedQueries)
            {
                subscribedQuery.OnAddComponent(component, entity);
            }
        }
    }

    internal void OnRemoveComponent(Component component, Entity entity)
    {
        if (subscriptions.TryGetValue(component.GetType(), out var subscribedQueries))
        {
            foreach (var subscribedQuery in subscribedQueries)
            {
                subscribedQuery.OnRemoveComponent(component, entity);
            }
        }
    }

    internal T Get<T>() where T : Query, new()
    {
        // Return existing query, if it exists
        {
            if (queries.TryGetValue(typeof(T), out var query))
            {
                return (T)query;
            }
        }

        // Generate new query
        {
            T query = new()
            {
                EntityManager = entityManager
            };

            foreach (var componentType in query.GetComponentTypes())
            {
                if (!subscriptions.TryGetValue(componentType, out var subscribedQueries))
                {
                    subscribedQueries = new();
                    subscriptions[componentType] = subscribedQueries;
                }

                subscribedQueries.Add(query);
            }

            return query;
        }
    }
}