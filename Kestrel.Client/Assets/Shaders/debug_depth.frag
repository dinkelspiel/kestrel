#version 330 core
in vec2 vTexCoord;

out vec4 FragColor;

uniform sampler2D uDepthTexture;

void main() {
  float depth = texture(uDepthTexture, vTexCoord).r;
  FragColor = vec4(vec3(depth), 1.0);
}
