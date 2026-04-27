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
    const uint ShadowSize = 10240;
    const uint ShadowPreviewSize = 256;

    public Shader Shader = null!;
    public Shader SkyShader = null!;
    public Shader DebugDepthShader = null!;
    public Texture Atlas = null!;
    public uint DebugQuadVao;
    public uint DebugQuadVbo;

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
        SkyShader = Shader.FromFiles(clientContext.Gl, Path.Combine(shadersDir, "debug_depth.vert"), Path.Combine(shadersDir, "sky.frag"));


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
        float[] borderColor = [1f, 1f, 1f, 1f];
        fixed (float* borderColorPtr = borderColor)
            clientContext.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColorPtr);

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

        DebugDepthShader = Shader.FromFiles(
            clientContext.Gl,
            Path.Combine(shadersDir, "debug_depth.vert"),
            Path.Combine(shadersDir, "debug_depth.frag"));

        float[] debugQuadVertices =
        [
            -1f, -1f, 0f, 0f,
             1f, -1f, 1f, 0f,
            -1f,  1f, 0f, 1f,
             1f,  1f, 1f, 1f,
        ];

        DebugQuadVao = clientContext.Gl.GenVertexArray();
        clientContext.Gl.BindVertexArray(DebugQuadVao);

        DebugQuadVbo = clientContext.Gl.GenBuffer();
        clientContext.Gl.BindBuffer(BufferTargetARB.ArrayBuffer, DebugQuadVbo);
        fixed (float* v = debugQuadVertices)
            clientContext.Gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(debugQuadVertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);

        const uint debugQuadStride = 4 * sizeof(float);
        clientContext.Gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, debugQuadStride, (void*)0);
        clientContext.Gl.EnableVertexAttribArray(0);
        clientContext.Gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, debugQuadStride, (void*)(2 * sizeof(float)));
        clientContext.Gl.EnableVertexAttribArray(1);
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
        var sunDirection = Vector3.Normalize(new Vector3(-70f, 35f, -70f));
        var sunPosition = sunDirection * 200f;
        var sceneCenter = new Vector3(64f, 0f, 64f);
        if (clientContext.TryGetPlayer(out var player))
            sceneCenter = player.Get<TransformComponent>().Postition;

        sunPosition += sceneCenter;
        var lightView = Matrix4x4.CreateLookAt(
            sunPosition,
            sceneCenter,
            Vector3.UnitY);

        var lightProjection = Matrix4x4.CreateOrthographic(
            200f,
            200f,
            1f,
            300f);

        clientContext.Gl.Viewport(0, 0, ShadowSize, ShadowSize);
        clientContext.Gl.BindFramebuffer(FramebufferTarget.Framebuffer, ShadowFbo);
        clientContext.Gl.Clear(ClearBufferMask.DepthBufferBit);

        ShadowShader.Use();
        ShadowShader.SetInt("uTexture", 0);
        ShadowShader.SetMatrix4("uLightView", lightView);
        ShadowShader.SetMatrix4("uLightProjection", lightProjection);
        Atlas.Bind(TextureUnit.Texture0);

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
            0.1f, 1000f);

        Matrix4x4 view = clientContext.camera.GetViewMatrix();
        DrawSky(view, projection);

        Shader.Use();
        Shader.SetInt("uTexture", 0);
        Shader.SetInt("uShadowMap", 1);
        Shader.SetInt("uWireframe", 0);

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

        DrawShadowPreview((uint)size.X, (uint)size.Y);
    }

    void DrawSky(Matrix4x4 view, Matrix4x4 projection)
    {
        if (!Matrix4x4.Invert(view, out var inverseView) || !Matrix4x4.Invert(projection, out var inverseProjection))
            return;

        var cameraPosition = new Vector3(inverseView.M41, inverseView.M42, inverseView.M43);

        clientContext.Gl.DepthMask(false);
        clientContext.Gl.Disable(EnableCap.DepthTest);
        clientContext.Gl.Disable(EnableCap.CullFace);

        SkyShader.Use();
        SkyShader.SetMatrix4("uInverseView", inverseView);
        SkyShader.SetMatrix4("uInverseProjection", inverseProjection);
        SkyShader.SetVector3("uCameraPos", cameraPosition);
        clientContext.Gl.BindVertexArray(DebugQuadVao);
        clientContext.Gl.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

        clientContext.Gl.Enable(EnableCap.DepthTest);
        clientContext.Gl.Enable(EnableCap.CullFace);
        clientContext.Gl.DepthMask(true);
    }

    void DrawShadowPreview(uint windowWidth, uint windowHeight)
    {
        uint previewSize = Math.Min(ShadowPreviewSize, Math.Min(windowWidth, windowHeight));
        int padding = 16;
        int previewX = Math.Max(0, (int)(windowWidth - previewSize) - padding);

        clientContext.Gl.Viewport(previewX, padding, previewSize, previewSize);
        clientContext.Gl.Disable(EnableCap.DepthTest);
        clientContext.Gl.Disable(EnableCap.CullFace);

        DebugDepthShader.Use();
        DebugDepthShader.SetInt("uDepthTexture", 0);
        clientContext.Gl.ActiveTexture(TextureUnit.Texture0);
        clientContext.Gl.BindTexture(TextureTarget.Texture2D, ShadowMap);
        clientContext.Gl.BindVertexArray(DebugQuadVao);
        clientContext.Gl.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

        clientContext.Gl.Enable(EnableCap.DepthTest);
        clientContext.Gl.Enable(EnableCap.CullFace);
        clientContext.Gl.Viewport(0, 0, windowWidth, windowHeight);
    }

    public void CleanUp()
    {
        CubeDrawInstruction.CleanUp(clientContext);
        BillboardDrawInstruction.CleanUp(clientContext);
        HeightmapDrawInstruction.CleanUp(clientContext);

        Atlas.Dispose();
        Shader.Dispose();
        SkyShader.Dispose();
        ShadowShader.Dispose();
        DebugDepthShader.Dispose();
        clientContext.Gl.DeleteFramebuffer(ShadowFbo);
        clientContext.Gl.DeleteTexture(ShadowMap);
        clientContext.Gl.DeleteVertexArray(DebugQuadVao);
        clientContext.Gl.DeleteBuffer(DebugQuadVbo);
    }
}