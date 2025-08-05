using Kestrel.Framework.Shaders;
using Silk.NET.OpenGL;

namespace Kestrel.Framework.Buffers;

public class QuadMesh
{
    private uint _vao, _vbo;
    private ShaderProgram _shader;
    private GL _gl;

    public unsafe QuadMesh(GL _gl, ShaderProgram shader)
    {
        this._gl = _gl;
        this._shader = shader;
        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        float[] vertices =
        [
            0.5f,  0.5f, 0.0f,
            0.5f, -0.5f, 0.0f,
            -0.5f, -0.5f, 0.0f,
            -0.5f,  0.5f, 0.0f
        ];

        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        fixed (float* buf = vertices)
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);

        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*)0);
    }

    public unsafe void Draw()
    {
        _gl.BindVertexArray(_vao);
        _shader.Use();
        _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);
    }
}