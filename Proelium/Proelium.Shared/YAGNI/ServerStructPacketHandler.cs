//using LiteNetLib;
//using LiteNetLib.Utils;
//using System;
//using System.Collections.Generic;

//namespace Proelium.Shared.Packets.Server;

//public class StructPacketHandler<T> where T : struct, INetSerializable
//{
//    private readonly NetPacketProcessor packetProcessor;
//    private readonly NetManager netManager;
//    private readonly NetDataWriter writer;

//    private readonly Queue<ValueTuple<T, NetPeer>> unreceived = new();

//    public StructPacketHandler(NetPacketProcessor packetProcessor, NetManager netManager, NetDataWriter writer)
//    {
//        this.packetProcessor = packetProcessor;
//        this.netManager = netManager;
//        this.writer = writer;

//        packetProcessor.SubscribeNetSerializable((T packet, NetPeer peer) =>
//        {
//            unreceived.Enqueue((packet, peer));
//        }, () => default);
//    }

//    public IEnumerable<ValueTuple<T, NetPeer>> Receive()
//    {
//        while (unreceived.TryDequeue(out var pair))
//        {
//            yield return pair;
//        }
//    }

//    public void Send(ref T packet, NetPeer peer, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
//    {
//        writer.Reset();
//        packetProcessor.WriteNetSerializable(writer, ref packet);
//        peer.Send(writer, deliveryMethod);
//    }

//    public void SendToAll(ref T packet, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
//    {
//        writer.Reset();
//        packetProcessor.WriteNetSerializable(writer, ref packet);
//        netManager.SendToAll(writer, deliveryMethod);
//    }
//}