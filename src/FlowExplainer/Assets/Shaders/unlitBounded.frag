#version 460
//@bounds.glsl

in vec2 uv;
in vec3 normal;
in vec3 lineTangent;
in vec4 vertexColor;
in vec3 worldPos;
in flat uint id;
in flat uint flags;
uniform vec4 tint = vec4(1,1,1,1);
out vec4 color;

void main()
{
	color = vertexColor * tint;
	if(color.a <= .01 || !withinBounds(worldPos))
		discard;
}