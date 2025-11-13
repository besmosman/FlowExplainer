#version 460
in vec2 uv;
in vec3 normal;
in vec4 vertexColor;
uniform vec4 tint = vec4(1, 1, 1, 1);

uniform vec2 gridSize;
out vec4 color;
uniform sampler2D colorgradient;
uniform bool interpolate;
struct Data {
    #pragma data
};

layout (std430, binding = 2)
buffer InstanceBuffer {
    Data data[];
};

#pragma interpolation

int GetIndex(vec2 coords) {
    return max(0, int(coords.y * gridSize.x + coords.x));
}

int GetCellAt(vec2 p) {
    return GetIndex(floor(p * gridSize));
}

Data Bilinear(vec2 uvv) {
    vec2 p = uvv * (gridSize -  vec2(1));
    vec2 coords = floor(p);
    vec2 m = p - coords;
    coords = min(coords, gridSize - vec2(2));

    Data lt = data[GetIndex(coords)];
    Data rt = data[GetIndex(coords + vec2(1, 0))];
    Data lb = data[GetIndex(coords + vec2(0, 1))];
    Data rb = data[GetIndex(coords + vec2(1, 1))];

    //return linear(linear(lt, lb, m.y), linear(rt, rb, m.y), m.x);
    return linear(linear(lt, rt, m.x), linear(lb, rb, m.x), m.y);
}

vec4 ColorGradient(float f) {
    return texture(colorgradient, vec2(min(0.9999, max(0, f)), .5));
}

vec4 toColor()
{
    Data Dat = data[GetIndex(round(uv * (gridSize-vec2(1))))];
    if (interpolate) {
        Dat = Bilinear(uv);
    }

    #pragma toColor
}


void main()
{
    color = vec4(1);
    color = toColor();
}