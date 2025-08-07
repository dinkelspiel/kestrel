using Silk.NET.OpenGL;

namespace Kestrel.Framework.Graphics.Shaders;

public class VertexShader(GL gl, string path) : Shader(gl, path, ShaderType.VertexShader)
{
}