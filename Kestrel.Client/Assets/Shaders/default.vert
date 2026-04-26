#version 330 core
layout(location = 0) in vec3 aPos;
layout(location = 1) in vec2 aTexCoord;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;
uniform vec2 uTileOffset;
uniform vec2 uTileSize;
uniform mat4 uLightView;
uniform mat4 uLightProjection;

out vec2 vTexCoord;
out vec2 vLocalTexCoord;
out vec4 vLightSpacePos;
out vec3 vWorldPos;

void main() {
  vec4 worldPos = uModel * vec4(aPos, 1.0);
  gl_Position = uProjection * uView * worldPos;
  vTexCoord = uTileOffset + aTexCoord * uTileSize;
  vLocalTexCoord = aTexCoord;
  vLightSpacePos = uLightProjection * uLightView * worldPos;
  vWorldPos = worldPos.xyz;
}
