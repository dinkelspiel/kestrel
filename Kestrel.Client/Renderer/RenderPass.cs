using System.Numerics;
using Kestrel.Client.ECS;
using Silk.NET.OpenGL;
using Arch.Core.Extensions;
using Kestrel.Client.Mesh;
using Kestrel.Client.MMath;

namespace Kestrel.Client.Renderer;

public class RenderPass(ClientContext clientContext)
{
    public Shader ShadowShader = null!;
    public uint ShadowFbo;
    public uint ShadowMap;
    const uint ShadowSize = 10240;
    const uint ShadowPreviewSize = 256;

    public uint CameraDepthFbo;
    public uint CameraDepthMap;
    public uint CameraNormalMap;
    public uint TerrainNoiseMap;
    const uint CameraDepthSize = 10240;
    const uint CameraDepthPreviewSize = 256;


    public Shader SkyShader = null!;
    public Shader DebugDepthShader = null!;
    public Shader DebugTextureShader = null!;
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
        TileSize = new Vector2(16f / Atlas.Width, 16f / Atlas.Height);
        TerrainNoiseMap = CreateTerrainNoiseTexture(HeightmapDrawInstruction.Size);
        var shadersDir = Path.Combine(AppContext.BaseDirectory, "Assets", "Shaders"); ;
        SkyShader = Shader.FromFiles(clientContext.Gl, Path.Combine(shadersDir, "debug_depth.vert"), Path.Combine(shadersDir, "sky.frag"));
        Shader.SetupRegularShaders(clientContext, shadersDir);

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

        // Camera Depth
        CameraDepthFbo = clientContext.Gl.GenFramebuffer();
        CameraDepthMap = clientContext.Gl.GenTexture();
        CameraNormalMap = clientContext.Gl.GenTexture();

        clientContext.Gl.BindTexture(TextureTarget.Texture2D, CameraDepthMap);
        clientContext.Gl.TexImage2D(
            TextureTarget.Texture2D,
            0,
            InternalFormat.DepthComponent,
            CameraDepthSize,
            CameraDepthSize,
            0,
            PixelFormat.DepthComponent,
            PixelType.Float,
            null);
        clientContext.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
        clientContext.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        clientContext.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
        clientContext.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
        fixed (float* borderColorPtr = borderColor)
            clientContext.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColorPtr);

        clientContext.Gl.BindTexture(TextureTarget.Texture2D, CameraNormalMap);
        clientContext.Gl.TexImage2D(
            TextureTarget.Texture2D,
            0,
            InternalFormat.Rgba16f,
            CameraDepthSize,
            CameraDepthSize,
            0,
            PixelFormat.Rgba,
            PixelType.Float,
            null);
        clientContext.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
        clientContext.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        clientContext.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        clientContext.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        clientContext.Gl.BindFramebuffer(FramebufferTarget.Framebuffer, CameraDepthFbo);
        clientContext.Gl.FramebufferTexture2D(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D,
            CameraNormalMap,
            0);
        clientContext.Gl.FramebufferTexture2D(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.DepthAttachment,
            TextureTarget.Texture2D,
            CameraDepthMap,
            0);
        clientContext.Gl.DrawBuffer(DrawBufferMode.ColorAttachment0);
        clientContext.Gl.ReadBuffer(ReadBufferMode.None);
        clientContext.Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        // Debug
        DebugDepthShader = Shader.FromFiles(
            clientContext.Gl,
            Path.Combine(shadersDir, "debug_depth.vert"),
            Path.Combine(shadersDir, "debug_depth.frag"));

        DebugTextureShader = Shader.FromFiles(
            clientContext.Gl,
            Path.Combine(shadersDir, "debug_depth.vert"),
            Path.Combine(shadersDir, "debug_texture.frag"));

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

    public void Draw(IDrawInstruction drawInstruction)
    {
        drawInstructions.Enqueue(drawInstruction);
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

        // clientContext.Gl.Viewport(0, 0, ShadowSize, ShadowSize);
        // clientContext.Gl.BindFramebuffer(FramebufferTarget.Framebuffer, ShadowFbo);
        // clientContext.Gl.Clear(ClearBufferMask.DepthBufferBit);

        // ShadowShader.Use();
        // ShadowShader.SetInt("uTexture", 0);
        // ShadowShader.SetMatrix4("uLightView", lightView);
        // ShadowShader.SetMatrix4("uLightProjection", lightProjection);
        // Atlas.Bind(TextureUnit.Texture0);

        // foreach (IDrawInstruction drawInstruction in drawInstructions)
        // {
        //     drawInstruction.Draw(lightView, lightProjection, ShadowShader);
        // }

        // clientContext.Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        // clientContext.Gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);

        // Camera Depth
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(
            MathF.PI / 180f * 65f,
            (float)size.X / size.Y,
            0.1f, 1000f);

        Matrix4x4 view = clientContext.camera.GetViewMatrix();
        clientContext.Gl.Viewport(0, 0, CameraDepthSize, CameraDepthSize);
        clientContext.Gl.BindFramebuffer(FramebufferTarget.Framebuffer, CameraDepthFbo);
        clientContext.Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        ShadowShader.Use();
        ShadowShader.SetInt("uTexture", 0);
        ShadowShader.SetMatrix4("uLightView", view);
        ShadowShader.SetMatrix4("uLightProjection", projection);
        Atlas.Bind(TextureUnit.Texture0);

        foreach (IDrawInstruction drawInstruction in drawInstructions)
        {
            if (drawInstruction is not ModelDrawInstruction)
                Atlas.Bind(TextureUnit.Texture0);

            drawInstruction.Draw(view, projection, ShadowShader);
        }

        clientContext.Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        clientContext.Gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);

        // Regular
        DrawSky(view, projection);

        foreach (var group in drawInstructions.GroupBy((di) => di.GetShader()))
        {
            Shader shader = Shader.Shaders[group.Key]!;
            shader.Use();
            shader.SetInt("uTexture", 0);
            shader.SetInt("uShadowMap", 1);
            shader.SetInt("uCameraDepthMap", 2);
            shader.SetInt("uCameraNormalMap", 3);
            shader.SetInt("uTerrainNoiseMap", 4);
            shader.SetInt("uWireframe", 0);

            Atlas.Bind(TextureUnit.Texture0);

            clientContext.Gl.ActiveTexture(TextureUnit.Texture1);
            clientContext.Gl.BindTexture(TextureTarget.Texture2D, ShadowMap);

            clientContext.Gl.ActiveTexture(TextureUnit.Texture2);
            clientContext.Gl.BindTexture(TextureTarget.Texture2D, CameraDepthMap);

            clientContext.Gl.ActiveTexture(TextureUnit.Texture3);
            clientContext.Gl.BindTexture(TextureTarget.Texture2D, CameraNormalMap);

            clientContext.Gl.ActiveTexture(TextureUnit.Texture4);
            clientContext.Gl.BindTexture(TextureTarget.Texture2D, TerrainNoiseMap);

            shader.SetMatrix4("uLightView", lightView);
            shader.SetMatrix4("uLightProjection", lightProjection);
            shader.SetVector3("uSunDirection", Vector3.Normalize(sunPosition - sceneCenter));

            foreach (IDrawInstruction drawInstruction in group)
            {
                if (drawInstruction is not ModelDrawInstruction)
                    Atlas.Bind(TextureUnit.Texture0);

                drawInstruction.Draw(view, projection, shader);
            }
        }

        // DrawShadowPreview((uint)size.X, (uint)size.Y);
    }

    unsafe uint CreateTerrainNoiseTexture(int textureSize)
    {
        var noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFrequency(0.04f);

        byte[] data = new byte[textureSize * textureSize];
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float value = noise.GetNoise(x, y) * 0.5f + 0.5f;
                // if (value > 0.5)
                //     data[y * textureSize + x] = 255;
                // else
                //     data[y * textureSize + x] = 0;
                data[y * textureSize + x] = (byte)Math.Clamp(value * 255f, 0f, 255f);
            }
        }

        uint texture = clientContext.Gl.GenTexture();
        clientContext.Gl.BindTexture(TextureTarget.Texture2D, texture);
        fixed (byte* ptr = data)
            clientContext.Gl.TexImage2D(
                TextureTarget.Texture2D,
                0,
                InternalFormat.R8,
                (uint)textureSize,
                (uint)textureSize,
                0,
                PixelFormat.Red,
                PixelType.UnsignedByte,
                ptr);

        clientContext.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        clientContext.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        clientContext.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        clientContext.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        return texture;
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
        int depthPreviewX = Math.Max(0, (int)(windowWidth - previewSize) - padding);
        int normalPreviewX = Math.Max(0, depthPreviewX - (int)previewSize - padding);

        clientContext.Gl.Disable(EnableCap.DepthTest);
        clientContext.Gl.Disable(EnableCap.CullFace);

        clientContext.Gl.Viewport(depthPreviewX, padding, previewSize, previewSize);
        DebugDepthShader.Use();
        DebugDepthShader.SetInt("uDepthTexture", 0);
        clientContext.Gl.ActiveTexture(TextureUnit.Texture0);
        clientContext.Gl.BindTexture(TextureTarget.Texture2D, CameraDepthMap);
        clientContext.Gl.BindVertexArray(DebugQuadVao);
        clientContext.Gl.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

        clientContext.Gl.Viewport(normalPreviewX, padding, previewSize, previewSize);
        DebugTextureShader.Use();
        DebugTextureShader.SetInt("uTexture", 0);
        clientContext.Gl.ActiveTexture(TextureUnit.Texture0);
        clientContext.Gl.BindTexture(TextureTarget.Texture2D, CameraNormalMap);
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
        SkyShader.Dispose();
        ShadowShader.Dispose();
        DebugDepthShader.Dispose();
        DebugTextureShader.Dispose();
        clientContext.Gl.DeleteFramebuffer(ShadowFbo);
        clientContext.Gl.DeleteTexture(ShadowMap);
        clientContext.Gl.DeleteFramebuffer(CameraDepthFbo);
        clientContext.Gl.DeleteTexture(CameraDepthMap);
        clientContext.Gl.DeleteTexture(CameraNormalMap);
        clientContext.Gl.DeleteTexture(TerrainNoiseMap);
        clientContext.Gl.DeleteVertexArray(DebugQuadVao);
        clientContext.Gl.DeleteBuffer(DebugQuadVbo);
    }
}