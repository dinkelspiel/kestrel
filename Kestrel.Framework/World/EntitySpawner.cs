namespace Kestrel.Framework.World;

using System.Numerics;
using ArchEntity = Arch.Core.Entity;
using Kestrel.Framework.Server;
using Kestrel.Framework.Entity.Components;
using Kestrel.Framework.Networking.Packets.S2C;
using Kestrel.Framework.Networking.Packets;
using LiteNetLib;

public class EntitySpawner(ServerState serverState)
{
    private readonly ServerState serverState = serverState;

    public void SpawnEntities(Chunk chunk)
    {
        Random random = new();
        if (random.NextDouble() < 0.2)
        {
            List<Vector3> eligblePositions = [];
            for (int wx = chunk.ChunkX * serverState.World.ChunkSize; wx < (chunk.ChunkX + 1) * serverState.World.ChunkSize; wx++)
            {
                for (int wy = chunk.ChunkY * serverState.World.ChunkSize; wy < (chunk.ChunkY + 1) * serverState.World.ChunkSize; wy++)
                {
                    for (int wz = chunk.ChunkZ * serverState.World.ChunkSize; wz < (chunk.ChunkZ + 1) * serverState.World.ChunkSize; wz++)
                    {
                        if (chunk.GetBlock(wx, wy, wz)?.IsSolid() == true && chunk.GetBlock(wx, wy, wz) == BlockType.Grass && chunk.GetBlock(wx, wy + 1, wz)?.IsSolid() == false)
                        {
                            eligblePositions.Add(new(wx, wy + 1, wz));
                            break;
                        }
                    }
                }
            }

            var firstFive = eligblePositions.OrderBy(x => random.Next()).Take(random.Next(0, 20)).ToList();
            firstFive.ForEach(pos =>
            {
                SpawnEntity((int)pos.X, (int)pos.Y, (int)pos.Z);
            });
        }
    }

    public void SpawnEntity(int x, int y, int z)
    {
        var guid = Guid.NewGuid();
        ArchEntity entity = serverState.Entities.Create(new ServerId(guid), new Location(serverState.World, x, y + 20, z), new Velocity(0, 0, 0), new Physics(), new Collider(), new EntityAi(new EntityIdle()));
        serverState.NetworkableEntities.TryAdd(guid, entity);

        var spawnPacket = new S2CBroadcastEntitySpawn
        {
            ServerId = guid,
            Components = serverState.GetNetworkableComponents(entity)
        };
        serverState.NetServer.SendToAll(IPacket.Serialize(spawnPacket), DeliveryMethod.Unreliable);
    }
}