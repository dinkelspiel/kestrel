using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Kestrel.Client.ECS;
using Kestrel.Client.Mesh;
using Kestrel.Client.Renderer;
using Kestrel.Client.Scene;
using Silk.NET.Input;

namespace Kestrel.Client.Scenes;

public class GameScene(ClientContext clientContext) : SceneBase(clientContext)
{
    const float Gravity = 25f;
    const float JumpSpeed = 12f;
    public Camera camera = null!;
    public RenderPass renderPass = null!;
    public GrassDrawInstruction grassDrawInstruction = null!;
    public ModelDrawInstruction playerModel = null!;

    public override unsafe void Load()
    {
        var gl = clientContext.Gl;

        clientContext.Mouse.Cursor.CursorMode = CursorMode.Raw;
        clientContext.Mouse.MouseMove += OnMouseMove;

        camera = new(clientContext);
        clientContext.camera = camera;

        renderPass = new(clientContext);
        renderPass.Setup();

        grassDrawInstruction = new(clientContext, renderPass.TileSize, Matrix4x4.Identity, (2, 0), HeightmapDrawInstruction.Heightmap, HeightmapDrawInstruction.Size);
        grassDrawInstruction.Setup(clientContext);
        playerModel = new(
            clientContext,
            Path.Combine(AppContext.BaseDirectory, "Assets", "player.obj"));
    }

    public override void Update(double dt)
    {
        var input = clientContext.Input;
        float step = (float)dt;

        float yawRad = MathF.PI / 180f * camera.Yaw; var forward = Vector3.Normalize(new Vector3(MathF.Cos(yawRad), 0f, MathF.Sin(yawRad)));
        var right = Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitY));

        if (clientContext.TryGetPlayer(out var player))
        {
            var wish = Vector3.Zero;
            if (input.IsKeyDown(Key.W)) wish -= forward;
            if (input.IsKeyDown(Key.S)) wish += forward;
            if (input.IsKeyDown(Key.A)) wish += right;
            if (input.IsKeyDown(Key.D)) wish -= right;
            if (wish.LengthSquared() > 0f) wish = Vector3.Normalize(wish);

            ref var vel = ref player.Get<VelocityComponent>();
            vel.Velocity.X = wish.X * camera.Speed;
            vel.Velocity.Z = wish.Z * camera.Speed;
            if (input.IsKeyPressed(Key.Space) && player.Get<TransformComponent>().IsGrounded)
                vel.Velocity.Y = JumpSpeed;
            // if (input.IsKeyPressed(Key.Space))
            // if (input.IsKeyPressed(Key.ShiftLeft))
            // vel.Velocity.Y = 0;
            player.Get<TransformComponent>().Yaw = camera.Yaw;
        }

        clientContext.World.Query(new QueryDescription().WithAll<VelocityComponent, TransformComponent>(), (ref TransformComponent transform, ref VelocityComponent velocity) =>
        {
            velocity.Velocity.Y -= Gravity * step;
        });

        clientContext.World.Query(new QueryDescription().WithAll<TransformComponent, VelocityComponent, HeightmapColliderComponent>(), (ref TransformComponent transform, ref VelocityComponent velocity) =>
        {
            bool wasGrounded = transform.IsGrounded;
            transform.IsGrounded = false;

            float[,] heightmap = HeightmapDrawInstruction.Heightmap;
            int size = HeightmapDrawInstruction.Size;
            var nextPosition = transform.Postition + velocity.Velocity * step;
            float groundHeight = HeightmapDrawInstruction.SampleHeight(heightmap, size, nextPosition.X, nextPosition.Z) + 0.5f;

            float maxStepDown = 1.5f;
            bool canStickToGround = wasGrounded && velocity.Velocity.Y <= 0f;

            if (nextPosition.Y < groundHeight || (canStickToGround && nextPosition.Y - groundHeight <= maxStepDown))
            {
                nextPosition.Y = groundHeight;
                transform.IsGrounded = true;
                velocity.Velocity.Y = 0f;
            }

            transform.Postition = nextPosition;
        });

        clientContext.World.Query(new QueryDescription().WithAll<VelocityComponent, TransformComponent>().WithNone<HeightmapColliderComponent>(), (ref TransformComponent transform, ref VelocityComponent velocity) =>
        {
            transform.Postition += velocity.Velocity * step;
        });
    }

    public override void Render(double dt)
    {
        var gl = clientContext.Gl;
        renderPass.Begin();
        renderPass.DrawCube(Matrix4x4.Identity * Matrix4x4.CreateTranslation(0, -1, 0), (0, 0));
        renderPass.DrawBillboard(Matrix4x4.Identity * Matrix4x4.CreateTranslation(0, 0, 0), (2, 0));

        clientContext.World.Query(new QueryDescription().WithAll<PlayerTag, TransformComponent>(), (ref TransformComponent transform) =>
        {
            float pYawRad = MathF.PI / 180f * transform.Yaw;
            var model = Matrix4x4.CreateScale(0.4f) * Matrix4x4.CreateRotationY(MathF.PI - pYawRad + MathF.PI / 2 + MathF.PI) * Matrix4x4.CreateTranslation(transform.Postition);
            playerModel.Transform = model;
            renderPass.Draw(playerModel);
        });

        renderPass.DrawHeightmap(Matrix4x4.Identity, (0, 0));

        renderPass.Draw(grassDrawInstruction);

        renderPass.End();
    }

    public override void Unload()
    {
        clientContext.Mouse.MouseMove -= OnMouseMove;
        playerModel.Dispose();
        renderPass.CleanUp();
    }

    void OnMouseMove(IMouse mouse, Vector2 position)
    {
        camera.OnMouseMove(mouse, position);
    }
}
