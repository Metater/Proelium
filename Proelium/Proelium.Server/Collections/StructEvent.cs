namespace Proelium.Server.Collections;

public class StructEvent<T> where T : struct
{
    private ulong currentTickId = ulong.MaxValue;
    private readonly List<T> events = new();

    public void Add(Time time, T @event)
    {
        if (time.TickId != currentTickId)
        {
            currentTickId = time.TickId;
            events.Clear();
        }

        events.Add(@event);
    }

    public IEnumerable<T> Get(Time time)
    {
        if (time.TickId != currentTickId)
        {
            return Enumerable.Empty<T>();
        }

        return events;
    }
}