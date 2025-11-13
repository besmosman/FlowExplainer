#version 460
in vec2 uv;
in vec3 normal;
in vec4 vertexColor;
uniform vec4 tint = vec4(1, 1, 1, 1);

uniform vec2 gridSize;
out vec4 color;

struct Data {
    vec4 col;
};

layout (std430, binding = 2)
buffer InstanceBuffer {
    Data data[];
};

int GetIndex(vec2 coords){
    return int(coords.y * gridSize.x + coords.x);
}

void main()
{
    vec2 coords = floor(uv * gridSize);
    coords = clamp(coords, vec2(0), gridSize - vec2(2));
    vec4 lt = data[GetIndex(coords)].col;
    vec4 rt = data[GetIndex(coords+ vec2(1,0))].col;
    vec4 rb = data[GetIndex(coords+ vec2(1,1))].col;
    vec4 lb = data[GetIndex(coords+ vec2(0,1))].col;
    vec2 m = (uv * gridSize - coords);
    color = mix(mix(lt, rt, m.x), mix(lb, rb, m.x), m.y);
}