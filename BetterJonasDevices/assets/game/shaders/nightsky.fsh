#version 330 core
#extension GL_ARB_explicit_attrib_location: enable

in vec3 texCoords;
in float worldPosY;
in float nightVisonStrengthv;

uniform vec4 rgbaFog;
uniform samplerCube ctex;
uniform int ditherSeed;
uniform int horizontalResolution;
uniform float dayLight;
uniform float horizonFog;
uniform float playerToSealevelOffset;
uniform float fogDensityIn;
uniform float fogMinIn;


out vec4 outColor;
#if SSAOLEVEL > 0
layout(location = 2) out vec4 outGNormal;
layout(location = 3) out vec4 outGPosition;
#endif


#include nightvision.ash
#include dither.fsh
#include fogandlight.fsh

void main () {
	vec4 skyCol = texture (ctex, texCoords) + NoiseFromPixelPosition(ivec2(gl_FragCoord.xy), ditherSeed, horizontalResolution);
	skyCol -= 0.03f;
	skyCol.rgb *= 2;
	skyCol.a = max(0, 1 - 2*(dayLight - 0.05));
	
	outColor = skyCol;
	outColor.rgb += nightVisionLight() * nightVisonStrengthv;
	
#if SSAOLEVEL > 0
	outGPosition = vec4(0);
	outGNormal = vec4(0);
#endif

}