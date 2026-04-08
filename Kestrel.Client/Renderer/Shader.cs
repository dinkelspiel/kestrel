using System.Numerics;
using Silk.NET.OpenGL;

namespace Kestrel.Client.Renderer;

public class Shader : IDisposable
{
    readonly GL _gl;
    readonly uint _handle;

    public uint Handle => _handle;

    public Shader(GL gl, string vertexSource, string fragmentSource)
    {
        _gl = gl;

        uint vert = Compile(ShaderType.VertexShader, vertexSource);
        uint frag = Compile(ShaderType.FragmentShader, fragmentSource);

        _handle = _gl.CreateProgram();
        _gl.AttachShader(_handle, vert);
        _gl.AttachShader(_handle, frag);
        _gl.LinkProgram(_handle);
        _gl.GetProgram(_handle, ProgramPropertyARB.LinkStatus, out int status);
        if (status == 0)
            throw new Exception($"Shader link error: {_gl.GetProgramInfoLog(_handle)}");

        _gl.DetachShader(_handle, vert);
        _gl.DetachShader(_handle, frag);
        _gl.DeleteShader(vert);
        _gl.DeleteShader(frag);
    }

    public static Shader FromFiles(GL gl, string vertexPath, string fragmentPath)
    {
        return new Shader(gl, File.ReadAllText(vertexPath), File.ReadAllText(fragmentPath));
    }

    public void Use() => _gl.UseProgram(_handle);

    public unsafe void SetMatrix4(string name, Matrix4x4 mat)
    {
        int loc = _gl.GetUniformLocation(_handle, name);
        _gl.UniformMatrix4(loc, 1, false, (float*)&mat);
    }

    public void SetInt(string name, int value)
    {
        int loc = _gl.GetUniformLocation(_handle, name);
        _gl.Uniform1(loc, value);
    }

    public void SetFloat(string name, float value)
    {
        int loc = _gl.GetUniformLocation(_handle, name);
        _gl.Uniform1(loc, value);
    }

    public void SetVector3(string name, Vector3 value)
    {
        int loc = _gl.GetUniformLocation(_handle, name);
        _gl.Uniform3(loc, value.X, value.Y, value.Z);
    }

    public void SetVector2(string name, Vector2 value)
    {
        int loc = _gl.GetUniformLocation(_handle, name);
        _gl.Uniform2(loc, value.X, value.Y);
    }

    public void Dispose()
    {
        _gl.DeleteProgram(_handle);
        GC.SuppressFinalize(this);
    }

    uint Compile(ShaderType type, string src)
    {
        uint handle = _gl.CreateShader(type);
        _gl.ShaderSource(handle, src);
        _gl.CompileShader(handle);
        _gl.GetShader(handle, ShaderParameterName.CompileStatus, out int status);
        if (status == 0)
            throw new Exception($"Shader compile error ({type}): {_gl.GetShaderInfoLog(handle)}");
        return handle;
    }
}
