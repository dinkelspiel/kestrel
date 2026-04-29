#version 330 core
in vec2 vTexCoord;
in vec3 vWorldPos;
in vec3 vWorldNormal;

out vec4 FragColor;

uniform sampler2D uTexture;
uniform int uIsHeightmap;

void main() {
  if (texture(uTexture, vTexCoord).a < 0.1)
    discard;

  vec3 faceNormal = normalize(cross(dFdx(vWorldPos), dFdy(vWorldPos)));
  vec3 normal = uIsHeightmap == 1 ? normalize(vWorldNormal) : faceNormal;
  FragColor = vec4(normal * 0.5 + 0.5, 1.0);
}