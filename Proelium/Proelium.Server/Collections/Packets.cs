using LiteNetLib;
using LiteNetLib.Utils;
using Proelium.Shared.Packets;

namespace Proelium.Server.Collections;

public class Packets
{
    public required NetManager NetManager { get; init; }
    public required Pools Pools { get; init; }
    public required NetPacketProcessor PacketProcessor { get; init; }

    private readonly Dictionary<Type, List<ValueTuple<IPacket, NetPeer>>> lists = new();
    private readonly HashSet<Type> registeredPackets = new();
    private readonly NetDataWriter writer = new();

    public IEnumerable<ValueTuple<T, NetPeer>> Receive<T>(bool returnToPool = true) where T : class, IPacket, new()
    {
        Type type = typeof(T);
        if (!lists.TryGetValue(type, out var list))
        {
            if (!registeredPackets.Contains(type))
            {
                registeredPackets.Add(type);
                T packet = Pools.Get<T>();
                packet.RegisterNestedTypes(PacketProcessor);
                Pools.Return(packet);
            }

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

        if (!returnToPool)
        {
            list.Clear();
            yield break;
        }

        foreach ((IPacket packet, _) in list)
        {
            Pools.Return(packet);
        }

        list.Clear();
    }

    public void Send<T>(T packet, NetPeer peer, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) where T : class, IPacket, new()
    {
        Type type = typeof(T);
        if (!registeredPackets.Contains(type))
        {
            registeredPackets.Add(type);
            packet.RegisterNestedTypes(PacketProcessor);
        }

        writer.Reset();
        PacketProcessor.Write(writer, packet);
        peer.Send(writer, deliveryMethod);
    }

    public void SendToAll<T>(T packet, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) where T : class, IPacket, new()
    {
        Type type = typeof(T);
        if (!registeredPackets.Contains(type))
        {
            registeredPackets.Add(type);
            packet.RegisterNestedTypes(PacketProcessor);
        }

        writer.Reset();
        PacketProcessor.Write(writer, packet);
        NetManager.SendToAll(writer, deliveryMethod);
    }
}