namespace Proelium.Server.General;

public class Pools
{
    private readonly int capacity;
    private readonly Dictionary<Type, Stack<object>> pools = new();

    public Pools(int capacity = 100)
    {
        this.capacity = capacity;
    }

    public T Rent<T>() where T : new()
    {
        if (pools.TryGetValue(typeof(T), out var pool) && pool.TryPop(out var item))
        {
            return (T)item;
        }
        return new();
    }

    public bool TryRent<T>(out T item)
    {
        if (pools.TryGetValue(typeof(T), out var pool) && pool.TryPop(out var itemObj))
        {
            item = (T)itemObj;
            return true;
        }

        item = default!;
        return false;
    }

    /// <summary>
    /// Only return a packet when you are certain you have no lasting references to it.
    /// </summary>
    public void Return<T>(T item) where T : new()
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        Type type = typeof(T);
        if (!pools.TryGetValue(type, out var pool))
        {
            pool = new();
            pools[type] = pool;
        }

        if (pool.Count < capacity)
        {
            pool.Push(item);
        }
    }

    /// <summary>
    /// Only return a packet when you are certain you have no lasting references to it.
    /// </summary>
    public void Return(object item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        Type type = item.GetType();
        if (!pools.TryGetValue(type, out var pool))
        {
            pool = new();
            pools[type] = pool;
        }

        if (pool.Count < capacity)
        {
            pool.Push(item);
        }
    }
}