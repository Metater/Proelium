using LiteNetLib;
using Proelium.Server.Collections;

namespace Proelium.Server.General;

public partial class Context
{
    public required int Port { get; init; }
    public required Time Time { get; init; }
    public required Pools Pools { get; init; }
    public required Events Events { get; init; }
    public required Players Players { get; init; }
    public required NetManager NetManager { get; init; }
    public required Packets Packets { get; init; }
}