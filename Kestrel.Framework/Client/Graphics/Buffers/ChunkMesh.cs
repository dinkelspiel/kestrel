using Kestrel.Framework.World;
using Silk.NET.OpenGL;

namespace Kestrel.Framework.Client.Graphics.Buffers;

public class ChunkMesh(ClientState clientState, Chunk chunk)
{
    readonly Chunk chunk = chunk;
    public float[] Vertices = [];
    uint vbo, vao;

    public unsafe void Generate()
    {
        vao = clientState.Window.GL.GenVertexArray();
        clientState.Window.GL.BindVertexArray(vao);

        vbo = clientState.Window.GL.GenBuffer();
        clientState.Window.GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);

        Mesher mesher = new();
        for (int x = 0; x < chunk.World.ChunkSize; x++)
        {
            for (int y = 0; y < chunk.World.ChunkSize; y++)
            {
                for (int z = 0; z < chunk.World.ChunkSize; z++)
                {
                    if (chunk.GetBlock(x, y, z) == BlockType.Air)
                        continue;

                    if (chunk.GetBlock(x, y + 1, z) == BlockType.Air)
                        mesher.AddUpFace(x, y, z);
                    if (chunk.GetBlock(x, y - 1, z) == BlockType.Air)
                        mesher.AddDownFace(x, y, z);
                    if (chunk.GetBlock(x + 1, y, z) == BlockType.Air)
                        mesher.AddEastFace(x, y, z);
                    if (chunk.GetBlock(x - 1, y, z) == BlockType.Air)
                        mesher.AddWestFace(x, y, z);
                    if (chunk.GetBlock(x, y, z + 1) == BlockType.Air)
                        mesher.AddNorthFace(x, y, z);
                    if (chunk.GetBlock(x, y, z - 1) == BlockType.Air)
                        mesher.AddSouthFace(x, y, z);
                }
            }
        }
        Vertices = mesher.Vertices.ToArray();

        fixed (float* buf = Vertices)
            clientState.Window.GL.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(Vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);

        clientState.Window.GL.EnableVertexAttribArray(0);
        clientState.Window.GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), (void*)0);

        clientState.Window.GL.EnableVertexAttribArray(1);
        clientState.Window.GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 6 * sizeof(float), (void*)(3 * sizeof(float)));

        clientState.Window.GL.EnableVertexAttribArray(2);
        clientState.Window.GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, 6 * sizeof(float), (void*)(5 * sizeof(float)));

        // Clean up
        clientState.Window.GL.BindVertexArray(0);
        clientState.Window.GL.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
    }

    public void Bind() => clientState.Window.GL.BindVertexArray(vao);
}