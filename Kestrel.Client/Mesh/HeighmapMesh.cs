using System.Numerics;
using Kestrel.Client.MMath;
using Kestrel.Client.Renderer;

namespace Kestrel.Client.Mesh;

public class HeighmapMesh
{
    public void Draw(RenderPass renderPass)
    {
        FastNoiseLite noise = new();
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

        // Gather noise data
        float[,] noiseData = new float[128, 128];

        for (int x = 0; x < 128; x++)
        {
            for (int y = 0; y < 128; y++)
            {
                // noiseData[x, y] = noise.GetNoise(x, y);
                renderPass.DrawCube(Matrix4x4.CreateTranslation(x, MathF.Floor(noise.GetNoise(x, y) * 6), y), (0, 0));
            }
        }
    }
}