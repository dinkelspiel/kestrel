#version 330 core
in vec2 vTexCoord;
in vec4 vLightSpacePos;
in vec3 vWorldPos;
in vec4 vClipPos;

out vec4 FragColor;

uniform sampler2D uTexture;
uniform sampler2D uShadowMap;
uniform sampler2D uCameraDepthMap;
uniform sampler2D uCameraNormalMap;
uniform sampler2D uTerrainNoiseMap;
uniform int uIsHeightmap;
uniform mat4 uView;
vec3 cameraPos = inverse(uView)[3].xyz;
float fogNear = 80;
float fogFar = 150;
vec3 skyColor = vec3(0.588, 0.780, 0.769);
// vec3 skyColor = vec3(1, 0, 0);

const float outlineWidth = 12.0;
const float steepNormalThreshold = 0.80;
const float depthNear = 0.1;
const float depthFar = 1000.0;

float distanceBetweenPointAndCamera() {
  return sqrt(pow(vWorldPos.x - cameraPos.x, 2) +
              pow(vWorldPos.y - cameraPos.y, 2) +
              pow(vWorldPos.z - cameraPos.z, 2));
}

float linearizeDepth(float depth) {
  float z = depth * 2.0 - 1.0;
  return (2.0 * depthNear * depthFar) /
         (depthFar + depthNear - z * (depthFar - depthNear));
}

float depthEdge(vec2 uv) {
  vec2 texel = outlineWidth / vec2(textureSize(uCameraDepthMap, 0));
  float d = linearizeDepth(texture(uCameraDepthMap, uv).r);
  float d1 =
      linearizeDepth(texture(uCameraDepthMap, uv + vec2(texel.x, 0.0)).r);
  float d2 =
      linearizeDepth(texture(uCameraDepthMap, uv + vec2(-texel.x, 0.0)).r);
  float d3 =
      linearizeDepth(texture(uCameraDepthMap, uv + vec2(0.0, texel.y)).r);
  float d4 =
      linearizeDepth(texture(uCameraDepthMap, uv + vec2(0.0, -texel.y)).r);
  float maxDiff =
      max(max(abs(d - d1), abs(d - d2)), max(abs(d - d3), abs(d - d4)));
  float threshold = max(0.35, d * 0.025);
  return maxDiff > threshold ? 1.0 : 0.0;
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

  vec2 screenUv = (vClipPos.xy / vClipPos.w) * 0.5 + 0.5;
  vec3 cameraNormal =
      normalize(texture(uCameraNormalMap, screenUv).rgb * 2.0 - 1.0);
  float upDot = abs(dot(cameraNormal, vec3(0.0, 1.0, 0.0)));

  if (uIsHeightmap == 1) {
    if (upDot < steepNormalThreshold) {
      color.r = 155.0 / 255.0;
      color.g = 177.0 / 255.0;
      color.b = 152.0 / 255.0;
    } else {
      ivec2 noiseSize = textureSize(uTerrainNoiseMap, 0);
      ivec2 noiseCoord =
          clamp(ivec2(floor(vWorldPos.xz)), ivec2(0), noiseSize - ivec2(1));
      float grassNoise = texelFetch(uTerrainNoiseMap, noiseCoord, 0).r;
      vec3 grassLow = vec3(153.0 / 255.0, 167.0 / 255.0, 106.0 / 255.0);
      vec3 grassHigh = vec3(145.0 / 255.0, 161.0 / 255.0, 94.0 / 255.0);
      color.rgb = mix(grassLow, grassHigh, grassNoise);
    }
  }

  if (uIsHeightmap == 1 && vWorldPos.y < 0.5) {
    color = vec4(155.0 / 255.0, 177.0 / 255.0, 152.0 / 255.0, 1);
  }

  if (vWorldPos.y < 0) {
    vec3 waterColor = vec3(86.0 / 255.0, 128.0 / 255.0, 129.0 / 255.0);

    color = vec4(mix(waterColor.xyz, color.rgb, 0.5), 1.0);
    // mix(waterColor.xyz, color.rgb, 0.5) * mix(0.8, 1.0, visibility), 1.0);
  }

  float pixelDistanceToCamera = distanceBetweenPointAndCamera();
  if (pixelDistanceToCamera > fogNear) {
    pixelDistanceToCamera = pixelDistanceToCamera - fogNear;
    float delta = fogFar - fogNear;
    pixelDistanceToCamera = pixelDistanceToCamera / delta;
    color =
        vec4(mix(color.rgb, skyColor, clamp(pixelDistanceToCamera, 0, 1)), 1);
  }

  float outline =
      uIsHeightmap == 1 ? ((vWorldPos.y > 0) ? depthEdge(screenUv) : 0.0) : 0.0;
  FragColor = vec4(mix(color.rgb, color.rgb * 0.8, outline), color.a);
  //   FragColor = vec4(color.rgb * mix(0.8, 1.0, visibility), color.a);
}