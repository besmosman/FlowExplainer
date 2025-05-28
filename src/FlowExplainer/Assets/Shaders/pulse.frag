#version 460

in vec2 uv;
in vec3 normal;
in vec4 vertexColor;
in flat uint id;
in flat uint flags;
in float progress;
out vec4 color;

uniform float disabledAlpha;
uniform float time;

void main()
{
	color = vertexColor;
	if (flags == 2)
		color.a = disabledAlpha;

	color.rgb += vec3(1) * mod((progress * progress * progress) + time , 1);

	if(color.a == 0)
		discard;
}