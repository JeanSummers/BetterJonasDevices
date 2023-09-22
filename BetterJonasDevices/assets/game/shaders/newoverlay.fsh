#version 330 core

in vec2 uv;

out vec4 outColor;

uniform float time;
uniform int height;

void main () {
    float lines = 1 - mod(floor(height * uv.y / 2) / 2, 2);
    outColor = vec4(0.1, lines, 0.1, lines);

    outColor.a = clamp(outColor.a, 0, 0.9);

    float radiusShadow = clamp(distance(uv, vec2(.5)), 0, 1);
    outColor.a = clamp(outColor.a - radiusShadow, 0, 0.9);

    // float linePos = height - mod(20*time, height);
    // float dist = abs(linePos - height * uv.y);
    // float lineStrength = 0.8 * (clamp(dist / (height / 20), 0, 1));

    

    // float radiusShadow = clamp(1.35 - distance(uv, vec2(.5)), 0, 1);

    // float alpha = clamp(mix(lines, lineStrength, 0.5) - radiusShadow, 0, 0.9);

    
}