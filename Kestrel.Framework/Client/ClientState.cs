using System.Collections.Concurrent;
using System.Numerics;
using GlmSharp;
using Kestrel.Framework.Client.Graphics;
using Kestrel.Framework.Client.Graphics.Buffers;
using Kestrel.Framework.Client.Graphics.Camera;
using Kestrel.Framework.Networking.Packets;
using Kestrel.Framework.Networking.Packets.C2S;
using Kestrel.Framework.Utils;
using KestrelWorld = Kestrel.Framework.World.World;
using LiteNetLib;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using ArchWorld = Arch.Core.World;
using ArchEntity = Arch.Core.Entity;
using Kestrel.Framework.Entity.Components;
using Arch.Core;
using Arch.Core.Extensions;
using Kestrel.Framework.Entity;

public enum ClientStatus
{
    Connecting,
    Connected,
    Disconnected
}

public class ClientState
{
    public ClientStatus status = ClientStatus.Connecting;
    public Kestrel.Framework.Client.Graphics.Window Window;
    public Camera Camera;
    public ArchEntity Player;
    public string PlayerName;
    public ArchWorld Entities;
    public ConcurrentDictionary<Guid, ArchEntity> NetworkableEntities = [];
    public ConcurrentDictionary<Guid, ArchEntity> ServerIdToEntity = [];
    public NetPeer NetServer;
    public KestrelWorld World;
    public ConcurrentDictionary<Vector3I, ChunkMesh> ChunkMeshes = [];
    public ChunkMeshManager ChunkMeshManager = new();
    public int RenderDistance = 12;
    public HashSet<Vector3I> RequestedChunks = [];
    public HashSet<Vector3I> RequestedChunksQueue = [];
    public Profiler Profiler = new();

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public ClientState(GL gl, IWindow silkWindow)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        Window = new(gl, 800, 600);
        silkWindow.FramebufferResize += @Window.OnResize;

        Camera = new ThirdPersonCamera(this);
    }

    public void RequestChunk(Vector3I chunkPos)
    {
        if (RequestedChunks.Contains(chunkPos))
            return;
        RequestedChunks.Add(chunkPos);
        RequestedChunksQueue.Add(chunkPos);
    }

    public void RequestChunksFromQueue()
    {
        if (RequestedChunksQueue.Count == 0 || NetServer == null) return;

        if (!Player.Has<Location>())
            throw new Exception("Player does not have required Location component.");

        Location playerLocation = Player.Get<Location>();

        World.WorldToChunk(
            (int)playerLocation.X, (int)playerLocation.Y, (int)playerLocation.Z,
            out var playerChunk, out _);

        var queueSortedByDistance = new List<Vector3I>(RequestedChunksQueue);
        queueSortedByDistance.Sort((a, b) =>
        {
            float aDistance = LocationUtil.HorizontallyWeightedDistance(playerChunk.ToVector3(), a.ToVector3());
            float bDistance = LocationUtil.HorizontallyWeightedDistance(playerChunk.ToVector3(), b.ToVector3());
            return aDistance.CompareTo(bDistance);
        });


        int batchCount = Math.Min(8, queueSortedByDistance.Count);
        NetServer.Send(IPacket.Serialize(new C2SChunkRequest
        {
            ChunkCount = batchCount,
            Chunks = [.. queueSortedByDistance.Take(batchCount)]
        }), DeliveryMethod.ReliableOrdered);

        foreach (var chunkPos in queueSortedByDistance.Take(batchCount))
        {
            RequestedChunksQueue.Remove(chunkPos);
        }
    }
}