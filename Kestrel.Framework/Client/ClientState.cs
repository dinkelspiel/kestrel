using System.Numerics;
using GlmSharp;
using Kestrel.Framework.Client.Graphics;
using Kestrel.Framework.Client.Graphics.Buffers;
using Kestrel.Framework.Client.Graphics.Camera;
using Kestrel.Framework.Networking.Packets;
using Kestrel.Framework.Networking.Packets.C2S;
using Kestrel.Framework.Server.Player;
using Kestrel.Framework.Utils;
using Kestrel.Framework.World;
using LiteNetLib;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

public class ClientState
{
    public Kestrel.Framework.Client.Graphics.Window Window;
    public Camera Camera;
    public ClientPlayer Player;
    public Dictionary<String, ClientPlayer> Players = [];
    public NetPeer NetServer;
    public World World;
    public Dictionary<Vector3I, ChunkMesh> ChunkMeshes = [];
    public int RenderDistance = 6;
    public List<Vector3I> RequestedChunks = [];
    public List<Vector3I> RequestedChunksQueue = [];

    public ClientState(GL gl, IWindow silkWindow)
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

        World.WorldToChunk(
            (int)Player.Location.X, (int)Player.Location.Y, (int)Player.Location.Z,
            out var playerChunk, out _);

        var queueSortedByDistance = new List<Vector3I>(RequestedChunksQueue);
        queueSortedByDistance.Sort((a, b) =>
        {
            float aDistance = LocationUtil.HorizontallyWeightedDistance(playerChunk.ToVector3(), a.ToVector3());
            float bDistance = LocationUtil.HorizontallyWeightedDistance(playerChunk.ToVector3(), b.ToVector3());
            return aDistance.CompareTo(bDistance);
        });


        foreach (var chunkPos in queueSortedByDistance.Take(4))
        {
            RequestedChunksQueue.Remove(chunkPos);
            NetServer.Send(PacketManager.SerializeC2SPacket(new C2SChunkRequest
            {
                ChunkX = chunkPos.X,
                ChunkY = chunkPos.Y,
                ChunkZ = chunkPos.Z
            }), DeliveryMethod.ReliableOrdered);
        }
    }
}