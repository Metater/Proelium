using LiteNetLib.Utils;
using Proelium.Shared.Packets.Core;
using System.Numerics;

namespace Proelium.Shared.Types;

public static class NetDataExtensions
{
    public static void RegisterVector2(this INestedTypeRegistrar registrar) => registrar.RegisterNestedType(Put, GetVector2);
    public static void Put(this NetDataWriter writer, Vector2 value)
    {
        writer.Put(value.X);
        writer.Put(value.Y);
    }
    public static Vector2 GetVector2(this NetDataReader reader)
    {
        return new(reader.GetFloat(), reader.GetFloat());
    }

    public static void RegisterVector3(this INestedTypeRegistrar registrar) => registrar.RegisterNestedType(Put, GetVector3);
    public static void Put(this NetDataWriter writer, Vector3 value)
    {
        writer.Put(value.X);
        writer.Put(value.Y);
        writer.Put(value.Z);
    }
    public static Vector3 GetVector3(this NetDataReader reader)
    {
        return new(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
    }
}