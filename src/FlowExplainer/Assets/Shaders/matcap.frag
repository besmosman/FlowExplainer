#version 460

in vec2 uv;
in vec3 normal;
in vec4 vertexColor;
in flat uint id;
in flat uint flags;
out vec4 color;

uniform sampler2D matcap;
uniform vec3 incoming;

// from https://github.com/hughsk/matcap/blob/master/matcap.glsl
vec2 matcapCoords(vec3 eye, vec3 normal) 
{
    vec3 reflected = reflect(eye, normal);
    float m = 2.8284271247461903 * sqrt(reflected.z+1.0);
    return reflected.xy / m + 0.5;
}

void main()
{
	if(flags == 2)
		discard;

    vec4 col = texture(matcap, matcapCoords(incoming, normal));
    col.a = 1;
    color = col;
}