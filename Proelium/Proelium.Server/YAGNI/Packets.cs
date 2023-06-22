//using LiteNetLib;
//using LiteNetLib.Utils;
//using Proelium.Shared.PacketPatterns;
//using Proelium.Shared.Packets;

//namespace Proelium.Server.Collections;

//public class Packets : INestedTypeRegistrar
//{
//    // May want to add INetSerializable packets

//    public required Pools Pools { get; init; }
//    public required NetPacketProcessor PacketProcessor { get; init; }
//    public required NetManager NetManager { get; init; }

//    private readonly Dictionary<Type, List<ValueTuple<IHaveNestedTypes, NetPeer>>> lists = new();
//    private readonly HashSet<Type> registeredPacketTypes = new();
//    private readonly HashSet<Type> registeredNestedTypes = new();
//    private readonly NetDataWriter writer = new();

//    /// <summary>
//    /// Callers are responsible for returning received packets to a pool. It is safe to do so at any time.
//    /// </summary>
//    public IEnumerable<ValueTuple<T, NetPeer>> Receive<T>() where T : class, IHaveNestedTypes, new()
//    {
//        Type type = typeof(T);
//        if (!lists.TryGetValue(type, out var list))
//        {
//            T packet = Pools.Rent<T>();
//            TryRegisterNestedTypes(type, packet);
//            Pools.Return(packet);

//            list = new();
//            lists[type] = list;

//            PacketProcessor.Subscribe((T packet, NetPeer peer) =>
//            { 
//                list.Add((packet, peer));
//            }, Pools.Rent<T>);
//        }

//        foreach ((IHaveNestedTypes packet, NetPeer peer) in list)
//        {
//            yield return ((T)packet, peer);
//        }

//        list.Clear();
//    }

//    /// <summary>
//    /// Callers are responsible for returning sent packets to a pool. It is safe to do so directly after this is called.
//    /// </summary>
//    public void Send<T>(T packet, NetPeer peer, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) where T : class, IHaveNestedTypes, new()
//    {
//        TryRegisterNestedTypes(typeof(T), packet);
//        Write(packet);
//        peer.Send(writer, deliveryMethod);
//    }

//    /// <summary>
//    /// Callers are responsible for returning sent packets to a pool. It is safe to do so directly after this is called.
//    /// </summary>
//    public void SendToAll<T>(T packet, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) where T : class, IHaveNestedTypes, new()
//    {
//        TryRegisterNestedTypes(typeof(T), packet);
//        Write(packet);
//        NetManager.SendToAll(writer, deliveryMethod);
//    }

//    private void TryRegisterNestedTypes(Type type, IHaveNestedTypes packet)
//    {
//        if (!registeredPacketTypes.Contains(type))
//        {
//            registeredPacketTypes.Add(type);
//            packet.RegisterNestedTypes(this);
//        }
//    }

//    private void Write<T>(T packet) where T : class, IHaveNestedTypes, new()
//    {
//        writer.Reset();
//        PacketProcessor.Write(writer, packet);
//    }

//    void INestedTypeRegistrar.RegisterNestedType<T>()
//    {
//        Type type = typeof(T);
//        if (!registeredNestedTypes.Contains(type))
//        {
//            registeredNestedTypes.Add(type);
//            PacketProcessor.RegisterNestedType<T>();
//        }
//    }

//    void INestedTypeRegistrar.RegisterNestedType<T>(Action<NetDataWriter, T> writeDelegate, Func<NetDataReader, T> readDelegate)
//    {
//        Type type = typeof(T);
//        if (!registeredNestedTypes.Contains(type))
//        {
//            registeredNestedTypes.Add(type);
//            PacketProcessor.RegisterNestedType(writeDelegate, readDelegate);
//        }
//    }

//    void INestedTypeRegistrar.RegisterNestedType<T>(Func<T> constructor)
//    {
//        Type type = typeof(T);
//        if (!registeredNestedTypes.Contains(type))
//        {
//            registeredNestedTypes.Add(type);
//            PacketProcessor.RegisterNestedType(constructor);
//        }
//    }
//}