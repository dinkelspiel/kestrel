namespace Kestrel.Framework.Client.Graphics;

using Silk.NET.Maths;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

public class Window(GL gl, int width, int height)
{
    public readonly GL GL = gl;
    public int Width { get; set; } = width;
    public int Height { get; set; } = height;

    public void OnResize(Vector2D<int> size)
    {
        GL.Viewport(0, 0, (uint)size.X, (uint)size.Y);
        Width = size.X;
        Height = size.Y;
    }
}