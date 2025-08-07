#version 330 core

in vec2 fragTexCoords;

out vec4 out_color;

uniform sampler2D uTexture;

void main() {
  // out_color = vec4(fragTexCoords.x, fragTexCoords.y, 0.0, 1.0);
  out_color = texture(uTexture, fragTexCoords);
}