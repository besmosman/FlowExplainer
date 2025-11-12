#version 460
in vec2 uv;
in vec3 normal;
in vec4 vertexColor;
uniform vec4 tint = vec4(1, 1, 1, 1);

uniform vec2 gridSize;
out vec4 color;
uniform sampler2D colorgradient;
uniform bool interpolate;
uniform float minGrad;
uniform float maxGrad;
uniform bool useCustomColor;

struct Data {
    float Value;
    float Marker;
    vec2 padding;
    vec4 CustomColor;
};

layout (std430, binding = 2)
buffer InstanceBuffer {
    Data data[];
};

//#pragma interpolation

int GetIndex(vec2 coords) {
    return max(0, int(coords.y * gridSize.x + coords.x));
}

int GetCellAt(vec2 p) {
    return GetIndex(floor(p * gridSize));
}

vec4 ColorGradient(float f) {
    float m = (f - minGrad) / (maxGrad-minGrad);
    return texture(colorgradient, vec2(min(0.9999, max(0, m)), .5));
}

vec4 Bilinear(vec2 uvv) {
    vec2 p = uvv * (gridSize -  vec2(1));
    vec2 coords = floor(p);
    vec2 m = p - coords;
    coords = min(coords, gridSize - vec2(1));

    Data lt = data[GetIndex(coords)];
    Data rt = data[GetIndex(coords + vec2(1, 0))];
    Data lb = data[GetIndex(coords + vec2(0, 1))];
    Data rb = data[GetIndex(coords + vec2(1, 1))];

    float marked =(mix(mix(lt.Marker, rt.Marker, m.x), mix(lb.Marker, rb.Marker, m.x), m.y));
    //return linear(linear(lt, lb, m.y), linear(rt, rb, m.y), m.x);
    float val = mix(mix(lt.Value, rt.Value, m.x), mix(lb.Value, rb.Value, m.x), m.y);
    return mix( ColorGradient(val), vec4(1,1,1,1), marked);
}

vec4 BilinearCustomColor(vec2 uvv) {
    vec2 p = uvv * (gridSize -  vec2(1));
    vec2 coords = floor(p);
    vec2 m = p - coords;
    coords = min(coords, gridSize - vec2(1));

    Data lt = data[GetIndex(coords)];
    Data rt = data[GetIndex(coords + vec2(1, 0))];
    Data lb = data[GetIndex(coords + vec2(0, 1))];
    Data rb = data[GetIndex(coords + vec2(1, 1))];

    return (mix(mix(lt.CustomColor, rt.CustomColor, m.x), mix(lb.CustomColor, rb.CustomColor, m.x), m.y));
}


vec2 GetCoords(vec2 uvv){

    ivec2 coords = ivec2(clamp(floor(uv * (gridSize - 2)), vec2(0), gridSize - vec2(1)));
    return  ivec2(floor(uv * gridSize));
}

void main()
{
    
    if(useCustomColor)
    {
        if (interpolate) 
            color = BilinearCustomColor(uv);
        else
            color = data[GetIndex(GetCoords(uv))].CustomColor;
    }
    else
    {
        ivec2 coords = ivec2(clamp(floor(uv * (gridSize - 2)), vec2(0), gridSize - vec2(1)));
        coords =  ivec2(floor(uv * (gridSize - vec2(0))));
        Data Dat = data[GetIndex(coords)];
        color = ColorGradient(Dat.Value);


        if (interpolate) {
            color = Bilinear(uv);
        }

        if(Dat.Marker > .5f)
            color = vec4(1);
    }

}
