using Silk.NET.OpenGL;

namespace Kestrel.Framework.Client.Graphics.Shaders;

public class VertexShader(GL gl, string path) : Shader(gl, path, ShaderType.VertexShader)
{
}