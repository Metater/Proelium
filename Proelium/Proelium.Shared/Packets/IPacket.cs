using LiteNetLib.Utils;

namespace Proelium.Shared.Packets;

public interface IPacket
{
    public virtual void RegisterNestedTypes(NetPacketProcessor processor) { }
}