using System.Collections.Concurrent;
using Kestrel.Framework.Utils;

namespace Kestrel.Framework.World;

public sealed class World
{
    public readonly int ChunkSize = 32;
    private readonly ConcurrentDictionary<Vector3I, Chunk> _chunks = new();
    public Generator Generator;

    public World()
    {
        Generator = new(this);
    }

    public Chunk GetChunkOrGenerate(int cx, int cy, int cz)
    {
        Vector3I chunkPos = new(cx, cy, cz);
        if (_chunks.TryGetValue(chunkPos, out var chunk))
        {
            return chunk;
        }

        Chunk newChunk = new(this, cx, cy, cz);
        newChunk.Generate();
        _chunks.TryAdd(chunkPos, newChunk);
        return newChunk;
    }

    public bool LocationIsLoaded(int wx, int wy, int wz, out Chunk chunk)
    {
        WorldToChunk(wx, wy, wz, ChunkSize, out var chunkPos, out var _);
        Chunk? c = GetChunk(chunkPos.X, chunkPos.Y, chunkPos.Z);
        if (c != null)
        {
            chunk = c;
            return true;
        }
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        chunk = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        return false;
    }

    public Chunk? GetChunk(int cx, int cy, int cz)
    {
        Vector3I chunkPos = new(cx, cy, cz);
        if (_chunks.TryGetValue(chunkPos, out var chunk))
        {
            return chunk;
        }
        return null;
    }

    public void SetChunk(int cx, int cy, int cz, Chunk chunk) =>
        _chunks[new Vector3I(cx, cy, cz)] = chunk;

    public static void WorldToChunk(int wx, int wy, int wz, int chunkSize,
                                   out Vector3I cpos, out (int lx, int ly, int lz) local)
    {
        int cx = MathF.Floor((float)wx / chunkSize) is var fcx ? (int)fcx : 0;
        int cy = MathF.Floor((float)wy / chunkSize) is var fcy ? (int)fcy : 0;
        int cz = MathF.Floor((float)wz / chunkSize) is var fcz ? (int)fcz : 0;

        int lx = wx - cx * chunkSize;
        int ly = wy - cy * chunkSize;
        int lz = wz - cz * chunkSize;

        cpos = new Vector3I(cx, cy, cz);
        local = (lx, ly, lz);
    }

    public void WorldToChunk(int wx, int wy, int wz,
                               out Vector3I cpos, out (int lx, int ly, int lz) local)
    {
        WorldToChunk(wx, wy, wz, ChunkSize, out cpos, out local);
    }
}