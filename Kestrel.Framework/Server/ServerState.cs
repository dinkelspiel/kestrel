using System.Collections.Concurrent;
using Kestrel.Framework.Server.Player;
using Kestrel.Framework.World;
using LiteNetLib;
using ArchWorld = Arch.Core.World;
using ArchEntity = Arch.Core.Entity;

namespace Kestrel.Framework.Server;

public class ServerState
{
    public ConcurrentDictionary<string, ArchEntity> PlayersByName = [];
    public ConcurrentDictionary<NetPeer, ArchEntity> PlayersByConnection = [];
    public NetManager NetServer { get; set; }
    public World.World World;
    public ArchWorld Entities;
    public ConcurrentDictionary<Guid, ArchEntity> NetworkableEntities = [];

    public void SpawnEntity()
    {

    }
}