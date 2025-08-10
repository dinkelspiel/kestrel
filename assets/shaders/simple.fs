#version 330 core

in vec2 fragTexCoords;
flat in int fragDirection;

out vec4 out_color;

uniform sampler2D uTexture;

/* --- FOG UNIFORMS --- */
uniform vec3 uFogColor; // fog color in linear RGB (e.g., vec3(0.7, 0.8, 0.9))
uniform float uFogDensity; // exp^2 fog density, e.g., 0.0015
uniform float uNear;       // camera near plane (must match your projection)
uniform float uFar;        // camera far  plane (must match your projection)

/* Shade helper (your original) */
float getShade(int dir) {
  switch (dir) {
  case 0:
    return 1.0;
  case 1:
  case 3:
    return 0.8;
  case 2:
  case 4:
    return 0.6;
  case 5:
    return 0.5;
  default:
    return 1.0;
  }
}

/* --- Depth linearization: converts hardware depth to view-space Z distance ---
 */
float linearizeDepth(float depth) {
  // depth is [0..1] after viewport transform.
  // Convert to NDC [-1..1], then invert the projection's non-linear mapping.
  float z = depth * 2.0 - 1.0; // NDC z
  return (2.0 * uNear * uFar) / (uFar + uNear - z * (uFar - uNear));
}

void main() {
  // 1) Sample base color and apply your per-face shading
  vec4 texColor = texture(uTexture, fragTexCoords);
  vec3 base = texColor.rgb * getShade(fragDirection);

  // 2) Reconstruct linear eye-space depth for this fragment
  float d =
      linearizeDepth(gl_FragCoord.z); // distance from camera in world units

  // 3) Exp^2 fog factor: f = exp( - (density * d)^2 )
  float fogFactor = exp(-(uFogDensity * d) * (uFogDensity * d));
  fogFactor = clamp(fogFactor, 0.0, 1.0);

  // 4) Mix towards fog color (when fogFactor -> 0, fully fogged)
  vec3 fogged = mix(uFogColor, base, fogFactor);

  out_color = vec4(fogged, texColor.a);
}
