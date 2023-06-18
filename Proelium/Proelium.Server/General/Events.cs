using LiteNetLib;

namespace Proelium.Server;

public partial class Events
{
    public record struct PeerConnected(NetPeer Peer);
    public record struct PeerDisconnected(NetPeer Peer);
}