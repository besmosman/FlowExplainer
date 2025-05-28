//! #version 460
uniform int colorMappingType;
uniform float[16] colorMappingSettings;
uniform sampler2D matcap;

const int ColorMappingType_AbsoluteDirection = 1;
const int ColorMappingType_ViewDirection = 2;
const int ColorMappingType_RandomGradient = 3;
const int ColorMappingType_Constant = 4;
const int ColorMappingType_MatCap = 5;
const int ColorMappingSettings_AlphaMultiplier = 15;
const int ColorMappingSettings_DisabledAlphaMultiplier = 14;

in vec3 incoming;

float getSettingFloat(int i)
{
	return colorMappingSettings[i]; 
}

vec3 getSettingsVec3(int i)
{
	return vec3(colorMappingSettings[i], colorMappingSettings[i+1], colorMappingSettings[i+2]);
}

float rand(float n){return fract(sin(n) * 4371.5453123);}

float noise(float p){
	float fl = floor(p);
  float fc = fract(p);
	return mix(rand(fl), rand(fl + 1.0), fc);
}


// from https://github.com/hughsk/matcap/blob/master/matcap.glsl
vec2 matcapCoords(vec3 eye, vec3 normal) 
{
    vec3 reflected = reflect(eye, normal);
    float m = 2.8284271247461903 * sqrt(reflected.z+1.0);
    return reflected.xy / m + 0.5;
}

vec4 getColorMapColor(uint id, vec3 lineTangent, uint flags, vec4 defaultColor){
	
	vec4 mapColor = vec4(1,0,1,1);
	if(colorMappingType != 0)
	{
		switch (colorMappingType)
		{
			case ColorMappingType_AbsoluteDirection:
			    vec3 xCol = getSettingsVec3(0);
			    vec3 yCol = getSettingsVec3(3);
			    vec3 zCol = getSettingsVec3(6);
				mapColor = vec4(xCol*abs(lineTangent.y)+yCol*abs(lineTangent.x)+zCol*abs(lineTangent.z),1);
				break;
			case ColorMappingType_RandomGradient:
				vec3 c1 = getSettingsVec3(0);
				vec3 c2 = getSettingsVec3(3);
				mapColor = vec4(mix(c1,c2, noise(float(id)/100.)),1);
				break;
			case ColorMappingType_Constant:
				mapColor = vec4(getSettingsVec3(0),1);
				break;
			case ColorMappingType_ViewDirection:
				mapColor =  vec4(abs(lineTangent.x),abs(lineTangent.y),abs(lineTangent.z),1);;
				vec3 dir = getSettingsVec3(0);
				vec3 rdir = normalize(lineTangent.rgb);
				float x = abs(rdir.x*rdir.x) * dir.x;
				float y = abs(rdir.y*rdir.y) * dir.y;
				float z = abs(rdir.z*rdir.z) * dir.z;
				float f = (x+y+z)/length(dir);
				mapColor.a = f*f*f;
				break;
			case ColorMappingType_MatCap:
				mapColor = texture(matcap, matcapCoords(incoming, lineTangent));
				break;
		}
		if (flags == 2) //TODO bitwise gedoe maar success daarmee in glsl
			mapColor.a *= getSettingFloat(ColorMappingSettings_DisabledAlphaMultiplier);
		else 
			mapColor.a *= getSettingFloat(ColorMappingSettings_AlphaMultiplier);
	}
	else 
	{
		mapColor = defaultColor;
	}
	return mapColor;
}
