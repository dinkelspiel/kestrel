#version 330 core
in vec2 vTexCoord;

uniform sampler2D uTexture;

void main() {
  if (texture(uTexture, vTexCoord).a < 0.1)
    discard;
}