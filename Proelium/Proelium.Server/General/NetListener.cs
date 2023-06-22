using LiteNetLib;
using LiteNetLib.Utils;
using Proelium.Server.Data;
using Proelium.Server.Player;
using Proelium.Shared;
using System.Net;
using System.Net.Sockets;

namespace Proelium.Server.General;

public class NetListener : INetEventListener
{
    public required Time Time { get; init; }
    public required Pools Pools { get; init; }
    public required Events Events { get; init; }
    public required Players Players { get; init; }
    public required NetPacketProcessor PacketProcessor { get; init; }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        var connectionData = Pools.Rent<ConnectionData>();
        try
        {
            connectionData.Deserialize(request.Data);
        }
        catch
        {
            Time.Log(
                "Invalid ConnectionData",
                $"EndPoint: {request.RemoteEndPoint}"
            );
            return;
        }

        NetPeer peer = request.Accept();
        peer.Tag = new PlayerTag()
        {
            ConnectionData = connectionData
        };
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        Time.Log(
            $"{socketError}",
            $"EndPoint: {endPoint}"
        );
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {

    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        try
        {
            PacketProcessor.ReadAllPackets(reader, peer);
        }
        catch (ParseException)
        {
            peer.Disconnect();

            Time.Log(
                "ParseException",
                $"Id: {peer.Id}",
                $"EndPoint: {peer.EndPoint}",
                $"Name: {peer.GetPlayerTag().ConnectionData.Name}"
            );
        }
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {

    }

    public void OnPeerConnected(NetPeer peer)
    {
        Players.peers.Add(peer);

        Events.peerConnected.Add(Time, new(peer));

        Time.Log(
            "Connection",
            $"Id: {peer.Id}",
            $"EndPoint: {peer.EndPoint}",
            $"Name: {peer.GetPlayerTag().ConnectionData.Name}"
        );
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Players.peers.Remove(peer);

        Events.peerDisconnected.Add(Time, new(peer));

        Time.Log(
            "Disconnection",
            $"Id: {peer.Id}",
            $"EndPoint: {peer.EndPoint}",
            $"Name: {peer.GetPlayerTag().ConnectionData.Name}"
        );
    }
}