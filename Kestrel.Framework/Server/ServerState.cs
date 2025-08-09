using System.Collections.Concurrent;
using Kestrel.Framework.Server.Player;
using Kestrel.Framework.World;
using LiteNetLib;

namespace Kestrel.Framework.Server;

public class ServerState
{
    public ConcurrentDictionary<String, ServerPlayer> PlayersByName = [];
    public ConcurrentDictionary<NetPeer, ServerPlayer> PlayersByConnection = [];
    public NetManager NetServer { get; set; }
    public World.World World;
}