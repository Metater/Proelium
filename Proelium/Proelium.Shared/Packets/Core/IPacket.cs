namespace Proelium.Shared.Packets.Core;

public interface IPacket
{
    public void RegisterNestedTypes(INestedTypeRegistrar registrar);
}