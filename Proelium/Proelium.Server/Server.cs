using LiteNetLib;
using LiteNetLib.Utils;
using Proelium.Server.Data;
using Proelium.Server.General;
using Proelium.Server.Player;
using Proelium.Shared.Packets;
using Proelium.Shared.Packets.Core;

namespace Proelium.Server;

public class Server
{
    public required Context Ctx { get; init; }

    public static Context DefaultContext()
    {
        int port = 7777;
        Time time = new()
        {
            TicksPerSecond = 60
        };
        Pools pools = new();
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

        ServerPackets packets = new(packetProcessor, netManager);
        
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

                // Once per second
                if (Ctx.Time.TickId % Ctx.Time.TicksPerSecond == 0)
                {
                    Console.Title = $"TPS: {Ctx.Time.TicksPerSecond} | Uptime: {(ulong)Ctx.Time.Now}s | Tick Id: {Ctx.Time.TickId} | Time Per Tick: {(Ctx.Time.Now - Ctx.Time.TickTime) * 1000.0:0.000}ms";
                }
            }

            Thread.Sleep(1);
        }

        Stop();
    }

    private void Start()
    {
        Ctx.NetManager.Start(Ctx.Port);
        Ctx.Log("Started");
    }

    private void Tick()
    {
        if (Ctx.Time.TickId != 0)
        {
            Ctx.NetManager.PollEvents();
        }
        else
        {
            Ctx.Log("Skipped NetManager.PollEvents() on first tick");
        }

        foreach ((TestPacket testPacket, NetPeer peer) in Ctx.Packets.Receive<TestPacket>(autoRecycle: true))
        {
            Ctx.Log(
                "Got test packet",
                $"Sender Id: {peer.Id}",
                $"Sender EndPoint: {peer.EndPoint}",
                $"TestInt: {testPacket.TestInt}",
                $"TestVector2: {testPacket.TestVector2}"
            );
        }

        foreach (var disconnection in Ctx.Events.peerDisconnected.Get(Ctx.Time))
        {
            var playerTag = disconnection.Peer.GetPlayerTag();
            Ctx.Pools.Return(playerTag.ConnectionData);
            disconnection.Peer.Tag = null;
        }
    }

    private void Stop()
    {
        Ctx.NetManager.Stop();
        Ctx.Log("Stopped");
    }
}