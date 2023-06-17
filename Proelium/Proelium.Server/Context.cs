using LiteNetLib;
using Proelium.Server.Collections;
using Proelium.Server.Events;
using Proelium.Shared.Collections;

namespace Proelium.Server;

public class Context
{
    public readonly Pools pools = new(100);
    public readonly StructEvent<PeerConnected> peerConnectedEvent = new();
    public readonly StructEvent<PeerDisconnected> peerDisconnectedEvent = new();

    public readonly Time time = new(60);
    public readonly Players players = new();
}