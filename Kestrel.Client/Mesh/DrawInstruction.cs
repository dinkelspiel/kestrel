using System.Numerics;
using Kestrel.Client.Renderer;

namespace Kestrel.Client.Mesh;

public interface IDrawInstruction
{
    ShaderKind GetShader();
    void Draw(Matrix4x4 view, Matrix4x4 projection, Shader shader);
}