using System.Numerics;
using GlmSharp;
using Kestrel.Framework.Client.Graphics;
using Kestrel.Framework.Client.Graphics.Buffers;
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

        Camera = new(this);
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

        // Convert player world pos -> player chunk pos
        World.WorldToChunk(
            (int)Player.Location.X, (int)Player.Location.Y, (int)Player.Location.Z,
            out var playerChunk, out _);

        // Sort nearest-first in chunk space (XZ primary, |dY| secondary)
        var queueSortedByDistance = new List<Vector3I>(RequestedChunksQueue);
        queueSortedByDistance.Sort((a, b) =>
        {
            int dax = a.X - playerChunk.X, daz = a.Z - playerChunk.Z, day = Math.Abs(a.Y - playerChunk.Y);
            int dbx = b.X - playerChunk.X, dbz = b.Z - playerChunk.Z, dby = Math.Abs(b.Y - playerChunk.Y);

            int ra = dax * dax + daz * daz;
            int rb = dbx * dbx + dbz * dbz;
            if (ra != rb) return ra.CompareTo(rb);
            return day.CompareTo(dby);
        });

        // Send the closest few first
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