using Proelium.Server;

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

server.Run(cts.Token);

Console.WriteLine("Press any key to close this window . . .");
Console.ReadKey();