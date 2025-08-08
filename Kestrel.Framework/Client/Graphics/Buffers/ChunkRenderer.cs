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
    private ShaderProgram _shader;
    private ClientState clientState;
    private static uint _texture;
    World.World world;
    ChunkMesh mesh1;
    ChunkMesh mesh2;
    CubeMesh cube;

    public unsafe QuadMesh(ClientState clientState, ShaderProgram shader)
    {
        world = new();
        Chunk chunk1 = new(world, 0, 0, 0);
        Chunk chunk2 = new(world, 1, 0, 0);

        this.clientState = clientState;
        this._shader = shader;

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

        mesh1 = new(clientState, chunk1);
        mesh1.Generate();
        mesh2 = new(clientState, chunk2);
        mesh2.Generate();
        cube = new(clientState);
        cube.Generate();
    }

    public unsafe void Draw()
    {
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

        mesh1.Bind();
        vec3 pos = new(0, 0, 0);
        mat4 model = mat4.Identity * mat4.Translate(pos);
        fixed (float* ptr = model.Values1D)
            clientState.Window.GL.UniformMatrix4(_shader.GetUniformLocation("model"), 1, false, ptr);

        clientState.Window.GL.DrawArrays(PrimitiveType.Triangles, 0, (uint)mesh1.Vertices.Length / 5);

        mesh2.Bind();
        pos = new(32, 0, 0);
        model = mat4.Identity * mat4.Translate(pos);
        fixed (float* ptr = model.Values1D)
            clientState.Window.GL.UniformMatrix4(_shader.GetUniformLocation("model"), 1, false, ptr);

        clientState.Window.GL.DrawArrays(PrimitiveType.Triangles, 0, (uint)mesh2.Vertices.Length / 5);
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

        cube.Bind();
        foreach (ClientPlayer player in clientState.Players.Values)
        {
            pos = player.Location;
            model = mat4.Identity * mat4.Translate(pos);
            fixed (float* ptr = model.Values1D)
                clientState.Window.GL.UniformMatrix4(_shader.GetUniformLocation("model"), 1, false, ptr);

            clientState.Window.GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
        }

        clientState.Window.GL.BindVertexArray(0);
        // clientState.Window.GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);
    }
}