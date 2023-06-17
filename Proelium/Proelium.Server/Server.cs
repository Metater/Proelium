using Microsoft.VisualBasic;

namespace Proelium.Server;

public class Server
{
    private readonly Context ctx;

    private readonly NetListener netListener;

    public Server()
    {
        ctx = Start();

        netListener = new()
        {
            Ctx = ctx
        };
    }

    public void Run(CancellationToken cancellationToken)
    {
        ctx.time.Start();

        while (!cancellationToken.IsCancellationRequested)
        {
            while (ctx.time.ShouldTick())
            {
                Tick();

                if (ctx.time.TickId % ctx.time.ticksPerSecond == 0)
                {
                    Console.Title = $"TPS: {ctx.time.ticksPerSecond} | Uptime: {(ulong)ctx.time.Now}s | Tick Id: {ctx.time.TickId} | Time Per Tick: {(ctx.time.Now - ctx.time.TickTime) * 1000.0:0.000}ms";
                }
            }

            Thread.Sleep(1);
        }

        Stop();
    }

    private static Context Start()
    {
        return new();
    }

    private void Tick()
    {
        Console.WriteLine($"Tick {ctx.time.TickId}");

        if (ctx.time.TickId < 1)
        {
            
        }
        else
        {
            Console.WriteLine("Ready");
        }
    }

    private void Stop()
    {
        Console.WriteLine($"Stopped on tick {ctx.time.TickId}");
    }
}