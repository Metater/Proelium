using LiteNetLib;
using LiteNetLib.Utils;
using Proelium.Server;
using Proelium.Shared.Packets;

CancellationTokenSource cts = new();
Console.CancelKeyPress += (s, e) =>
{
    cts.Cancel();
    e.Cancel = true;
};

Console.WriteLine("Hello, World!");

Server server = new()
{
    Ctx = Server.DefaultContext()
};

Task serverTask = Task.Run(() => server.Run(cts.Token));

#region Testing
await Task.Delay(200);

EventBasedNetListener listener = new();
NetManager client = new(listener);
client.Start();
client.Connect("localhost", server.Ctx.Port, new NetDataWriter());

await Task.Delay(100);

NetDataWriter writer = new();
NetPacketProcessor packetProcessor = new();

while (!Console.KeyAvailable)
{
    client.PollEvents();

    writer.Reset();
    packetProcessor.Write(writer, new TestPacket()
    {
        TestPosition = new(10, 10),
        TestString = "Hello, World!"
    });
    client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);

    // Thread.Sleep(1);
}

client.Stop();
#endregion

await serverTask;