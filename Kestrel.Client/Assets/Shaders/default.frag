#version 330 core
in vec2 vTexCoord;
in vec4 vLightSpacePos;
in vec3 vWorldPos;
in vec4 vClipPos;

out vec4 FragColor;

uniform sampler2D uTexture;
uniform sampler2D uShadowMap;
uniform sampler2D uCameraDepthMap;

const float outlineThreshold = 0.00008;
const float outlineWidth = 3.0 / 1024.0;

float depthEdge(vec2 uv) {
  float d = texture(uCameraDepthMap, uv).r;
  float d1 = texture(uCameraDepthMap, uv + vec2(outlineWidth, 0.0)).r;
  float d2 = texture(uCameraDepthMap, uv + vec2(-outlineWidth, 0.0)).r;
  float d3 = texture(uCameraDepthMap, uv + vec2(0.0, outlineWidth)).r;
  float d4 = texture(uCameraDepthMap, uv + vec2(0.0, -outlineWidth)).r;
  float maxDiff =
      max(max(abs(d - d1), abs(d - d2)), max(abs(d - d3), abs(d - d4)));
  return maxDiff > outlineThreshold ? 1.0 : 0.0;
}
uniform vec3 uSunDirection;
uniform int uWireframe;

float shadowVisibility(vec3 proj) {
  if (proj.x < 0.0 || proj.x > 1.0 || proj.y < 0.0 || proj.y > 1.0 ||
      proj.z < 0.0 || proj.z > 1.0)
    return 1.0;

  vec3 normal = normalize(cross(dFdx(vWorldPos), dFdy(vWorldPos)));
  float slope = 1.0 - abs(dot(normal, normalize(uSunDirection)));
  float bias = max(0.00025 * slope, 0.00003);
  vec2 texelSize = 1.0 / vec2(textureSize(uShadowMap, 0));

  float visibility = 0.0;
  for (int x = -1; x <= 1; x++) {
    for (int y = -1; y <= 1; y++) {
      float closestDepth =
          texture(uShadowMap, proj.xy + vec2(x, y) * texelSize).r;
      visibility += proj.z - bias <= closestDepth ? 1.0 : 0.0;
    }
  }

  return visibility / 9.0;
}

void main() {
  vec4 color = texture(uTexture, vTexCoord);
  if (color.a < 0.1)
    discard;

  vec3 proj = vLightSpacePos.xyz / vLightSpacePos.w;
  proj = proj * 0.5 + 0.5;
  float visibility = shadowVisibility(proj);

  if (vWorldPos.y < 0.5) {
    color.r = 155.0 / 255.0;
    color.g = 177.0 / 255.0;
    color.b = 152.0 / 255.0;
  }

  if (vWorldPos.y < 0) {
    vec3 waterColor = vec3(86.0 / 255.0, 128.0 / 255.0, 129.0 / 255.0);

    FragColor = vec4(mix(waterColor.xyz, color.rgb, 0.5), 1.0);
    // mix(waterColor.xyz, color.rgb, 0.5) * mix(0.8, 1.0, visibility), 1.0);
    return;
  }

  vec2 depthUv = (vClipPos.xy / vClipPos.w) * 0.5 + 0.5;
  float outline = depthEdge(depthUv);
  FragColor = vec4(mix(color.rgb, color.rgb * 0.8, outline), color.a);
  //   FragColor = vec4(color.rgb * mix(0.8, 1.0, visibility), color.a);
}