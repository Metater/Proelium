using LiteNetLib;
using LiteNetLib.Utils;
using Proelium.Server.Collections;
using Proelium.Server.General;
using Proelium.Shared;
using System.Net;
using System.Net.Sockets;

namespace Proelium.Server;

public class NetListener : INetEventListener
{
    public required Time Time { get; init; }
    public required Pools Pools { get; init; }
    public required Events Events { get; init; }
    public required Players Players { get; init; }
    public required NetPacketProcessor PacketProcessor { get; init; }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        if (Time.TickId < 1)
        {
            request.Reject();
            return;
        }

        var packet = Pools.Get<ConnectionDataPacket>();
        packet.Deserialize(request.Data);
        NetPeer peer = request.Accept();
        Players.connectionDataPackets[peer.Id] = packet;
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        PacketProcessor.ReadAllPackets(reader, peer);
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        
    }

    public void OnPeerConnected(NetPeer peer)
    {
        Players.peers.Add(peer);

        Events.onPeerConnected.Add(Time, new(peer));
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        var packet = Players.connectionDataPackets[peer.Id];
        Players.connectionDataPackets.Remove(peer.Id);
        Pools.Return(packet);

        Players.peers.Remove(peer);

        Events.onPeerDisconnected.Add(Time, new(peer));
    }
}