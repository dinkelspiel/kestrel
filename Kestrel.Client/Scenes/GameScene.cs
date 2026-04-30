using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Kestrel.Client.ECS;
using Kestrel.Client.Mesh;
using Kestrel.Client.Prefab;
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
    public Entity? Player = null;
    public Entity? Tree = null;

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
        var prefabsDir = Path.Combine(AppContext.BaseDirectory, "Assets", "Prefabs", "Player.pfb");
        PrefabConfig.FromFile(prefabsDir);

        Player = clientContext.World.Create(new PlayerTag(), new TransformComponent(new(220, 0, 220), 0.4f), new VelocityComponent(), new HeightmapColliderComponent(), new ModelRendererComponent(new(clientContext, Path.Combine(AppContext.BaseDirectory, "Assets", "player.obj"), Path.Combine(AppContext.BaseDirectory, "Assets", "player.png"))));
        Tree = clientContext.World.Create(new TransformComponent(new(220, HeightmapDrawInstruction.Heightmap[220, 220], 220), 0.05f), new ModelRendererComponent(new(clientContext, Path.Combine(AppContext.BaseDirectory, "Assets", "asd.obj"), Path.Combine(AppContext.BaseDirectory, "Assets", "leaf.png"))));
        clientContext.Player = Player;
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
        // renderPass.DrawCube(Matrix4x4.Identity * Matrix4x4.CreateTranslation(0, -1, 0), (0, 0));
        // renderPass.DrawBillboard(Matrix4x4.Identity * Matrix4x4.CreateTranslation(0, 0, 0), (2, 0));

        clientContext.World.Query(new QueryDescription().WithAll<ModelRendererComponent, TransformComponent>(), (ref TransformComponent transform, ref ModelRendererComponent modelRenderer) =>
        {
            float pYawRad = MathF.PI / 180f * transform.Yaw;
            var model = Matrix4x4.CreateScale(transform.Scale) * Matrix4x4.CreateRotationY(MathF.PI - pYawRad + MathF.PI / 2 + MathF.PI) * Matrix4x4.CreateTranslation(transform.Postition);
            modelRenderer.Model.Transform = model;
            renderPass.Draw(modelRenderer.Model);
        });

        renderPass.DrawHeightmap(Matrix4x4.Identity, (0, 0));

        renderPass.Draw(grassDrawInstruction);

        renderPass.End();
    }

    public override void Unload()
    {
        clientContext.Mouse.MouseMove -= OnMouseMove;
        renderPass.CleanUp();

        // clientContext.World.Query(new QueryDescription().WithAll<ModelRendererComponent>(), (ref ModelRendererComponent modelRenderer) =>
        // {
        //     modelRenderer.Model.Dispose();
        // });
    }

    void OnMouseMove(IMouse mouse, Vector2 position)
    {
        camera.OnMouseMove(mouse, position);
    }
}
