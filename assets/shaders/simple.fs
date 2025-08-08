#version 330 core

in vec2 fragTexCoords;
flat in int fragDirection;

out vec4 out_color;

uniform sampler2D uTexture;

float getShade(int dir) {
  switch (dir) {
  case 0:
    return 1.0f;
  case 1:
  case 3:
    return 0.8f;
  case 2:
  case 4:
    return 0.6f;
  case 5:
    return 0.5f;
  default:
    return 1.0f;
  }
}

void main() {
  // out_color = vec4(fragTexCoords.x, fragTexCoords.y, 0.0, 1.0);
  vec4 color = texture(uTexture, fragTexCoords);
  out_color = vec4(color.rgb * getShade(fragDirection), color.a);
}