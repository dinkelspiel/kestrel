using System.Collections.Concurrent;
using Kestrel.Framework.World;
using LiteNetLib;
using ArchWorld = Arch.Core.World;
using ArchEntity = Arch.Core.Entity;
using Kestrel.Framework.Entity.Components;
using Arch.Core.Extensions;
using Kestrel.Framework.Networking.Packets.S2C;
using Kestrel.Framework.Entity;
using System.Net;
using Kestrel.Framework.Networking.Packets;

namespace Kestrel.Framework.Server;

public class ServerState
{
    public ConcurrentDictionary<string, ArchEntity> PlayersByName = [];
    public ConcurrentDictionary<NetPeer, ArchEntity> PlayersByConnection = [];
    public NetManager NetServer { get; set; }
    public World.World World;
    public ArchWorld Entities;
    public ConcurrentDictionary<Guid, ArchEntity> NetworkableEntities = [];

    public INetworkableComponent[] GetNetworkableComponents(ArchEntity entity)
    {
        List<INetworkableComponent> serializedComponents = [];
        var entityComponents = Entities.GetAllComponents(entity);

        foreach (var _component in entityComponents)
        {
            if (_component is INetworkableComponent component)
            {
                serializedComponents.Add(component);
            }
        }

        return [.. serializedComponents];
    }

    public void SpawnEntity(int x, int y, int z)
    {
        var guid = Guid.NewGuid();
        ArchEntity entity = Entities.Create(new ServerId(guid), new Location(World, x, y + 20, z), new Velocity(0, 0, 0), new Physics(), new Collider(), new EntityAi(new EntityIdle()));
        NetworkableEntities.TryAdd(guid, entity);

        var spawnPacket = new S2CBroadcastEntitySpawn
        {
            ServerId = guid,
            Components = GetNetworkableComponents(entity)
        };
        NetServer.SendToAll(IPacket.Serialize(spawnPacket), DeliveryMethod.Unreliable);
    }
}