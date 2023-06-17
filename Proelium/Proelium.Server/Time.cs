using System.Diagnostics;

namespace Proelium.Server;

public class Time
{
    public readonly double ticksPerSecond;

    private readonly Stopwatch stopwatch = new();
    private ulong nextTickId = 0;

    public double Now => stopwatch.Elapsed.TotalSeconds;
    public double TickTime { get; private set; }
    public ulong TickId { get; private set; }

    public Time(double ticksPerSecond)
    {
        this.ticksPerSecond = ticksPerSecond;
    }

    public void Start()
    {
        stopwatch.Start();
    }

    public bool ShouldTick()
    {
        bool shouldTick = Now * ticksPerSecond > nextTickId;

        if (shouldTick)
        {
            TickTime = Now;
            TickId = nextTickId++;
        }

        return shouldTick;
    }
}