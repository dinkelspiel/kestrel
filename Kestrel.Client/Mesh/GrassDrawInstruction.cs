using System.Numerics;
using Kestrel.Client.Renderer;
using Silk.NET.OpenGL;

namespace Kestrel.Client.Mesh;

public class GrassDrawInstruction(ClientContext clientContext, Vector2 tileSize, Matrix4x4 translation, (int X, int Y) atlasPosition, float[,] heightmap, int size) : IDrawInstruction
{
    const float SteepNormalThreshold = 0.80f;

    public static uint Vao, Vbo, Ebo, InstanceVbo;

    static readonly float[] Vertices =
    [
        -0.5f, -0.5f,  0f,  0f, 1f,
         0.5f, -0.5f,  0f,  1f, 1f,
         0.5f,  0.5f,  0f,  1f, 0f,
        -0.5f,  0.5f,  0f,  0f, 0f,
    ];

    static readonly uint[] Indices =
    [
         2,  1,  0,   0,  3,  2,
    ];

    Vector3[] translations = [];

    public unsafe void Setup(ClientContext clientContext)
    {
        Vao = clientContext.Gl.GenVertexArray();
        clientContext.Gl.BindVertexArray(Vao);

        Vbo = clientContext.Gl.GenBuffer();
        clientContext.Gl.BindBuffer(BufferTargetARB.ArrayBuffer, Vbo);
        fixed (float* v = Vertices)
            clientContext.Gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(Vertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);

        Ebo = clientContext.Gl.GenBuffer();
        clientContext.Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, Ebo);
        fixed (uint* i = Indices)
            clientContext.Gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(Indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);

        const uint stride = 5 * sizeof(float);
        clientContext.Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, (void*)0);
        clientContext.Gl.EnableVertexAttribArray(0);
        clientContext.Gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float)));
        clientContext.Gl.EnableVertexAttribArray(1);

        Vector3 GetNormal(int x, int y)
        {
            float left = heightmap[Math.Max(x - 1, 0), y];
            float right = heightmap[Math.Min(x + 1, size - 1), y];
            float down = heightmap[x, Math.Max(y - 1, 0)];
            float up = heightmap[x, Math.Min(y + 1, size - 1)];

            return Vector3.Normalize(new Vector3(left - right, 2f, down - up));
        }

        List<Vector3> grassTranslations = [];
        int start = (int)(size * 0.2f);
        int end = (int)(size * 0.8f) - 1;

        Random random = new();
        for (int y = start; y < end; y += 1)
        {
            for (int x = start; x < end; x += 1)
            {
                if ((x + y) % 3 != 0)
                    continue;

                if (heightmap[x, y] - 0.5f < 0)
                    continue;

                float upDot = Math.Abs(Vector3.Dot(GetNormal(x, y), Vector3.UnitY));
                if (upDot < SteepNormalThreshold)
                    continue;

                Vector3 translation = new(x + (((float)random.NextDouble() - 0.5f) * 2.0f), heightmap[x, y] + 0.25f, y + (((float)random.NextDouble() - 0.5f) * 2.0f));
                grassTranslations.Add(translation);
            }
        }

        translations = [.. grassTranslations];

        InstanceVbo = clientContext.Gl.GenBuffer();
        clientContext.Gl.BindBuffer(BufferTargetARB.ArrayBuffer, InstanceVbo);
        fixed (Vector3* t = translations)
            clientContext.Gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(translations.Length * sizeof(Vector3)), t, BufferUsageARB.StaticDraw);

        clientContext.Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Vector3), (void*)0);
        clientContext.Gl.EnableVertexAttribArray(2);
        clientContext.Gl.VertexAttribDivisor(2, 1);
    }

    public unsafe void Draw(Matrix4x4 view, Matrix4x4 projection, Renderer.Shader shader)
    {
        shader.SetMatrix4("uView", view);
        shader.SetMatrix4("uProjection", projection);
        clientContext.Gl.BindVertexArray(Vao);

        shader.SetVector2("uTileOffset", new Vector2(tileSize.X * atlasPosition.X, tileSize.Y * atlasPosition.Y));
        shader.SetVector2("uTileSize", tileSize);
        shader.SetInt("uIsHeightmap", 0);
        shader.SetMatrix4("uModel", translation * clientContext.camera.GetViewRotation());
        clientContext.Gl.DrawElementsInstanced(PrimitiveType.Triangles, (uint)Indices.Length, DrawElementsType.UnsignedInt, null, (uint)translations.Length);
    }

    public void CleanUp(ClientContext clientContext)
    {
        clientContext.Gl.DeleteVertexArray(Vao);
        clientContext.Gl.DeleteBuffer(Vbo);
        clientContext.Gl.DeleteBuffer(Ebo);
        clientContext.Gl.DeleteBuffer(InstanceVbo);
    }

    public ShaderKind GetShader() => ShaderKind.GRASS;
}