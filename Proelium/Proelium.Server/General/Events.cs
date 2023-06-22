using LiteNetLib;
using Proelium.Server.Patterns;

namespace Proelium.Server.General;

public class Events
{
    public record struct PeerConnected(NetPeer Peer);
    public readonly StructEvent<PeerConnected> peerConnected = new();

    public record struct PeerDisconnected(NetPeer Peer);
    public readonly StructEvent<PeerDisconnected> peerDisconnected = new();
}