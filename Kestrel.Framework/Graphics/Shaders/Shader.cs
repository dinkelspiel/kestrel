using Kestrel.Framework.Utils;
using Silk.NET.OpenGL;

namespace Kestrel.Framework.Graphics.Shaders;

public class Shader
{
    private GL gl;

    public uint GlShader
    { private set; get; }

    public Shader(GL gl, string path, ShaderType shaderType)
    {
        this.gl = gl;

        GlShader = gl.CreateShader(shaderType);
        string code = File.ReadAllText(Paths.InAssets(path));
        gl.ShaderSource(GlShader, code);
        gl.CompileShader(GlShader);

        gl.GetShader(GlShader, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int)GLEnum.True)
            throw new Exception("Shader failed to compile: " + gl.GetShaderInfoLog(GlShader));

    }

    public void Delete() =>
        gl.DeleteShader(GlShader);
}