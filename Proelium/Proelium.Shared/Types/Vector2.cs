using LiteNetLib.Utils;

namespace Proelium.Shared.Types;

public struct Vector2 : INetSerializable
{
    public System.Numerics.Vector2 Inner { get; private set; }

    public Vector2()
    {

    }

    public Vector2(float x, float y)
    {
        Inner = new(x, y);
    }

    public void Deserialize(NetDataReader reader)
    {
        float x = reader.GetFloat();
        float y = reader.GetFloat();
        Inner = new(x, y);
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Inner.X);
        writer.Put(Inner.Y);
    }
}