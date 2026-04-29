#version 330 core
layout(location = 0) in vec3 aPos;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec3 aOffset;

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
out vec4 vClipPos;

void main() {
  vec4 worldPos = uModel * vec4(aPos, 1.0);
  worldPos.xyz += aOffset;
  gl_Position = uProjection * uView * worldPos;
  vClipPos = gl_Position;
  vTexCoord = uTileOffset + aTexCoord * uTileSize;
  vLocalTexCoord = aTexCoord;
  vLightSpacePos = uLightProjection * uLightView * worldPos;
  vWorldPos = worldPos.xyz;
}
