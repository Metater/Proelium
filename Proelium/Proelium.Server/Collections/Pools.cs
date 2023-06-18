namespace Proelium.Server.Collections;

public class Pools
{
    private readonly int capacity;
    private readonly Dictionary<Type, Stack<object>> pools = new();

    public Pools(int capacity)
    {
        this.capacity = capacity;
    }

    public T Get<T>() where T : new()
    {
        Type type = typeof(T);
        if (!pools.TryGetValue(type, out var stack))
        {
            stack = new();
            pools[type] = stack;
        }

        if (!stack.TryPop(out var item))
        {
            return new T();
        }

        return (T)item;
    }

    public void Return<T>(T item) where T : new()
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        if (!pools.TryGetValue(typeof(T), out var stack))
        {
            return;
        }

        if (stack.Count < capacity)
        {
            stack.Push(item);
        }
    }

    public bool TryGet<T>(out T item)
    {
        if (pools.TryGetValue(typeof(T), out var stack) && stack.TryPop(out var itemObj))
        {
            item = (T)itemObj;
            return true;
        }

        item = default!;
        return false;
    }


    public void Return(object item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        if (!pools.TryGetValue(item.GetType(), out var stack))
        {
            return;
        }

        if (stack.Count < capacity)
        {
            stack.Push(item);
        }
    }
}