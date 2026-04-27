#version 330 core
in vec2 vTexCoord;
in vec3 vWorldPos;

out vec4 FragColor;

uniform sampler2D uTexture;

void main() {
  if (texture(uTexture, vTexCoord).a < 0.1)
    discard;

  vec3 normal = normalize(cross(dFdx(vWorldPos), dFdy(vWorldPos)));
  FragColor = vec4(normal * 0.5 + 0.5, 1.0);
}