using LiteNetLib.Utils;

namespace Proelium.Shared;

public class ConnectionData : INetSerializable
{
    public string Name { get; set; } = "";

    public void Deserialize(NetDataReader reader)
    {
        Name = reader.GetString();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Name);
    }
}