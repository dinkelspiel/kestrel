namespace Kestrel.Framework.Client.Graphics.Buffers;

public class Mesher
{
    public List<float> Vertices = [];

    public void AddUpFace(float x, float y, float z)
    {
        Vertices.AddRange([
            -0.5f + x,  0.5f + y, -0.5f + z, 0.0f, 1.0f, (float)Direction.UP,
            -0.5f + x,  0.5f + y,  0.5f + z, 0.0f, 0.0f, (float)Direction.UP,
             0.5f + x,  0.5f + y,  0.5f + z, 1.0f, 0.0f, (float)Direction.UP,
             0.5f + x,  0.5f + y,  0.5f + z, 1.0f, 0.0f, (float)Direction.UP,
             0.5f + x,  0.5f + y, -0.5f + z, 1.0f, 1.0f, (float)Direction.UP,
            -0.5f + x,  0.5f + y, -0.5f + z, 0.0f, 1.0f, (float)Direction.UP,
        ]);
    }

    public void AddDownFace(float x, float y, float z)
    {
        Vertices.AddRange([
            -0.5f + x, -0.5f + y, -0.5f + z, 0.0f, 1.0f, (float)Direction.DOWN,
             0.5f + x, -0.5f + y, -0.5f + z, 1.0f, 1.0f, (float)Direction.DOWN,
             0.5f + x, -0.5f + y,  0.5f + z, 1.0f, 0.0f, (float)Direction.DOWN,
             0.5f + x, -0.5f + y,  0.5f + z, 1.0f, 0.0f, (float)Direction.DOWN,
            -0.5f + x, -0.5f + y,  0.5f + z, 0.0f, 0.0f, (float)Direction.DOWN,
            -0.5f + x, -0.5f + y, -0.5f + z, 0.0f, 1.0f, (float)Direction.DOWN,
        ]);
    }

    public void AddEastFace(float x, float y, float z)
    {
        Vertices.AddRange([
            0.5f + x,  0.5f + y,  0.5f + z, 1.0f, 0.0f, (float)Direction.EAST,
            0.5f + x, -0.5f + y,  0.5f + z, 1.0f, 1.0f, (float)Direction.EAST,
            0.5f + x, -0.5f + y, -0.5f + z, 0.0f, 1.0f, (float)Direction.EAST,
            0.5f + x, -0.5f + y, -0.5f + z, 0.0f, 1.0f, (float)Direction.EAST,
            0.5f + x,  0.5f + y, -0.5f + z, 0.0f, 0.0f, (float)Direction.EAST,
            0.5f + x,  0.5f + y,  0.5f + z, 1.0f, 0.0f, (float)Direction.EAST,
        ]);
    }

    public void AddWestFace(float x, float y, float z)
    {
        Vertices.AddRange([
            -0.5f + x,  0.5f + y,  0.5f + z, 1.0f, 0.0f, (float)Direction.WEST,
            -0.5f + x,  0.5f + y, -0.5f + z, 1.0f, 1.0f, (float)Direction.WEST,
            -0.5f + x, -0.5f + y, -0.5f + z, 0.0f, 1.0f, (float)Direction.WEST,
            -0.5f + x, -0.5f + y, -0.5f + z, 0.0f, 1.0f, (float)Direction.WEST,
            -0.5f + x, -0.5f + y,  0.5f + z, 0.0f, 0.0f, (float)Direction.WEST,
            -0.5f + x,  0.5f + y,  0.5f + z, 1.0f, 0.0f, (float)Direction.WEST,
        ]);
    }

    public void AddSouthFace(float x, float y, float z)
    {
        Vertices.AddRange([
            -0.5f + x, -0.5f + y, -0.5f + z, 0.0f, 0.0f, (float)Direction.SOUTH,
            -0.5f + x,  0.5f + y, -0.5f + z, 0.0f, 1.0f, (float)Direction.SOUTH,
             0.5f + x,  0.5f + y, -0.5f + z, 1.0f, 1.0f, (float)Direction.SOUTH,
             0.5f + x,  0.5f + y, -0.5f + z, 1.0f, 1.0f, (float)Direction.SOUTH,
             0.5f + x, -0.5f + y, -0.5f + z, 1.0f, 0.0f, (float)Direction.SOUTH,
            -0.5f + x, -0.5f + y, -0.5f + z, 0.0f, 0.0f, (float)Direction.SOUTH,
        ]);
    }

    public void AddNorthFace(float x, float y, float z)
    {
        Vertices.AddRange([
            -0.5f + x, -0.5f + y,  0.5f + z, 0.0f, 0.0f, (float)Direction.NORTH,
             0.5f + x, -0.5f + y,  0.5f + z, 1.0f, 0.0f, (float)Direction.NORTH,
             0.5f + x,  0.5f + y,  0.5f + z, 1.0f, 1.0f, (float)Direction.NORTH,
             0.5f + x,  0.5f + y,  0.5f + z, 1.0f, 1.0f, (float)Direction.NORTH,
            -0.5f + x,  0.5f + y,  0.5f + z, 0.0f, 1.0f, (float)Direction.NORTH,
            -0.5f + x, -0.5f + y,  0.5f + z, 0.0f, 0.0f, (float)Direction.NORTH,
        ]);
    }
}