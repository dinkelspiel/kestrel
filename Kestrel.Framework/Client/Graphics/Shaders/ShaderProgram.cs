using Silk.NET.OpenGL;

namespace Kestrel.Framework.Client.Graphics.Shaders;

public class ShaderProgram
{
    private readonly GL gl;
    private Shader[]? _attachedShaders;

    public uint GlProgram
    { private set; get; }

    public ShaderProgram(GL gl, params Shader[] shaders)
    {
        this.gl = gl;
        GlProgram = gl.CreateProgram();

        Attach(shaders);
        Link();
        DetachAll();
    }

    public void Attach(params Shader[] shaders)
    {
        _attachedShaders = shaders;
        foreach (var shader in shaders)
            gl.AttachShader(GlProgram, shader.GlShader);
    }

    public void DetachAll()
    {
        if (_attachedShaders == null)
            throw new ArgumentNullException("Attached shaders can't be null when calling DetachAll");

        foreach (var shader in _attachedShaders)
            gl.DetachShader(GlProgram, shader.GlShader);

        foreach (var shader in _attachedShaders)
            shader.Delete();

    }

    public void Link()
    {
        gl.LinkProgram(GlProgram);

        gl.GetProgram(GlProgram, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int)GLEnum.True)
            throw new Exception("Program failed to link: " + gl.GetProgramInfoLog(GlProgram));
    }

    public void Use()
    {
        gl.UseProgram(GlProgram);
    }

    public int GetUniformLocation(string uniform)
    {
        return gl.GetUniformLocation(GlProgram, uniform);
    }
}