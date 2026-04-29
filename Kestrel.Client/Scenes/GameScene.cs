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

    public override unsafe void Load()
    {
        var gl = clientContext.Gl;

        clientContext.Mouse.Cursor.CursorMode = CursorMode.Raw;
        clientContext.Mouse.MouseMove += OnMouseMove;

        camera = new(clientContext);
        clientContext.camera = camera;

        renderPass = new(clientContext);
        renderPass.Setup();
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
            transform.IsGrounded = false;
            velocity.Velocity.Y -= Gravity * step;
        });

        clientContext.World.Query(new QueryDescription().WithAll<TransformComponent, VelocityComponent, HeightmapColliderComponent>(), (ref TransformComponent transform, ref VelocityComponent velocity) =>
        {
            float[,] heightmap = HeightmapDrawInstruction.Heightmap;
            int size = HeightmapDrawInstruction.Size;
            var nextPosition = transform.Postition + velocity.Velocity * step;
            float groundHeight = SampleHeight(heightmap, size, nextPosition.X, nextPosition.Z) + 0.5f;

            if (nextPosition.Y < groundHeight)
            {
                nextPosition.Y = groundHeight;
                transform.IsGrounded = true;
                if (velocity.Velocity.Y < 0f)
                    velocity.Velocity.Y = 0f;
            }

            transform.Postition = nextPosition;
        });

        clientContext.World.Query(new QueryDescription().WithAll<VelocityComponent, TransformComponent>().WithNone<HeightmapColliderComponent>(), (ref TransformComponent transform, ref VelocityComponent velocity) =>
        {
            transform.Postition += velocity.Velocity * step;
        });
    }

    static float SampleHeight(float[,] heightmap, int size, float x, float y)
    {
        x = Math.Clamp(x, 0f, size - 1f);
        y = Math.Clamp(y, 0f, size - 1f);

        int x0 = (int)MathF.Floor(x);
        int y0 = (int)MathF.Floor(y);
        int x1 = Math.Min(x0 + 1, size - 1);
        int y1 = Math.Min(y0 + 1, size - 1);
        float tx = x - x0;
        float ty = y - y0;

        float h00 = heightmap[x0, y0];
        float h10 = heightmap[x1, y0];
        float h01 = heightmap[x0, y1];
        float h11 = heightmap[x1, y1];
        float hx0 = h00 + (h10 - h00) * tx;
        float hx1 = h01 + (h11 - h01) * tx;
        return hx0 + (hx1 - hx0) * ty;
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
            var model = Matrix4x4.CreateRotationY(MathF.PI - pYawRad) * Matrix4x4.CreateTranslation(transform.Postition);
            renderPass.DrawCube(model, (1, 0));
        });

        renderPass.DrawHeightmap(Matrix4x4.Identity, (0, 0));

        renderPass.End();
    }

    public override void Unload()
    {
        clientContext.Mouse.MouseMove -= OnMouseMove;
        renderPass.CleanUp();
    }

    void OnMouseMove(IMouse mouse, Vector2 position)
    {
        camera.OnMouseMove(mouse, position);
    }
}
