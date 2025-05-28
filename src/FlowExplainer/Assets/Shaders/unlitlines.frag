#version 460
//! vec4 getColorMapColor(uint id, vec3 lineTangent, uint flags, vec4 defaultColor);
//! uniform int colorMappingType;
//! const int ColorMappingType_MatCap = 5;
//@colorMapping.glsl

in vec2 uv;
in vec3 worldPos;
in vec3 normal;
in vec3 lineTangent;
in vec4 vertexColor;
in flat uint id;
in flat uint flags;
out vec4 color;

void main()
{
    vec4 mapColor = vec4(1,0,1,1);
	color = getColorMapColor(id, colorMappingType == ColorMappingType_MatCap ? normal : lineTangent, flags, vertexColor);
	//float a = max(0,min(1, (worldPos.y+80) / 80.));
	//color.a = a*a;
	if(color.a <= 0)
		discard;
}