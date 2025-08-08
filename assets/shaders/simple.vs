#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTextureCoord;
layout(location = 2) in float aDirection;

out vec2 fragTexCoords;
flat out int fragDirection;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main() {
  gl_Position = projection * view * model * vec4(aPosition, 1.0);
  fragTexCoords = aTextureCoord;
  fragDirection = int(aDirection + 0.5f);
}