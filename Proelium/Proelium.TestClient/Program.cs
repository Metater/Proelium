//using LiteNetLib;
//using LiteNetLib.Utils;
//using Proelium.Shared;
//using Proelium.Shared.Packets;
//using Proelium.Shared.Packets.Core;
//using System.Diagnostics;

Console.WriteLine("Hello, World!");

Proelium.Ecs.Test.Run();

//EventBasedNetListener listener = new();
//NetManager client = new(listener);
//NetDataWriter writer = new();
//NetPacketProcessor processor = new();

//ClientPackets packets = new(processor, client);

//ConnectionData connectionData = new()
//{
//    Name = "TooManyTransistors"
//};

//connectionData.Serialize(writer);

//client.Start();

//Stopwatch testPacketTimer = Stopwatch.StartNew();

//while (!Console.KeyAvailable)
//{
//    while (client.FirstPeer == null)
//    {
//        client.Connect("localhost", 7777, writer);
//        Thread.Sleep(500);
//    }

//    client.PollEvents();

//    if (testPacketTimer.Elapsed.TotalSeconds > 2)
//    {
//        testPacketTimer.Restart();

//        var testPacket = packets.Rent<TestPacket>()
//            .Reset(420, new(69, 69));
//        packets.Send(autoRecycle: true, testPacket);
//    }

//    Thread.Sleep(17);
//}

//client.Stop();

Console.WriteLine("Press any key to close this window . . .");
Console.ReadKey();