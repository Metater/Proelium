using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proelium.Server.Collections;

public record struct StructPacket(object Packet, NetPeer Peer)
{
    public bool ExtendLife { get; set; } = false;
}