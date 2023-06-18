using Proelium.Server.Collections;

namespace Proelium.Server;

public partial class Events
{
	public readonly StructEvent<Events.PeerConnected> onPeerConnected = new();
	public readonly StructEvent<Events.PeerDisconnected> onPeerDisconnected = new();
}
