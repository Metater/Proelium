//using LiteNetLib;
//using LiteNetLib.Utils;
//using System.Collections.Generic;

//namespace Proelium.Shared.Packets.Client;

//public class StructPacketHandler<T> where T : struct, INetSerializable
//{
//    private readonly NetPacketProcessor packetProcessor;
//    private readonly NetManager netManager;
//    private readonly NetDataWriter writer;

//    private readonly Queue<T> unreceived = new();

//    public StructPacketHandler(NetPacketProcessor packetProcessor, NetManager netManager, NetDataWriter writer)
//    {
//        this.packetProcessor = packetProcessor;
//        this.netManager = netManager;
//        this.writer = writer;

//        packetProcessor.SubscribeNetSerializable(unreceived.Enqueue, () => default(T));
//    }

//    public IEnumerable<T> Receive()
//    {
//        while (unreceived.TryDequeue(out var packet))
//        {
//            yield return packet;
//        }
//    }

//    public bool Send(ref T packet, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
//    {
//        writer.Reset();
//        packetProcessor.WriteNetSerializable(writer, ref packet);
//        var server = netManager.FirstPeer;
//        if (server != null)
//        {
//            server.Send(writer, deliveryMethod);
//            return true;
//        }
//        return false;
//    }
//}