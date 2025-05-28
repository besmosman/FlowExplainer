#version 460
//! vec4 getColorMapColor(uint id, vec3 normal, uint flags, vec4 defaultColor);
//@colorMapping.glsl

in vec2 uv;
in vec3 normal;
in vec4 vertexColor;
in flat uint id;
in flat uint flags;
in vec3 lineTangent;
out vec4 color;

vec3 lambert(vec3 col, vec3 sunDir, vec3 sunCol)
{
    float d = max(dot(normal, sunDir), .0);
    return col * (sunCol * d);
}



// from https://github.com/dmnsgn/glsl-tone-map/blob/main/filmic.glsl
vec3 filmic(vec3 x) 
{
    vec3 X = max(vec3(0.0), x - 0.004);
    vec3 result = (X * (6.2 * X + 0.5)) / (X * (6.2 * X + 1.7) + 0.06);
    return pow(result, vec3(2.2));
}

void main()
{
	if(flags == 2)
		discard;

    const vec3 ambient = vec3(33, 28, 46) / 150;

    vec4 col = getColorMapColor(id, colorMappingType == ColorMappingType_MatCap ? normal : lineTangent, flags, vertexColor);
    vec3 lightCol = vec3(0,0,0);

    lightCol.rgb = lambert(col.rgb, normalize(vec3(-.9, -1, 1.1)), vec3(1,1,1)*3);
    lightCol.rgb += ambient;

    col.rgb *= lightCol;
    col.rgb = filmic(col.rgb);

    color = col;
}