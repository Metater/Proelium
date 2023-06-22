using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;

namespace Proelium.Shared.Packets.Core;

public class ServerPackets : INestedTypeRegistrar
{
    private readonly NetPacketProcessor packetProcessor;
    private readonly NetManager netManager;
    private readonly int poolCapacity;

    private readonly Dictionary<Type, Queue<ValueTuple<IPacket, NetPeer>>> queues = new();
    private readonly Dictionary<Type, Stack<IPacket>> pools = new();
    private readonly HashSet<Type> registeredPacketTypes = new();
    private readonly HashSet<Type> registeredNestedTypes = new();
    private readonly NetDataWriter writer = new();

    public ServerPackets(NetPacketProcessor packetProcessor, NetManager netManager, int poolCapacity = 100)
    {
        this.packetProcessor = packetProcessor;
        this.netManager = netManager;
        this.poolCapacity = poolCapacity;
    }

    /// <summary>
    /// Only enable auto recycle if you are certain you have no lasting references to received packets.
    /// </summary>
    public IEnumerable<ValueTuple<T, NetPeer>> Receive<T>(bool autoRecycle) where T : class, IPacket, new()
    {
        Type type = typeof(T);
        if (!queues.TryGetValue(type, out var queue))
        {
            if (!registeredPacketTypes.Contains(type))
            {
                registeredPacketTypes.Add(type);
                T packet = Rent<T>();
                packet.RegisterNestedTypes(this);
                Return(packet);
            }

            queue = new();
            queues[type] = queue;

            packetProcessor.Subscribe((T packet, NetPeer peer) =>
            {
                queue.Enqueue((packet, peer));
            }, Rent<T>);
        }

        while (queue.TryDequeue(out var pair))
        {
            yield return ((T)pair.Item1, pair.Item2);

            if (autoRecycle)
            {
                UncheckedReturn(pair.Item1);
            }
        }
    }

    /// <summary>
    /// Only enable auto recycle if you are certain you have no lasting references to sent packets.
    /// </summary>
    public void Send<T>(bool autoRecycle, T packet, NetPeer peer, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) where T : class, IPacket, new()
    {
        Type type = typeof(T);
        if (!registeredPacketTypes.Contains(type))
        {
            registeredPacketTypes.Add(type);
            packet.RegisterNestedTypes(this);
        }

        writer.Reset();
        packetProcessor.Write(writer, packet);

        peer.Send(writer, deliveryMethod);

        if (autoRecycle)
        {
            Return(packet);
        }
    }

    /// <summary>
    /// Only enable auto recycle if you are certain you have no lasting references to sent packets.
    /// </summary>
    public void SendToAll<T>(bool autoRecycle, T packet, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) where T : class, IPacket, new()
    {
        Type type = typeof(T);
        if (!registeredPacketTypes.Contains(type))
        {
            registeredPacketTypes.Add(type);
            packet.RegisterNestedTypes(this);
        }

        writer.Reset();
        packetProcessor.Write(writer, packet);

        netManager.SendToAll(writer, deliveryMethod);

        if (autoRecycle)
        {
            Return(packet);
        }
    }

    public T Rent<T>() where T : class, IPacket, new()
    {
        if (pools.TryGetValue(typeof(T), out var pool) && pool.TryPop(out var packet))
        {
            return (T)packet;
        }
        return new();
    }

    /// <summary>
    /// Only return a packet when you are certain sure you have no lasting references to it.
    /// </summary>
    public void Return<T>(T packet) where T : class, IPacket, new()
    {
        if (packet == null)
        {
            throw new ArgumentNullException(nameof(packet));
        }

        Type type = typeof(T);
        if (!pools.TryGetValue(type, out var pool))
        {
            pool = new();
            pools[type] = pool;
        }

        if (pool.Count < poolCapacity)
        {
            pool.Push(packet);
        }
    }

    private void UncheckedReturn(IPacket packet)
    {
        Type type = packet.GetType();
        if (!pools.TryGetValue(type, out var pool))
        {
            pool = new();
            pools[type] = pool;
        }

        if (pool.Count < poolCapacity)
        {
            pool.Push(packet);
        }
    }

    void INestedTypeRegistrar.RegisterNestedType<T>()
    {
        Type type = typeof(T);
        if (!registeredNestedTypes.Contains(type))
        {
            registeredNestedTypes.Add(type);
            packetProcessor.RegisterNestedType<T>();
        }
    }

    void INestedTypeRegistrar.RegisterNestedType<T>(Action<NetDataWriter, T> writeDelegate, Func<NetDataReader, T> readDelegate)
    {
        Type type = typeof(T);
        if (!registeredNestedTypes.Contains(type))
        {
            registeredNestedTypes.Add(type);
            packetProcessor.RegisterNestedType(writeDelegate, readDelegate);
        }
    }

    void INestedTypeRegistrar.RegisterNestedType<T>(Func<T> constructor)
    {
        Type type = typeof(T);
        if (!registeredNestedTypes.Contains(type))
        {
            registeredNestedTypes.Add(type);
            packetProcessor.RegisterNestedType(constructor);
        }
    }
}