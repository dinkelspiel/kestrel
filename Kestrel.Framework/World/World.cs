namespace Kestrel.Framework.World;

public readonly struct ChunkPos : IEquatable<ChunkPos>
{
    public readonly int X, Y, Z;
    public ChunkPos(int x, int y, int z) { X = x; Y = y; Z = z; }

    public bool Equals(ChunkPos other) => X == other.X && Y == other.Y && Z == other.Z;
    public override bool Equals(object? obj) => obj is ChunkPos cp && Equals(cp);
    public override int GetHashCode() => HashCode.Combine(X, Y, Z);
}


public sealed class World
{
    public readonly int ChunkSize = 32;
    private readonly Dictionary<ChunkPos, Chunk> _chunks = new();

    public bool TryGetChunk(int cx, int cy, int cz, out Chunk chunk) =>
        _chunks.TryGetValue(new ChunkPos(cx, cy, cz), out chunk);

    public void SetChunk(int cx, int cy, int cz, Chunk chunk) =>
        _chunks[new ChunkPos(cx, cy, cz)] = chunk;

    public Generator Generator = new();

    public static void WorldToChunk(int wx, int wy, int wz, int chunkSize,
                                   out ChunkPos cpos, out (int lx, int ly, int lz) local)
    {
        int cx = MathF.Floor((float)wx / chunkSize) is var fcx ? (int)fcx : 0;
        int cy = MathF.Floor((float)wy / chunkSize) is var fcy ? (int)fcy : 0;
        int cz = MathF.Floor((float)wz / chunkSize) is var fcz ? (int)fcz : 0;

        int lx = wx - cx * chunkSize;
        int ly = wy - cy * chunkSize;
        int lz = wz - cz * chunkSize;

        cpos = new ChunkPos(cx, cy, cz);
        local = (lx, ly, lz);
    }

    public void WorldToChunk(int wx, int wy, int wz,
                               out ChunkPos cpos, out (int lx, int ly, int lz) local)
    {
        WorldToChunk(wx, wy, wz, ChunkSize, out cpos, out local);
    }
}