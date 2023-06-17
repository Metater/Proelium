using LiteNetLib;
using Proelium.Shared;

namespace Proelium.Server;

public class Players
{
    public readonly Dictionary<int, ConnectionDataPacket> connectionDataPackets = new(); // <NetPeer.Id, packet>
    public readonly List<NetPeer> peers = new();
}