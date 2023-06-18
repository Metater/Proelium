using LiteNetLib;
using Proelium.Shared;

namespace Proelium.Server.Collections;

public class Players
{
    // <NetPeer.Id, _>
    public readonly Dictionary<int, ConnectionDataPacket> connectionDataPackets = new();
    public readonly List<NetPeer> peers = new();
}