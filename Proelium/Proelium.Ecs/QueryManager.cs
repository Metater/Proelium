namespace Proelium.Ecs;

public abstract class Query
{
    public required EntityManager EntityManager { get; init; }

    public abstract IEnumerable<Type> GetComponentTypes();
    /// <summary>
    /// Components are updated on the entity that this component references.
    /// </summary>
    public abstract void OnAddComponent(Component component);
    /// <summary>
    /// Do not rely on components being updated for the entity that this component references.
    /// </summary>
    public abstract void OnRemoveComponent(Component component);
}

internal class QueryManager
{
    internal required EntityManager EntityManager { get; init; }
    // <query type, query>
    private readonly Dictionary<Type, Query> queries = new();
    // <component type, List<Query>>
    private readonly Dictionary<Type, List<Query>> subscriptions = new();

    internal void OnAddComponent(Component component)
    {
        if (subscriptions.TryGetValue(component.GetType(), out var subscribedQueries))
        {
            foreach (var subscribedQuery in subscribedQueries)
            {
                subscribedQuery.OnAddComponent(component);
            }
        }
    }
    internal void OnRemoveComponent(Component component)
    {
        if (subscriptions.TryGetValue(component.GetType(), out var subscribedQueries))
        {
            foreach (var subscribedQuery in subscribedQueries)
            {
                subscribedQuery.OnRemoveComponent(component);
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
            T query = new() { EntityManager = EntityManager };

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