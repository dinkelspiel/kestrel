#version 330 core
layout(location = 0) in vec3 aPos;
layout(location = 1) in vec2 aTexCoord;

uniform mat4 uModel;
uniform mat4 uLightView;
uniform mat4 uLightProjection;
uniform vec2 uTileOffset;
uniform vec2 uTileSize;

out vec2 vTexCoord;

void main() {
  gl_Position = uLightProjection * uLightView * uModel * vec4(aPos, 1.0);
  vTexCoord = uTileOffset + aTexCoord * uTileSize;
}