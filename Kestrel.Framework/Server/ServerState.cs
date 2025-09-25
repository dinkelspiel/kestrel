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
using Kestrel.Framework.Utils;

namespace Kestrel.Framework.Server;

public class ServerState
{
    public ConcurrentDictionary<string, ArchEntity> PlayersByName = [];
    public ConcurrentDictionary<NetPeer, ArchEntity> PlayersByConnection = [];
    public required NetManager NetServer { get; set; }
    public required World.World World;
    public required ArchWorld Entities;
    public required EntitySpawner EntitySpawner;
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

    public Chunk GetChunkOrGenerate(int cx, int cy, int cz, out bool generated)
    {
        Vector3I chunkPos = new(cx, cy, cz);
        if (World.Chunks.TryGetValue(chunkPos, out var chunk))
        {
            generated = false;
            return chunk;
        }

        Chunk newChunk = new(World, cx, cy, cz);
        newChunk.Generate();
        Random random = new();
        if (random.NextDouble() < 0.2)
        {
            Console.WriteLine("Spawning entities in chunk {0},{1},{2}", cx, cy, cz);
            EntitySpawner.SpawnEntities(newChunk);
        }

        World.Chunks.TryAdd(chunkPos, newChunk);
        generated = true;
        return newChunk;
    }
}