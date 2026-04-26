#version 330 core
in vec2 vTexCoord;
in vec4 vLightSpacePos;
in vec3 vWorldPos;

out vec4 FragColor;

uniform sampler2D uTexture;
uniform sampler2D uShadowMap;
uniform vec3 uSunDirection;
uniform int uWireframe;

vec3 getWorldNormal() {
  vec3 normal = normalize(cross(dFdx(vWorldPos), dFdy(vWorldPos)));
  return gl_FrontFacing ? normal : -normal;
}

bool inShadow() {
  vec3 proj = vLightSpacePos.xyz / vLightSpacePos.w;
  proj = proj * 0.5 + 0.5;

  float closestDepth = texture(uShadowMap, proj.xy).r;
  float currentDepth = proj.z;

  return currentDepth > closestDepth;
}

float getFaceLight() {
  vec3 normal = getWorldNormal();

  return dot(normal, normalize(uSunDirection)) > 0.0 ? 1.0 : 0.75;
}

void main() {
  //   vec3 proj = vLightSpacePos.xyz / vLightSpacePos.w;
  //   proj = proj * 0.5 + 0.5;
  //   FragColor = texture(uShadowMap, proj.xy);
  //   return;

  if (uWireframe == 1) {
    FragColor = vec4(0.02, 0.02, 0.02, 1.0);
    return;
  }

  vec4 color = texture(uTexture, vTexCoord);
  if (color.a < 0.1)
    discard;

  vec3 proj = vLightSpacePos.xyz / vLightSpacePos.w;
  proj = proj * 0.5 + 0.5;

  FragColor = vec4(proj.zzz, color.a);
  return;

  FragColor = vec4(color.rgb, color.a);
}