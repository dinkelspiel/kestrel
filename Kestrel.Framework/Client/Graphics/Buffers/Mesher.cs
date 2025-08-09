namespace Kestrel.Framework.Client.Graphics.Buffers;

public class Mesher
{
    public List<float> Vertices = [];

    const int ATLAS_SIZE = 512;
    const int TILE_SIZE = 16;
    const int TILES = ATLAS_SIZE / TILE_SIZE;
    const float TILE_UV = 1f / (float)TILES;        // 0.03125
    const float EPS = 0.5f / (float)ATLAS_SIZE; // 0.0009765625

    static (float u0, float v0, float u1, float v1) GetUVRect(int tileX, int tileY)
    {
        float u0 = tileX * TILE_UV + EPS;
        float u1 = (tileX + 1) * TILE_UV - EPS;

        float v0 = tileY * TILE_UV + EPS;
        float v1 = (tileY + 1) * TILE_UV - EPS;

        return (u0, v0, u1, v1);
    }

    public void AddUpFace(float x, float y, float z)
    {
        var (u0, v0, u1, v1) = GetUVRect(0, 0);

        Vertices.AddRange([
            -0.5f + x,  0.5f + y, -0.5f + z, u0, v1, (float)Direction.UP,
            -0.5f + x,  0.5f + y,  0.5f + z, u0, v0, (float)Direction.UP,
             0.5f + x,  0.5f + y,  0.5f + z, u1, v0, (float)Direction.UP,
             0.5f + x,  0.5f + y,  0.5f + z, u1, v0, (float)Direction.UP,
             0.5f + x,  0.5f + y, -0.5f + z, u1, v1, (float)Direction.UP,
            -0.5f + x,  0.5f + y, -0.5f + z, u0, v1, (float)Direction.UP,
        ]);
    }

    public void AddDownFace(float x, float y, float z)
    {
        var (u0, v0, u1, v1) = GetUVRect(0, 0);

        Vertices.AddRange([
            -0.5f + x, -0.5f + y, -0.5f + z, u0, v1, (float)Direction.DOWN,
             0.5f + x, -0.5f + y, -0.5f + z, u0, v0, (float)Direction.DOWN,
             0.5f + x, -0.5f + y,  0.5f + z, u1, v0, (float)Direction.DOWN,
             0.5f + x, -0.5f + y,  0.5f + z, u1, v0, (float)Direction.DOWN,
            -0.5f + x, -0.5f + y,  0.5f + z, u1, v1, (float)Direction.DOWN,
            -0.5f + x, -0.5f + y, -0.5f + z, u0, v1, (float)Direction.DOWN,
        ]);
    }

    public void AddEastFace(float x, float y, float z)
    {
        var (u0, v0, u1, v1) = GetUVRect(0, 0);

        Vertices.AddRange([
            0.5f + x,  0.5f + y,  0.5f + z, u0, v1, (float)Direction.EAST,
            0.5f + x, -0.5f + y,  0.5f + z, u0, v0, (float)Direction.EAST,
            0.5f + x, -0.5f + y, -0.5f + z, u1, v0, (float)Direction.EAST,
            0.5f + x, -0.5f + y, -0.5f + z, u1, v0, (float)Direction.EAST,
            0.5f + x,  0.5f + y, -0.5f + z, u1, v1, (float)Direction.EAST,
            0.5f + x,  0.5f + y,  0.5f + z, u0, v1, (float)Direction.EAST,
        ]);
    }

    public void AddWestFace(float x, float y, float z)
    {
        var (u0, v0, u1, v1) = GetUVRect(0, 0);

        Vertices.AddRange([
            -0.5f + x,  0.5f + y,  0.5f + z, u0, v1, (float)Direction.WEST,
            -0.5f + x,  0.5f + y, -0.5f + z, u0, v0, (float)Direction.WEST,
            -0.5f + x, -0.5f + y, -0.5f + z, u1, v0, (float)Direction.WEST,
            -0.5f + x, -0.5f + y, -0.5f + z, u1, v0, (float)Direction.WEST,
            -0.5f + x, -0.5f + y,  0.5f + z, u1, v1, (float)Direction.WEST,
            -0.5f + x,  0.5f + y,  0.5f + z, u0, v1, (float)Direction.WEST,
        ]);
    }

    public void AddSouthFace(float x, float y, float z)
    {
        var (u0, v0, u1, v1) = GetUVRect(0, 0);

        Vertices.AddRange([
            -0.5f + x, -0.5f + y, -0.5f + z, u0, v1, (float)Direction.SOUTH,
            -0.5f + x,  0.5f + y, -0.5f + z, u0, v0, (float)Direction.SOUTH,
             0.5f + x,  0.5f + y, -0.5f + z, u1, v0, (float)Direction.SOUTH,
             0.5f + x,  0.5f + y, -0.5f + z, u1, v0, (float)Direction.SOUTH,
             0.5f + x, -0.5f + y, -0.5f + z, u1, v1, (float)Direction.SOUTH,
            -0.5f + x, -0.5f + y, -0.5f + z, u0, v1, (float)Direction.SOUTH,
        ]);
    }

    public void AddNorthFace(float x, float y, float z)
    {
        var (u0, v0, u1, v1) = GetUVRect(0, 0);

        Vertices.AddRange([
            -0.5f + x, -0.5f + y,  0.5f + z, u0, v1, (float)Direction.NORTH,
             0.5f + x, -0.5f + y,  0.5f + z, u0, v0, (float)Direction.NORTH,
             0.5f + x,  0.5f + y,  0.5f + z, u1, v0, (float)Direction.NORTH,
             0.5f + x,  0.5f + y,  0.5f + z, u1, v0, (float)Direction.NORTH,
            -0.5f + x,  0.5f + y,  0.5f + z, u1, v1, (float)Direction.NORTH,
            -0.5f + x, -0.5f + y,  0.5f + z, u0, v1, (float)Direction.NORTH,
        ]);
    }
}