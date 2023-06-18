using LiteNetLib;
using LiteNetLib.Utils;
using Proelium.Shared.Packets;

namespace Proelium.Server.Collections;

public class Packets
{
    public required Pools Pools { get; init; }
    public required NetPacketProcessor PacketProcessor { get; init; }
    public required NetManager NetManager { get; init; }

    private readonly Dictionary<Type, List<ValueTuple<IPacket, NetPeer>>> lists = new();
    private readonly HashSet<Type> registeredPackets = new();
    private readonly NetDataWriter writer = new();

    public IEnumerable<ValueTuple<T, NetPeer>> Receive<T>() where T : class, IPacket, new()
    {
        Type type = typeof(T);
        if (!lists.TryGetValue(type, out var list))
        {
            T packet = Pools.Get<T>();
            TryRegister(type, packet);
            Pools.Return(packet);

            list = new();
            lists[type] = list;

            PacketProcessor.Subscribe((T packet, NetPeer peer) =>
            { 
                list.Add((packet, peer));
            }, Pools.Get<T>);
        }

        foreach ((IPacket packet, NetPeer peer) in list)
        {
            yield return ((T)packet, peer);
        }

        list.Clear();
    }

    public void Send<T>(T packet, NetPeer peer, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) where T : class, IPacket, new()
    {
        TryRegister(typeof(T), packet);
        Write(packet);
        peer.Send(writer, deliveryMethod);
    }

    public void SendToAll<T>(T packet, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) where T : class, IPacket, new()
    {
        TryRegister(typeof(T), packet);
        Write(packet);
        NetManager.SendToAll(writer, deliveryMethod);
    }

    private void TryRegister(Type type, IPacket packet)
    {
        if (!registeredPackets.Contains(type))
        {
            registeredPackets.Add(type);
            packet.RegisterNestedTypes(PacketProcessor);
        }
    }

    private void Write<T>(T packet) where T : class, IPacket, new()
    {
        writer.Reset();
        PacketProcessor.Write(writer, packet);
    }
}