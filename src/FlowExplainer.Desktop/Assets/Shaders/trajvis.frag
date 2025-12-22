#version 460
in vec2 uv;
in vec3 normal;
in vec4 vertexColor;
uniform vec4 tint = vec4(1, 1, 1, 1);
out vec4 color;

uniform vec2 WorldViewMin;
uniform vec2 WorldViewMax;
uniform vec2 GridSize;


struct Line {
    vec2 Start;
    vec2 End;
    
    float TimeAliveFactor;
    float padding0;
    float padding1;
    float padding2;
};


struct Cell {
    int LinesStartIndex;
    int LineCount;
    vec2 padding;
};

layout(std430, binding = 1) buffer cellBuffer {
    Cell Entries[];
} cells;



layout(std430, binding = 2) buffer lineBuffer {
    Line Entries[];
} lines;

float pointSegmentDistance(vec2 P, vec2 A, vec2 B)
{
    vec2 AB = B - A;
    vec2 AP = P - A;

    float t = dot(AP, AB) / dot(AB, AB);
    t = clamp(t, 0.0, 1.0);

    vec2 closest = A + t * AB;
    return length(P - closest);
}

void main()
{
    color = vertexColor * tint;
    vec2 worldPos = WorldViewMin + uv * (WorldViewMax - WorldViewMin);
    vec2 cellCoords = floor(uv * GridSize);
    int index = int(cellCoords.y) * int(GridSize.x) + int(cellCoords.x);
    index = max(0, index);
    index = min(index, int(round(GridSize.x*GridSize.y))-1);
    Cell cell = cells.Entries[index];
    float accum = 0.0;
    for (int i = cell.LinesStartIndex; i < cell.LinesStartIndex + cell.LineCount; i++)
    {
        Line l = lines.Entries[i];
        float dis = pointSegmentDistance(worldPos, l.Start, l.End);
        if (dis < .001)
            accum=max(accum, 1 - l.TimeAliveFactor);
    }
    color = vec4(accum, accum, accum, 1);
    //color = vec4(lineCount/1000.0, 0, 0, 1);
    //color = vec4(cells.length()/10.0, 0, 0, 1);
    /*if(color.a == 0)
        discard;*/
}