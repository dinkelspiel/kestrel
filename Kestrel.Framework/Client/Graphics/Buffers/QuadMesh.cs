using GlmSharp;
using Kestrel.Framework.Client.Graphics.Shaders;
using Kestrel.Framework.Server.Player;
using Kestrel.Framework.Utils;
using Kestrel.Framework.World;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace Kestrel.Framework.Client.Graphics.Buffers;

public class QuadMesh
{
    private uint _vao, _vbo;
    private ShaderProgram _shader;
    private ClientState clientState;
    private static uint _texture;
    Chunk chunk;
    World.World world;
    Mesher mesher;

    public unsafe QuadMesh(ClientState clientState, ShaderProgram shader)
    {
        world = new();
        this.chunk = new(world, 0, 0, 0);

        this.clientState = clientState;
        this._shader = shader;
        _vao = clientState.Window.GL.GenVertexArray();
        clientState.Window.GL.BindVertexArray(_vao);

        mesher = new();
        for (int x = 0; x < world.ChunkSize; x++)
        {
            for (int y = 0; y < world.ChunkSize; y++)
            {
                for (int z = 0; z < world.ChunkSize; z++)
                {
                    if (chunk.GetBlock(x, y, z) == BlockType.Air)
                        continue;

                    if (chunk.GetBlock(x, y + 1, z) == BlockType.Air)
                        mesher.AddTopFace(x, y, z);
                    if (chunk.GetBlock(x, y - 1, z) == BlockType.Air)
                        mesher.AddBottomFace(x, y, z);
                    if (chunk.GetBlock(x + 1, y, z) == BlockType.Air)
                        mesher.AddRightFace(x, y, z);
                    if (chunk.GetBlock(x - 1, y, z) == BlockType.Air)
                        mesher.AddLeftFace(x, y, z);
                    if (chunk.GetBlock(x, y, z + 1) == BlockType.Air)
                        mesher.AddFrontFace(x, y, z);
                    if (chunk.GetBlock(x, y, z - 1) == BlockType.Air)
                        mesher.AddBackFace(x, y, z);
                }
            }
        }
        mesher.AddBackFace(0, 0, 0);
        Console.WriteLine($"Vertices: {mesher.Vertices.Count}");

        _vbo = clientState.Window.GL.GenBuffer();
        clientState.Window.GL.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        float[] vertices = mesher.Vertices.ToArray();
        fixed (float* buf = vertices)
            clientState.Window.GL.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);

        clientState.Window.GL.EnableVertexAttribArray(1);
        clientState.Window.GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float)));

        clientState.Window.GL.EnableVertexAttribArray(0);
        clientState.Window.GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)0);

        _texture = clientState.Window.GL.GenTexture();
        clientState.Window.GL.ActiveTexture(TextureUnit.Texture0);
        clientState.Window.GL.BindTexture(TextureTarget.Texture2D, _texture);

        ImageResult result = ImageResult.FromMemory(File.ReadAllBytes(Paths.InAssets("textures/wall.jpg")), ColorComponents.RedGreenBlueAlpha);

        fixed (byte* ptr = result.Data)
            clientState.Window.GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)result.Width,
                (uint)result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);

        clientState.Window.GL.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)TextureWrapMode.Repeat);
        clientState.Window.GL.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)TextureWrapMode.Repeat);
        clientState.Window.GL.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)TextureMinFilter.Nearest);
        clientState.Window.GL.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)TextureMagFilter.Nearest);

        clientState.Window.GL.BindTexture(TextureTarget.Texture2D, 0);

        int location = shader.GetUniformLocation("uTexture");
        clientState.Window.GL.Uniform1(location, 0);

        clientState.Window.GL.BindVertexArray(0);
        clientState.Window.GL.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        clientState.Window.GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
    }

    public unsafe void Draw()
    {
        clientState.Window.GL.BindVertexArray(_vao);
        clientState.Window.GL.ActiveTexture(TextureUnit.Texture0);
        clientState.Window.GL.BindTexture(TextureTarget.Texture2D, _texture);
        _shader.Use();

        mat4 projection = mat4.Perspective(glm.Radians(90.0f), (float)clientState.Window.Width / (float)clientState.Window.Height, 0.1f, 100.0f);

        fixed (float* matrixPtr = clientState.Camera.View.Values1D)
        {
            int loc = _shader.GetUniformLocation("view");
            clientState.Window.GL.UniformMatrix4(loc, 1, false, matrixPtr);
        }
        fixed (float* matrixPtr = projection.Values1D)
        {
            int loc = _shader.GetUniformLocation("projection");
            clientState.Window.GL.UniformMatrix4(loc, 1, false, matrixPtr);
        }



        vec3 pos = new(0, 0, 0);
        mat4 model = mat4.Identity * mat4.Translate(pos);
        fixed (float* ptr = model.Values1D)
            clientState.Window.GL.UniformMatrix4(_shader.GetUniformLocation("model"), 1, false, ptr);

        clientState.Window.GL.DrawArrays(PrimitiveType.Triangles, 0, (uint)mesher.Vertices.Count / 5);
        // for (int x = 0; x < world.ChunkSize; x++)
        // {
        //     for (int y = 0; y < world.ChunkSize; y++)
        //     {
        //         for (int z = 0; z < world.ChunkSize; z++)
        //         {
        //             if (chunk.GetBlock(x, y, z) == BlockType.Air)
        //                 continue;

        //             vec3 pos = new(x, y, z);
        //             mat4 model = mat4.Identity * mat4.Translate(pos);
        //             fixed (float* ptr = model.Values1D)
        //                 clientState.Window.GL.UniformMatrix4(_shader.GetUniformLocation("model"), 1, false, ptr);

        //             clientState.Window.GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
        //         }
        //     }
        // }

        // foreach (ClientPlayer player in clientState.Players.Values)
        // {
        //     vec3 pos = player.Location;
        //     mat4 model = mat4.Identity * mat4.Translate(pos);
        //     fixed (float* ptr = model.Values1D)
        //         clientState.Window.GL.UniformMatrix4(_shader.GetUniformLocation("model"), 1, false, ptr);

        //     clientState.Window.GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
        // }

        clientState.Window.GL.BindVertexArray(0);
        // clientState.Window.GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);
    }
}