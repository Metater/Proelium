using LiteNetLib;
using Proelium.Shared;
using System.Net;
using System.Net.Sockets;

namespace Proelium.Server;

public class NetListener : INetEventListener, IRequireContext
{
    public required Context Ctx { get; init; }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        if (Ctx.time.TickId < 1)
        {
            request.Reject();
            return;
        }

        var packet = Ctx.pools.Get<ConnectionDataPacket>();
        packet.Deserialize(request.Data);
        NetPeer peer = request.Accept();
        Ctx.players.connectionDataPackets[peer.Id] = packet;
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        
    }

    public void OnPeerConnected(NetPeer peer)
    {
        Ctx.players.peers.Add(peer);

        Ctx.peerConnectedEvent.Add(Ctx.time, new(peer));
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        var packet = Ctx.players.connectionDataPackets[peer.Id];
        Ctx.players.connectionDataPackets.Remove(peer.Id);
        Ctx.pools.Return(packet);

        Ctx.players.peers.Remove(peer);

        Ctx.peerDisconnectedEvent.Add(Ctx.time, new(peer));
    }
}