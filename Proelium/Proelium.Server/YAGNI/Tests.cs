//using LiteNetLib;
//using LiteNetLib.Utils;
//using Proelium.Shared.Packets;
//using Proelium.Shared.Types;

//namespace Proelium.Server.General;

//public class Tests
//{
//    public required Context Ctx { get; init; }

//    public void Run()
//    {
//        Thread.Sleep(200);

//        EventBasedNetListener listener = new();
//        NetManager client = new(listener);
//        client.Start();
//        client.Connect("localhost", Ctx.Port, new NetDataWriter());

//        Thread.Sleep(100);

//        NetDataWriter writer = new();
//        NetPacketProcessor packetProcessor = new();

//        packetProcessor.RegisterNestedType(NestedTypeRegistrarExtensions.Put, NestedTypeRegistrarExtensions.GetVector2);

//        while (!Console.KeyAvailable)
//        {
//            client.PollEvents();

//            writer.Reset();
//            packetProcessor.Write(writer, new TestPacket()
//            {
//                TestPosition = new(10, 10),
//                TestString = "Hello, World!"
//            });
//            client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);

//            writer.Reset();
//            packetProcessor.Write(writer, new TestPacket2()
//            {
//                TestPosition = new(10, 10),
//                TestString = "Hello, World!"
//            });
//            client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);

//            Thread.Sleep(15);
//        }

//        client.Stop();
//    }
//}