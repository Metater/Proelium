using LiteNetLib;
using LiteNetLib.Utils;
using Proelium.Server.Collections;
using Proelium.Server.General;
using Proelium.Shared.Packets;

namespace Proelium.Server;

public class Server
{
    public required Context Ctx { get; init; }

    public static Context DefaultContext()
    {
        int port = 7777;
        Time time = new(60);
        Pools pools = new(100);
        Events events = new();
        Players players = new();

        NetPacketProcessor packetProcessor = new();

        NetListener netListener = new()
        {
            Time = time,
            Pools = pools,
            Events = events,
            Players = players,
            PacketProcessor = packetProcessor
        };

        NetManager netManager = new(netListener)
        {
            AutoRecycle = true,
            IPv6Enabled = true,
            UseNativeSockets = true
        };

        Packets packets = new()
        {
            NetManager = netManager,
            Pools = pools,
            PacketProcessor = packetProcessor
        };
        
        return new()
        {
            Port = port,
            Time = time,
            Pools = pools,
            Events = events,
            Players = players,
            NetManager = netManager,
            Packets = packets
        };
    }

    public void Run(CancellationToken cancellationToken)
    {
        Start();

        Ctx.Time.Start();

        while (!cancellationToken.IsCancellationRequested)
        {
            while (Ctx.Time.ShouldTick())
            {
                Tick();

                if (Ctx.Time.TickId % Ctx.Time.ticksPerSecond == 0)
                {
                    Console.Title = $"TPS: {Ctx.Time.ticksPerSecond} | Uptime: {(ulong)Ctx.Time.Now}s | Tick Id: {Ctx.Time.TickId} | Time Per Tick: {(Ctx.Time.Now - Ctx.Time.TickTime) * 1000.0:0.000}ms";
                }
            }

            Thread.Sleep(1);
        }

        Stop();
    }

    private void Start()
    {
        Ctx.NetManager.Start(Ctx.Port);
    }

    private void Tick()
    {
        Ctx.NetManager.PollEvents();

        foreach (var connection in Ctx.Events.onPeerConnected.Get(Ctx.Time))
        {
            Console.WriteLine($"Player connected, id: {connection.Peer.Id}");
        }

        foreach ((var packet, NetPeer peer) in Ctx.Packets.Receive<TestPacket>())
        {
            //Console.WriteLine($"Got packet from {peer.EndPoint}");
            //Console.WriteLine($"Position {packet.TestPosition}");
            //Console.WriteLine($"String {packet.TestString}");
        }
    }

    private void Stop()
    {
        Ctx.NetManager.Stop();
    }
}