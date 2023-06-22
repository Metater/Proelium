using LiteNetLib;
using LiteNetLib.Utils;
using Proelium.Shared;
using Proelium.Shared.Packets;
using Proelium.Shared.Packets.Core;
using System.Diagnostics;
using System.Numerics;

Console.WriteLine("Hello, World!");

EventBasedNetListener listener = new();
NetManager client = new(listener);
NetDataWriter writer = new();
NetPacketProcessor processor = new();

ClientPackets packets = new(processor, client);

ConnectionData connectionData = new()
{
    Name = "TooManyTransistors"
};

connectionData.Serialize(writer);

client.Start();

Stopwatch testPacketTimer = Stopwatch.StartNew();

while (!Console.KeyAvailable)
{
    while (client.FirstPeer == null)
    {
        client.Connect("localhost", 7777, writer);
        Thread.Sleep(500);
    }

    client.PollEvents();

    if (testPacketTimer.Elapsed.TotalSeconds > 2)
    {
        testPacketTimer.Restart();

        //var testPacket = packets.Rent<TestPacket>();
        //testPacket.TestInt = 420;
        //testPacket.TestVector2 = new Vector2(69, 69);
        //packets.Send(autoRecycle: true, testPacket);
    }

    Thread.Sleep(17);
}

client.Stop();