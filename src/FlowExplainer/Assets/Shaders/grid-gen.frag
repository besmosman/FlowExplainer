#version 460
in vec2 uv;
in vec3 normal;
in vec4 vertexColor;
uniform vec4 tint = vec4(1, 1, 1, 1);

uniform vec2 gridSize;
out vec4 color;

struct Data {
    #pragma data
};

layout (std430, binding = 2)
buffer InstanceBuffer {
    Data data[];
};

#pragma interpolation

int GetIndex(vec2 coords){
    return int(coords.y * gridSize.x + coords.x);
}

int GetCellAt(vec2 p){
    return GetIndex(floor(p * gridSize));
}

vec4 toColor(Data dat) 
{
    #pragma toColor
}


void main()
{
    vec2 coords = floor(uv * gridSize);
    Data lt = data[GetIndex(coords)];
    Data rt = data[GetIndex(coords+ vec2(1,0))];
    Data rb = data[GetIndex(coords+ vec2(1,1))];
    Data lb = data[GetIndex(coords+ vec2(0,1))];
    vec2 m = (uv * gridSize - coords);
    Data bilinear = linear(linear(lt, rt, m.x), linear(lb, rb, m.x), m.y);
    color = toColor(bilinear);
}