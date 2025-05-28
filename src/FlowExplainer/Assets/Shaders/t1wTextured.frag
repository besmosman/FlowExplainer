#version 460
//! vec4 getColorMapColor(uint id, vec3 normal, uint flags, vec4 defaultColor);
//@colorMapping.glsl
//@bounds.glsl

in vec2 uv;
in vec3 normal;
in vec4 vertexColor;
in vec3 cameraPos;
in vec3 worldPos;
in flat uint id;
in flat uint flags;
in vec3 lineTangent;
out vec4 color;
uniform vec3 gridSize;

uniform sampler3D t1wTexture;

vec3 lambert(vec3 col, vec3 sunDir, vec3 sunCol)
{
    float d = max(dot(normal, sunDir), .0);
    return col * (sunCol * d);
}

vec3 phongSpecular(vec3 lightDir, vec3 viewDir, vec3 lightColor, float shininess)
{
    vec3 halfwayDir = normalize(lightDir + viewDir); // Use Blinn-Phong halfway vector
    float spec = pow(max(dot(normal, halfwayDir), 0.0), shininess);
    return lightColor * spec;
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
    const vec3 ambient = vec3(33, 28, 46) / 150;

    float u =vertexColor.x;
    float v =vertexColor.y;
    float w =vertexColor.z;
    float a = texture(t1wTexture, vec3(vertexColor.xyz)).r;
    a = pow(a,2)*.000001;
    a = max(min(a,1),0.0);
    if(vertexColor.w < 0)
            a = 0;
    vec4 col = vec4(a,a,a, vertexColor.w);

    //a = pow(a,4)*.000000000001;
    //col.rgb = mix(vec3(249,250,240),vec3(180,113,116), 1 - a)/255.;
    //col.a = 1;
    if(false) 
    {
        vec3 lightCol = vec3(0, 0, 0);
        vec3 lightDir = normalize(vec3(-.9, -.9, 1.1));
        vec3 sunCol = vec3(1, 1, 1) * 3;
        lightCol.rgb = lambert(col.rgb, lightDir, sunCol);
        lightCol.rgb += phongSpecular(lightDir, normalize(cameraPos - worldPos), sunCol, 19.0) * .1;
        lightCol.rgb += ambient;
        col.rgb *= lightCol;
        col.rgb = filmic(col.rgb);
    }
    color = col;


    if(!withinBounds(worldPos))
        discard;

    if(color.a < .01)
       discard;

}