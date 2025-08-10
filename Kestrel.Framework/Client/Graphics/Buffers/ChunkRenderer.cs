using System.Numerics;
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
    CubeMesh cube;

    public unsafe QuadMesh(ClientState clientState, ShaderProgram shader)
    {
        this.clientState = clientState;
        this._shader = shader;

        _texture = clientState.Window.GL.GenTexture();
        clientState.Window.GL.ActiveTexture(TextureUnit.Texture0);
        clientState.Window.GL.BindTexture(TextureTarget.Texture2D, _texture);

        ImageResult result = ImageResult.FromMemory(File.ReadAllBytes(Paths.InAssets("textures/atlas.png")), ColorComponents.RedGreenBlueAlpha);

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

        cube = new(clientState);
        cube.Generate();
    }

    public unsafe void Draw()
    {
        clientState.Window.GL.ActiveTexture(TextureUnit.Texture0);
        clientState.Window.GL.BindTexture(TextureTarget.Texture2D, _texture);
        _shader.Use();

        float nearPlane = 0.1f;
        float farPlane = 3000.0f;

        var fogColor = new Vector3(105.0f / 255.0f, 196.0f / 255.0f, 224.0f / 255.0f);
        float fogDensity = 0.0050f;

        int locFogColor = _shader.GetUniformLocation("uFogColor");
        int locFogDensity = _shader.GetUniformLocation("uFogDensity");
        int locNear = _shader.GetUniformLocation("uNear");
        int locFar = _shader.GetUniformLocation("uFar");

        clientState.Window.GL.Uniform3(locFogColor, fogColor.X, fogColor.Y, fogColor.Z);
        clientState.Window.GL.Uniform1(locFogDensity, fogDensity);
        clientState.Window.GL.Uniform1(locNear, nearPlane);
        clientState.Window.GL.Uniform1(locFar, farPlane);

        DrawWorldAndEntities();

        clientState.Window.GL.BindVertexArray(0);
    }

    public unsafe void DrawWorldAndEntities()
    {
        mat4 projection = mat4.Perspective(glm.Radians(90.0f), (float)clientState.Window.Width / (float)clientState.Window.Height, 0.1f, 3000.0f);

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

        foreach (KeyValuePair<Vector3I, ChunkMesh> mesh in clientState.ChunkMeshes.ToList())
        {
            mesh.Value.Bind();

            vec3 pos = new(mesh.Key.X * clientState.World.ChunkSize, mesh.Key.Y * clientState.World.ChunkSize, mesh.Key.Z * clientState.World.ChunkSize);
            mat4 model = mat4.Identity * mat4.Translate(pos);
            fixed (float* ptr = model.Values1D)
                clientState.Window.GL.UniformMatrix4(_shader.GetUniformLocation("model"), 1, false, ptr);

            clientState.Window.GL.DrawArrays(PrimitiveType.Triangles, 0, (uint)mesh.Value.Vertices.Length / 6);
        }

        cube.Bind();
        foreach (ClientPlayer player in clientState.Players.Values)
        {
            vec3 pos = player.Location.ToVec3();
            mat4 model = mat4.Identity * mat4.Translate(pos);
            fixed (float* ptr = model.Values1D)
                clientState.Window.GL.UniformMatrix4(_shader.GetUniformLocation("model"), 1, false, ptr);

            clientState.Window.GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
        }

        {
            vec3 pos = clientState.Player.Location.ToVec3();
            mat4 model = mat4.Identity * mat4.Translate(pos);
            fixed (float* ptr = model.Values1D)
                clientState.Window.GL.UniformMatrix4(_shader.GetUniformLocation("model"), 1, false, ptr);

            clientState.Window.GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
        }
    }
}