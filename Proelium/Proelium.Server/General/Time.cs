using System.Diagnostics;
using System.Text;

namespace Proelium.Server.General;

public class Time
{
    public required ulong TicksPerSecond { get; init; }

    private readonly Stopwatch stopwatch = new();
    private ulong nextTickId = 0;

    public double Now => stopwatch.Elapsed.TotalSeconds;
    public double TickTime { get; private set; }
    public ulong TickId { get; private set; }

    private readonly StringBuilder sb = new();

    public void Start()
    {
        stopwatch.Start();
    }

    public bool ShouldTick()
    {
        bool shouldTick = Now * TicksPerSecond > nextTickId;

        if (shouldTick)
        {
            TickTime = Now;
            TickId = nextTickId++;
        }

        return shouldTick;
    }

    public void Log(string message, params string[] lines)
    {
        sb.Clear();

        double time = TickTime;
        ulong tickId = TickId;

        string caller = new StackTrace().GetFrame(1)!.GetMethod()!.Name;
        sb.AppendLine($"[{(ulong)time}s] [{tickId}t] [{caller}] {message}");
        foreach (var line in lines)
        {
            sb.AppendLine($"\t{line}");
        }

        Console.Write(sb.ToString());
    }
}