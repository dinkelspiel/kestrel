using System.Numerics;
using Kestrel.Client.ECS;
using Silk.NET.OpenGL;
using Arch.Core.Extensions;
using Kestrel.Client.Mesh;

namespace Kestrel.Client.Renderer;

public class RenderPass(ClientContext clientContext)
{
    public Shader ShadowShader = null!;
    public uint ShadowFbo;
    public uint ShadowMap;
    const uint ShadowSize = 2048;

    public Shader Shader = null!;
    public Texture Atlas = null!;

    public Vector2 TileSize;

    readonly Queue<IDrawInstruction> drawInstructions = [];

    public unsafe void Setup()
    {
        BillboardDrawInstruction.Setup(clientContext);
        CubeDrawInstruction.Setup(clientContext);
        HeightmapDrawInstruction.Setup(clientContext);

        Atlas = new Texture(clientContext.Gl, Path.Combine(AppContext.BaseDirectory, "Assets", "atlas.png"));
        var shadersDir = Path.Combine(AppContext.BaseDirectory, "Assets", "Shaders");
        Shader = Shader.FromFiles(clientContext.Gl, Path.Combine(shadersDir, "default.vert"), Path.Combine(shadersDir, "default.frag"));


        // Shadows
        ShadowFbo = clientContext.Gl.GenFramebuffer();
        ShadowMap = clientContext.Gl.GenTexture();

        clientContext.Gl.BindTexture(TextureTarget.Texture2D, ShadowMap);
        clientContext.Gl.TexImage2D(
            TextureTarget.Texture2D,
            0,
            InternalFormat.DepthComponent,
            ShadowSize,
            ShadowSize,
            0,
            PixelFormat.DepthComponent,
            PixelType.Float,
            null);
        clientContext.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
        clientContext.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        clientContext.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
        clientContext.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

        clientContext.Gl.BindFramebuffer(FramebufferTarget.Framebuffer, ShadowFbo);
        clientContext.Gl.FramebufferTexture2D(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.DepthAttachment,
            TextureTarget.Texture2D,
            ShadowMap,
            0);
        clientContext.Gl.DrawBuffer(DrawBufferMode.None);
        clientContext.Gl.ReadBuffer(ReadBufferMode.None);
        clientContext.Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        ShadowShader = Shader.FromFiles(
            clientContext.Gl,
            Path.Combine(shadersDir, "shadow.vert"),
            Path.Combine(shadersDir, "shadow.frag"));
    }

    public void Begin()
    {
        drawInstructions.Clear();
        TileSize = new Vector2(16f / Atlas.Width, 16f / Atlas.Height);
    }

    public void DrawCube(Matrix4x4 translation, (int X, int Y) atlasPosition)
    {
        drawInstructions.Enqueue(new CubeDrawInstruction(clientContext, TileSize, translation, atlasPosition));
    }

    public void DrawBillboard(Matrix4x4 translation, (int X, int Y) atlasPosition)
    {
        drawInstructions.Enqueue(new BillboardDrawInstruction(clientContext, TileSize, translation, atlasPosition));
    }

    public void DrawHeightmap(Matrix4x4 translation, (int X, int Y) atlasPosition)
    {
        drawInstructions.Enqueue(new HeightmapDrawInstruction(clientContext, TileSize, translation, atlasPosition));
    }

    public void End()
    {
        var size = clientContext.Window.Size;

        // Shadows
        var sunPosition = new Vector3(-30f, 50f, -30f);
        var sceneCenter = new Vector3(64f, 0f, 64f);
        if (clientContext.TryGetPlayer(out var player))
            sceneCenter = player.Get<TransformComponent>().Postition;

        sunPosition += sceneCenter;
        var lightView = Matrix4x4.CreateLookAt(
            sunPosition,
            sceneCenter,
            Vector3.UnitY);

        var lightProjection = Matrix4x4.CreateOrthographic(
            100f,
            100f,
            1f,
            150f);

        clientContext.Gl.Viewport(0, 0, ShadowSize, ShadowSize);
        clientContext.Gl.BindFramebuffer(FramebufferTarget.Framebuffer, ShadowFbo);
        clientContext.Gl.Clear(ClearBufferMask.DepthBufferBit);

        ShadowShader.Use();
        ShadowShader.SetMatrix4("uLightView", lightView);
        ShadowShader.SetMatrix4("uLightProjection", lightProjection);

        foreach (IDrawInstruction drawInstruction in drawInstructions)
        {
            drawInstruction.Draw(lightView, lightProjection, ShadowShader);
        }

        clientContext.Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        clientContext.Gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);

        // Regular
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(
            MathF.PI / 180f * 65f,
            (float)size.X / size.Y,
            0.1f, 100f);

        Matrix4x4 view = clientContext.camera.GetViewMatrix();

        Shader.Use();
        Shader.SetInt("uTexture", 0);
        Shader.SetInt("uShadowMap", 1);

        Atlas.Bind(TextureUnit.Texture0);

        clientContext.Gl.ActiveTexture(TextureUnit.Texture1);
        clientContext.Gl.BindTexture(TextureTarget.Texture2D, ShadowMap);

        Shader.SetMatrix4("uLightView", lightView);
        Shader.SetMatrix4("uLightProjection", lightProjection);
        Shader.SetVector3("uSunDirection", Vector3.Normalize(sunPosition - sceneCenter));

        foreach (IDrawInstruction drawInstruction in drawInstructions)
        {
            drawInstruction.Draw(view, projection, Shader);
        }
    }

    public void CleanUp()
    {
        CubeDrawInstruction.CleanUp(clientContext);
        BillboardDrawInstruction.CleanUp(clientContext);
        HeightmapDrawInstruction.CleanUp(clientContext);

        Atlas.Dispose();
        Shader.Dispose();
        ShadowShader.Dispose();
        clientContext.Gl.DeleteFramebuffer(ShadowFbo);
        clientContext.Gl.DeleteTexture(ShadowMap);
    }
}