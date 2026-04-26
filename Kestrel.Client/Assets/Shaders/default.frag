#version 330 core
in vec2 vTexCoord;
in vec4 vLightSpacePos;

out vec4 FragColor;

uniform sampler2D uTexture;
uniform sampler2D uShadowMap;

float getShadow() {
  vec3 proj = vLightSpacePos.xyz / vLightSpacePos.w;
  proj = proj * 0.5 + 0.5;

  if (proj.x < 0.0 || proj.x > 1.0 || proj.y < 0.0 || proj.y > 1.0 ||
      proj.z < 0.0 || proj.z > 1.0)
    return 1.0;

  float closestDepth = texture(uShadowMap, proj.xy).r;
  float currentDepth = proj.z;
  float bias = 0.003;

  return currentDepth > closestDepth + bias ? 0.75 : 1.0;
}

void main() {
  vec4 color = texture(uTexture, vTexCoord);
  if (color.a < 0.1)
    discard;

  float shadow = getShadow();
  FragColor = vec4(color.rgb * shadow, color.a);
}