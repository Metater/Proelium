using LiteNetLib.Utils;
using Proelium.Shared.Types;

namespace Proelium.Shared.Packets;

public class TestPacket : IPacket
{
    public Vector2 TestPosition { get; set; }
    public string TestString { get; set; } = "";

    public void RegisterNestedTypes(NetPacketProcessor processor)
    {
        processor.RegisterNestedType<Vector2>();
    }
}