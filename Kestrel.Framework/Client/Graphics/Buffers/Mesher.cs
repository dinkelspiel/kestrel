namespace Kestrel.Framework.Client.Graphics.Buffers;

public class Mesher
{
    public List<float> Vertices = [];

    public void AddTopFace(float x, float y, float z)
    {
        Vertices.AddRange([
            -0.5f + x,  0.5f + y, -0.5f + z, 0.0f, 1.0f,
            -0.5f + x,  0.5f + y,  0.5f + z, 0.0f, 0.0f,
             0.5f + x,  0.5f + y,  0.5f + z, 1.0f, 0.0f,
             0.5f + x,  0.5f + y,  0.5f + z, 1.0f, 0.0f,
             0.5f + x,  0.5f + y, -0.5f + z, 1.0f, 1.0f,
            -0.5f + x,  0.5f + y, -0.5f + z, 0.0f, 1.0f
        ]);
    }

    public void AddBottomFace(float x, float y, float z)
    {
        Vertices.AddRange([
            -0.5f + x, -0.5f + y, -0.5f + z, 0.0f, 1.0f,
             0.5f + x, -0.5f + y, -0.5f + z, 1.0f, 1.0f,
             0.5f + x, -0.5f + y,  0.5f + z, 1.0f, 0.0f,
             0.5f + x, -0.5f + y,  0.5f + z, 1.0f, 0.0f,
            -0.5f + x, -0.5f + y,  0.5f + z, 0.0f, 0.0f,
            -0.5f + x, -0.5f + y, -0.5f + z, 0.0f, 1.0f,
        ]);
    }

    public void AddRightFace(float x, float y, float z)
    {
        Vertices.AddRange([
            0.5f + x,  0.5f + y,  0.5f + z, 1.0f, 0.0f,
            0.5f + x, -0.5f + y,  0.5f + z, 1.0f, 1.0f,
            0.5f + x, -0.5f + y, -0.5f + z, 0.0f, 1.0f,
            0.5f + x, -0.5f + y, -0.5f + z, 0.0f, 1.0f,
            0.5f + x,  0.5f + y, -0.5f + z, 0.0f, 0.0f,
            0.5f + x,  0.5f + y,  0.5f + z, 1.0f, 0.0f,
        ]);
    }

    public void AddLeftFace(float x, float y, float z)
    {
        Vertices.AddRange([
            -0.5f + x,  0.5f + y,  0.5f + z, 1.0f, 0.0f,
            -0.5f + x,  0.5f + y, -0.5f + z, 1.0f, 1.0f,
            -0.5f + x, -0.5f + y, -0.5f + z, 0.0f, 1.0f,
            -0.5f + x, -0.5f + y, -0.5f + z, 0.0f, 1.0f,
            -0.5f + x, -0.5f + y,  0.5f + z, 0.0f, 0.0f,
            -0.5f + x,  0.5f + y,  0.5f + z, 1.0f, 0.0f,
        ]);
    }

    public void AddBackFace(float x, float y, float z)
    {
        Vertices.AddRange([
            -0.5f + x, -0.5f + y, -0.5f + z, 0.0f, 0.0f,
            -0.5f + x,  0.5f + y, -0.5f + z, 0.0f, 1.0f,
             0.5f + x,  0.5f + y, -0.5f + z, 1.0f, 1.0f,
             0.5f + x,  0.5f + y, -0.5f + z, 1.0f, 1.0f,
             0.5f + x, -0.5f + y, -0.5f + z, 1.0f, 0.0f,
            -0.5f + x, -0.5f + y, -0.5f + z, 0.0f, 0.0f,
        ]);
    }

    public void AddFrontFace(float x, float y, float z)
    {
        Vertices.AddRange([
            -0.5f + x, -0.5f + y,  0.5f + z, 0.0f, 0.0f,
             0.5f + x, -0.5f + y,  0.5f + z, 1.0f, 0.0f,
             0.5f + x,  0.5f + y,  0.5f + z, 1.0f, 1.0f,
             0.5f + x,  0.5f + y,  0.5f + z, 1.0f, 1.0f,
            -0.5f + x,  0.5f + y,  0.5f + z, 0.0f, 1.0f,
            -0.5f + x, -0.5f + y,  0.5f + z, 0.0f, 0.0f,
        ]);
    }
}