#version 460
in vec2 uv;
in vec3 normal;
in vec4 vertexColor;
uniform vec4 tint = vec4(1, 1, 1, 1);
out vec4 color;

uniform vec2 WorldViewMin;
uniform vec2 WorldViewMax;
uniform vec2 GridSize;
uniform sampler2D colorgradient;

uniform sampler2D gradFluid;
uniform sampler2D gradSteel;
uniform sampler2D gradInsulation;
uniform sampler2D gradCasing;
uniform sampler2D gradGround;


uniform float Power;
uniform bool colorByMaterial = true;
/*uniform float max;
uniform float min;*/

struct Sample {
    float Accum;
    float HeatingCooling;
    float Count;
    float Padding;
};

layout (std430, binding = 2) buffer cellBuffer {
    Sample Entries[];
} samples;


int toIndex(vec2 coord) {
    if (coord.x < 0 || coord.y < 0 || coord.x >= GridSize.x || coord.y >= GridSize.y)
    return 0;
    return int(coord.y * GridSize.x + coord.x);
}

vec4 ColorGradient(float f) {
    return texture(colorgradient, vec2(min(0.9999, max(0, f)), .5));
}

vec4 ColorGradient(sampler2D s, float f) {
    return texture(s, vec2(min(0.9999, max(0, f)), .5));
}

void main()
{
    vec2 worldPos = WorldViewMin + uv * (WorldViewMax - WorldViewMin);
    vec2 coord = uv * GridSize;
    vec2 ltCoord = floor(coord);
    Sample lt = samples.Entries[toIndex(ltCoord)];
    Sample rt = samples.Entries[toIndex(ltCoord + vec2(1, 0))];
    Sample lb = samples.Entries[toIndex(ltCoord + vec2(0, 1))];
    Sample rb = samples.Entries[toIndex(ltCoord + vec2(1, 1))];

    vec2 c = coord - ltCoord;
    float accum = mix(mix(lt.Accum, rt.Accum, c.x), mix(lb.Accum, rb.Accum, c.x), c.y);
    float heatingCoolingFactor = mix(mix(lt.HeatingCooling, rt.HeatingCooling, c.x), mix(lb.HeatingCooling, rb.HeatingCooling, c.x), c.y);
    float count = mix(mix(lt.Count, rt.Count, c.x), mix(lb.Count, rb.Count, c.x), c.y);
    //float lic = mix(mix(lt.LIC/lt.padding.x, rt.LIC/rt.padding.x, c.x),  mix(lb.LIC/lb.padding.x, rb.LIC/rb.padding.x, c.x), c.y);
    //accum=lt.Accum;
    //color =  vec4(0,0,0,1);
    //color.r = accum/10;
    //if(accum>.1)
    //	color.g = 1;

    color = ColorGradient(pow(accum, Power));

    if (colorByMaterial) {
        float rw = 27e-3;
        float rs = 30.15e-3;
        float ri = 59.5e-3;
        float rc = 62.5e-3;
        float rg = 1.5 * rc;
        float L = 20 * rw;
        float U_mean = 1e-2;

        float r = worldPos.y;
        float f =  pow(accum, Power);
        if (r < rw) {
            color = ColorGradient(gradFluid,f);
        } else if (r < rs)
        {
            color = ColorGradient(gradSteel,f);
        } else if(r < ri){
            color = ColorGradient(gradInsulation, f);
        } else if(r < rc){
            color = ColorGradient(gradCasing, f);
        } else {
            color = ColorGradient(gradGround, f);
        }

    }

    //color = ColorGradient(heatingCoolingFactor/10);
    //color = ColorGradient((signedDistanceZ+ 1) /2);

    //color = ColorGradient(sqrt(lic)/4);
    //color = vec4(0,0,0,1);



    //color.g = accum*1000;
/*	if(accum <= 0)
		color.r = 1;*/
/*if(accum >4000)
	color =vec4(1,1,1,1);*/
}