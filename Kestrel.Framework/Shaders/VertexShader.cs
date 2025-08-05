using Silk.NET.OpenGL;

namespace Kestrel.Framework.Shaders;

public class VertexShader(GL gl, string path) : Shader(gl, path, ShaderType.VertexShader)
{
}