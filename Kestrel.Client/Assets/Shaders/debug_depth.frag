#version 330 core
in vec2 vTexCoord;

out vec4 FragColor;

uniform sampler2D uDepthTexture;

const float near = 0.1;
const float far = 1000.0;

float linearizeDepth(float d) {
  return (2.0 * near * far) / (far + near - d * (far - near));
}

void main() {
  float depth = texture(uDepthTexture, vTexCoord).r;
  float linear = linearizeDepth(depth * 2.0 - 1.0) / far;
  FragColor = vec4(vec3(linear), 1.0);
}
