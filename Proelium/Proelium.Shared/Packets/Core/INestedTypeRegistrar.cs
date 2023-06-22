using LiteNetLib.Utils;
using System;

namespace Proelium.Shared.Packets.Core;

public interface INestedTypeRegistrar
{
    public void RegisterNestedType<T>() where T : struct, INetSerializable;
    public void RegisterNestedType<T>(Action<NetDataWriter, T> writeDelegate, Func<NetDataReader, T> readDelegate);
    public void RegisterNestedType<T>(Func<T> constructor) where T : class, INetSerializable;
}