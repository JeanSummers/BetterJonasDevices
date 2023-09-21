#version 330 core

in vec2 uv;

out vec4 outColor;

uniform float time;
uniform int height;

void main () {
    float lines = (1 + cos(height * uv.y / 1.2 + time)) / 2;
    float radiusShadow = 1.35 - distance(uv, vec2(.5));
    float alpha = clamp(lines - radiusShadow, 0, 0.9);

    outColor = vec4(0.1, lines, 0.1, alpha);
}