using LiteNetLib;

namespace Proelium.Server.Player;

public static class NetPeerExtensions
{
    public static PlayerTag GetPlayerTag(this NetPeer peer)
    {
        return (PlayerTag)peer.Tag;
    }
}
