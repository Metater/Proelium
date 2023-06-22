using LiteNetLib;
using Proelium.Server.Data;
using Proelium.Shared.Packets.Core;
using System.Diagnostics;
using System.Text;

namespace Proelium.Server.General;

public class Context
{
    public required int Port { get; init; }
    public required Time Time { get; init; }
    public required Pools Pools { get; init; }
    public required Events Events { get; init; }
    public required Players Players { get; init; }
    public required NetManager NetManager { get; init; }
    public required ServerPackets Packets { get; init; }

    private readonly StringBuilder sb = new();

    public void Log(string message, params string[] lines)
    {
        sb.Clear();

        double time = Time.TickTime;
        ulong tickId = Time.TickId;

        string caller = new StackTrace().GetFrame(1)!.GetMethod()!.Name;
        sb.AppendLine($"[{(ulong)time}s] [{tickId}t] [{caller}] {message}");
        foreach (var line in lines)
        {
            sb.AppendLine($"\t{line}");
        }

        Console.Write(sb.ToString());
    }
}