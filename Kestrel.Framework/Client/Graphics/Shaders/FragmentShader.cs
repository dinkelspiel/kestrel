using Silk.NET.OpenGL;

namespace Kestrel.Framework.Client.Graphics.Shaders;

public class FragmentShader(GL gl, string path) : Shader(gl, path, ShaderType.FragmentShader)
{
}