#version 330 core
in vec2 vTexCoord;

out vec4 FragColor;

uniform mat4 uInverseView;
uniform mat4 uInverseProjection;
uniform vec3 uCameraPos;

void main() {
  vec2 ndc = vTexCoord * 2.0 - 1.0;
  vec4 viewPos = uInverseProjection * vec4(ndc, 1.0, 1.0);
  viewPos /= viewPos.w;

  vec3 rayDir = normalize((uInverseView * vec4(viewPos.xyz, 0.0)).xyz);
  float planeDistance = -uCameraPos.y / rayDir.y;
  float belowHorizon =
      step(0.0, planeDistance) * smoothstep(0.01, -0.01, rayDir.y);

  vec3 skyColor = vec3(0.588, 0.780, 0.769);
  vec3 belowColor = vec3(86.0 / 255.0, 128.0 / 255.0, 129.0 / 255.0);

  FragColor = vec4(mix(skyColor, belowColor, belowHorizon), 1.0);
}
