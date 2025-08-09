using Kestrel.Framework.World;
using Silk.NET.OpenGL;

namespace Kestrel.Framework.Client.Graphics.Buffers;

public class ChunkMesh(ClientState clientState, Chunk chunk) : Mesh(clientState)
{
    readonly Chunk chunk = chunk;

    public override unsafe void Generate()
    {
        vao = clientState.Window.GL.GenVertexArray();
        clientState.Window.GL.BindVertexArray(vao);

        vbo = clientState.Window.GL.GenBuffer();
        clientState.Window.GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);

        Mesher mesher = new();
        Chunk? topChunk = chunk.World.GetChunk(chunk.ChunkX, chunk.ChunkY + 1, chunk.ChunkZ);
        Chunk? bottomChunk = chunk.World.GetChunk(chunk.ChunkX, chunk.ChunkY - 1, chunk.ChunkZ);
        Chunk? northChunk = chunk.World.GetChunk(chunk.ChunkX, chunk.ChunkY, chunk.ChunkZ + 1);
        Chunk? southChunk = chunk.World.GetChunk(chunk.ChunkX, chunk.ChunkY, chunk.ChunkZ - 1);
        Chunk? westChunk = chunk.World.GetChunk(chunk.ChunkX - 1, chunk.ChunkY, chunk.ChunkZ);
        Chunk? eastChunk = chunk.World.GetChunk(chunk.ChunkX + 1, chunk.ChunkY, chunk.ChunkZ);

        for (int x = 0; x < chunk.World.ChunkSize; x++)
        {
            for (int y = 0; y < chunk.World.ChunkSize; y++)
            {
                for (int z = 0; z < chunk.World.ChunkSize; z++)
                {
                    if (chunk.GetBlock(x, y, z) == BlockType.Air)
                        continue;

                    if (y + 1 >= chunk.World.ChunkSize)
                    {
                        if (topChunk != null)
                        {
                            if (topChunk.GetBlock(x, 0, z) == BlockType.Air)
                                mesher.AddUpFace(x, y, z);
                        }
                        else
                        {
                            mesher.AddUpFace(x, y, z);
                        }
                    }
                    else if (chunk.GetBlock(x, y + 1, z) == BlockType.Air)
                    {
                        mesher.AddUpFace(x, y, z);
                    }

                    if (y - 1 < 0)
                    {
                        if (bottomChunk != null)
                        {
                            if (bottomChunk.GetBlock(x, chunk.World.ChunkSize - 1, z) == BlockType.Air)
                                mesher.AddDownFace(x, y, z);
                        }
                        else
                        {
                            mesher.AddDownFace(x, y, z);
                        }
                    }
                    else if (chunk.GetBlock(x, y - 1, z) == BlockType.Air)
                    {
                        mesher.AddDownFace(x, y, z);
                    }

                    if (x + 1 >= chunk.World.ChunkSize)
                    {
                        if (eastChunk != null)
                        {
                            if (eastChunk.GetBlock(0, y, z) == BlockType.Air)
                                mesher.AddEastFace(x, y, z);
                        }
                        else
                        {
                            mesher.AddEastFace(x, y, z);
                        }
                    }
                    else if (chunk.GetBlock(x + 1, y, z) == BlockType.Air)
                    {
                        mesher.AddEastFace(x, y, z);
                    }

                    if (x - 1 < 0)
                    {
                        if (westChunk != null)
                        {
                            if (westChunk.GetBlock(chunk.World.ChunkSize - 1, y, z) == BlockType.Air)
                                mesher.AddWestFace(x, y, z);
                        }
                        else
                        {
                            mesher.AddWestFace(x, y, z);
                        }
                    }
                    else if (chunk.GetBlock(x - 1, y, z) == BlockType.Air)
                    {
                        mesher.AddWestFace(x, y, z);
                    }

                    if (z + 1 >= chunk.World.ChunkSize)
                    {
                        if (northChunk != null)
                        {
                            if (northChunk.GetBlock(x, y, 0) == BlockType.Air)
                                mesher.AddNorthFace(x, y, z);
                        }
                        else
                        {
                            mesher.AddNorthFace(x, y, z);
                        }
                    }
                    else if (chunk.GetBlock(x, y, z + 1) == BlockType.Air)
                    {
                        mesher.AddNorthFace(x, y, z);
                    }

                    if (z - 1 < 0)
                    {
                        if (southChunk != null)
                        {
                            if (southChunk.GetBlock(x, y, chunk.World.ChunkSize - 1) == BlockType.Air)
                                mesher.AddSouthFace(x, y, z);
                        }
                        else
                        {
                            mesher.AddSouthFace(x, y, z);
                        }
                    }
                    else if (chunk.GetBlock(x, y, z - 1) == BlockType.Air)
                    {
                        mesher.AddSouthFace(x, y, z);
                    }
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
}