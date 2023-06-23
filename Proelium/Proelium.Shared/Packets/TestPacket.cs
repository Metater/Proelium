using Proelium.Shared.Packets.Core;
using Proelium.Shared.Types;
using System.Numerics;

namespace Proelium.Shared.Packets;

public class TestPacket : IPacket
{
    public int TestInt { get; set; }
    public Vector2 TestVector2 { get; set; }

    public TestPacket Reset(int testInt, Vector2 testVector2)
    {
        TestInt = testInt;
        TestVector2 = testVector2;
        return this;
    }

    public void RegisterNestedTypes(INestedTypeRegistrar registrar)
    {
        registrar.RegisterVector2();
    }
}