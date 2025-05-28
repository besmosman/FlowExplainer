#version 460

in vec2 uv;
in vec3 normal;
in vec4 vertexColor;

out vec4 color;
uniform vec3 gridSize;
uniform vec3 focusGrid;
uniform vec3 focusWorld;
uniform sampler2D mainTex;
uniform float t;
uniform int axis;
uniform int mode;
uniform float intensity;
uniform float threshold;

layout (std430, binding = 2) buffer niiDataLayout
{
    float f[];
} niiData;

void main()
{
    int u = int(uv.x * gridSize.x);
    int v = int(uv.y * gridSize.y);

    int i = 0;
    int j = 0;
    int k = 0;
    if (axis == 0)
    {
        i = u;
        j = v;
        k = int(focusGrid.z);
    }
    if (axis == 1)
    {
        i = u;
        j = int(focusGrid.y);
        k = v;
    }
    if (axis == 2)
    {
        i = int(focusGrid.x);
        j = v;
        k = u;
    }
    color = vec4(0,1,0,1);
    
    //k = 30;
    int index  =int(i + j * gridSize.x + k * gridSize.x * gridSize.y);
    float a = max(0,niiData.f[index]) * intensity*30;
    a = max(0, a);


    if(mode == 0)
    {
        color = vec4(a, a, a, 1);
    }
    
    if(mode == 1)
    {
        float b = a * a * 2.4;
        float c = sqrt(a) * .6;
        color = vec4(b, a, c, min(1, (b + a + c) * 1));
    }

    if (color.r+color.g+color.b < threshold/3. || color.a < threshold)
            discard;
    
    if(niiData.f[index] <= 0)
            discard;
   /** 
    if(a < .2 || a > .5)
        discard;*/
}