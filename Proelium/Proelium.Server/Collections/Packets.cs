using LiteNetLib;
using LiteNetLib.Utils;
using Proelium.Shared.Collections;

namespace Proelium.Server.Collections;

public class Packets
{
    private readonly Dictionary<Type, List<ValueTuple<object, NetPeer>>> lists = new();

    public IEnumerable<ValueTuple<T, NetPeer>> Get<T>(NetPacketProcessor packetProcessor, Pools pools, bool returnToPool) where T : class, new()
    {
        if (!lists.TryGetValue(typeof(T), out var list))
        {
            list = new();
            packetProcessor.Subscribe((T packet, NetPeer peer) =>
            {
                list.Add((packet, peer));
            }, pools.Get<T>);
        }

        foreach ((object packet, NetPeer peer) in list)
        {
            yield return ((T)packet, peer);
        }

        if (!returnToPool)
        {
            yield break;
        }

        foreach ((object packet, _) in list)
        {
            pools.Return(packet);
        }
    }
}