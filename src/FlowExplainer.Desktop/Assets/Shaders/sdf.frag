#version 460
in vec2 uv;
in vec3 normal;
in vec4 vertexColor;
uniform vec4 tint = vec4(1,1,1,1);
out vec4 color;

uniform vec2 WorldViewMin;
uniform vec2 WorldViewMax;
uniform vec2 GridSize;
uniform sampler2D colorgradient;

struct Sample {
	float Accum;
	float MinDis;
	vec2 padding;
};

layout(std430, binding = 2) buffer cellBuffer {
	Sample Entries[];
} samples;


int toIndex(vec2 coord) {
	if(coord.x < 0 || coord.y<0 || coord.x >= GridSize.x || coord.y>=GridSize.y)
		return 0;
	return int(coord.y * GridSize.x + coord.x);
}

vec4 ColorGradient(float f) {
	return texture(colorgradient, vec2(min(0.9999, max(0, f)), .5));
}

void main()
{
	vec2 worldPos = WorldViewMin + uv * (WorldViewMax - WorldViewMin);
	vec2 coord = uv * GridSize;
	vec2 ltCoord = floor(coord);
	Sample lt = samples.Entries[toIndex(ltCoord)];
	Sample rt = samples.Entries[toIndex(ltCoord+vec2(1,0))];
	Sample lb = samples.Entries[toIndex(ltCoord+vec2(0,1))];
	Sample rb = samples.Entries[toIndex(ltCoord+vec2(1,1))];
	
	vec2 c = coord - ltCoord;
	float accum = mix(mix(lt.Accum, rt.Accum, c.x),  mix(lb.Accum, rb.Accum, c.x), c.y);
	float mindis = mix(mix(lt.MinDis, rt.MinDis, c.x),  mix(lb.MinDis, rb.MinDis, c.x), c.y);
	//accum=lt.Accum;
	color =  vec4(0,0,0,1);
	color.r = accum/10;
	//if(accum>.1)
	//	color.g = 1;
	if(mindis < .005)
		color.g = 1;

	color = ColorGradient(sqrt(accum)/4);
	//color = vec4(0,0,0,1);
	
	
	
	//color.g = accum*1000;
/*	if(accum <= 0)
		color.r = 1;*/
	/*if(accum >4000)
	color =vec4(1,1,1,1);*/
}