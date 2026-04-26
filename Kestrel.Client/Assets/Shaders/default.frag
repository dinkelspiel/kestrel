#version 330 core
in vec2 vTexCoord;
in vec4 vLightSpacePos;
in vec3 vWorldPos;

out vec4 FragColor;

uniform sampler2D uTexture;
uniform sampler2D uShadowMap;
uniform vec3 uSunDirection;

float getShadow() {
  vec3 proj = vLightSpacePos.xyz / vLightSpacePos.w;
  proj = proj * 0.5 + 0.5;

  if (proj.x < 0.0 || proj.x > 1.0 || proj.y < 0.0 || proj.y > 1.0 ||
      proj.z < 0.0 || proj.z > 1.0)
    return 1.0;

  float closestDepth = texture(uShadowMap, proj.xy).r;
  float currentDepth = proj.z;
  float bias =
      max(0.0004, max(abs(dFdx(currentDepth)), abs(dFdy(currentDepth))) * 4.0);

  return currentDepth > closestDepth + bias ? 0.75 : 1.0;
}

float getFaceLight() {
  vec3 normal = normalize(cross(dFdx(vWorldPos), dFdy(vWorldPos)));
  if (!gl_FrontFacing)
    normal = -normal;

  return dot(normal, normalize(uSunDirection)) > 0.0 ? 1.0 : 0.75;
}

void main() {
  vec4 color = texture(uTexture, vTexCoord);
  if (color.a < 0.1)
    discard;

  float light = min(getShadow(), getFaceLight());
  FragColor = vec4(color.rgb * light, color.a);
}