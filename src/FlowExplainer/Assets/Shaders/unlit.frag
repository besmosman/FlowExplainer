#version 460
in vec2 uv;
in vec3 normal;
in vec4 vertexColor;
uniform vec4 tint = vec4(1,1,1,1);
out vec4 color;

void main()
{
	color = vertexColor * tint;
	if(color.a == 0)
		discard;
}