using Silk.NET.OpenGL;

namespace Kestrel.Framework.Shaders;

public class FragmentShader(GL gl, string path) : Shader(gl, path, ShaderType.FragmentShader)
{
}