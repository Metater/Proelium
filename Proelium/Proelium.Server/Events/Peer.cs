using LiteNetLib;

namespace Proelium.Server.Events;

public record struct PeerConnected(NetPeer Peer);
public record struct PeerDisconnected(NetPeer Peer);