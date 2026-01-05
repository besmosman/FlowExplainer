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

    int ParticleId;
    float StartAliveFactor;
    float EndAliveFactor;
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

vec2 getDistSqAndT(vec2 P, vec2 A, vec2 B)
{
    vec2 AB = B - A;
    vec2 AP = P - A;
    float lenSq = dot(AB, AB);

    float t = 0.0;
    if (lenSq > 1e-8) {
        t = clamp(dot(AP, AB) / lenSq, 0.0, 1.0);
    }

    vec2 closest = A + t * AB;
    vec2 d = P - closest;
    return vec2(dot(d, d), t);
}

const int MAX_SEEN = 64;

float SmoothingKernel(float h, float r)
{
    float q = r / h;
    if (q >= 1.0f) return 0.0f;

    float a = 1.0f - q;
    float a2 = a * a;
    float a4 = a2 * a2;

    return a4 * (1.0f + 4.0f * q);
}


void main()
{
    color = vertexColor * tint;
    vec2 worldPos = WorldViewMin + uv * (WorldViewMax - WorldViewMin);
    vec2 centerCell = floor(uv * GridSize);
    
    int seenLines[MAX_SEEN];
    float seenLinesMinDis[MAX_SEEN];
    int seenLinesMinLineId[MAX_SEEN];
    int seenCount = 0;


    float accum = 0.0;

    float kernelRadius = .00009f;
    float k2 = kernelRadius*kernelRadius;
    int d = 1;
    bool stop = false;
    for (int offsetX = -d;offsetX <=d; offsetX++)
    for (int offsetY = -d;offsetY <=d; offsetY++)
    {
        if(stop)
            break;
        vec2 cellCoords = centerCell + vec2(offsetX, offsetY);
        int index = int(cellCoords.y) * int(GridSize.x) + int(cellCoords.x);
        
        index = max(0, index);
        index = min(index, int(round(GridSize.x*GridSize.y))-1);
        Cell cell = cells.Entries[index];

        for (int i = cell.LinesStartIndex; i < cell.LinesStartIndex + cell.LineCount; i++)
        {
            Line l = lines.Entries[i];
            vec2 inter =  getDistSqAndT(worldPos, l.Start, l.End);
            float disSqrt =inter.x;
            float t = inter.y;
            float aliveFactor = mix(l.StartAliveFactor, l.EndAliveFactor,t);
            if (disSqrt < k2)
            {
                int particleId = l.ParticleId;
                int seenIndex = -1;

                for (int j = 0; j < seenCount; j++)
                {
                    if (seenLines[j] == particleId){
                        seenIndex = j;
                        break;
                    }
                }

                if (seenIndex != -1)
                {
                    if (disSqrt < seenLinesMinDis[seenIndex])
                    {
                        seenLinesMinDis[seenIndex] = disSqrt;
                        seenLinesMinLineId[seenIndex] = i;
                    }
                }
                else
                {
                    if (seenCount < MAX_SEEN){
                        int n = seenCount;
                        seenLinesMinDis[n] = disSqrt;
                        seenLinesMinLineId[n] = i;
                        seenLines[n] = particleId;
                        seenCount++;
                    }
                    else
                    {
                        float maxDis = -10.0;
                        int maxDisIndexJ = -1;

                        for (int j = 0; j < seenCount; j++)
                        {
                            if (seenLinesMinDis[j] > maxDis){
                                maxDis = seenLinesMinDis[j];
                                maxDisIndexJ = j;
                            }
                        }
                        if(maxDis < k2/4){
                            stop = true;
                        }
                        if (maxDisIndexJ != -1 && disSqrt < maxDis)
                        {
                            seenLines[maxDisIndexJ] = particleId;
                            seenLinesMinDis[maxDisIndexJ] = disSqrt;
                            seenLinesMinLineId[maxDisIndexJ] = i;
                        }
                    }
                }
            }
        }
    }
    for (int i=0; i<seenCount;i++)
    {
        Line l = lines.Entries[seenLinesMinLineId[i]];
        
        vec2 B = l.End;
        vec2 A = l.Start;
        vec2 P = worldPos;
        vec2 AB = B - A;
        vec2 AP = P - A;

        float t = dot(AP, AB) / dot(AB, AB);
        t = clamp(t, 0.0, 1.0);
        vec2 closest = A + t * AB;
        float dis = length(P - closest);
        float aliveFactor = mix(l.StartAliveFactor, l.EndAliveFactor,t);
        
        float w = SmoothingKernel(kernelRadius, dis);
        accum += w*(aliveFactor);
    }

    /*    if(accum > 100000)
        color = vec4(accum, 0, 0, 1);
        else*/

    color = vec4( (accum)/(MAX_SEEN*.1), (accum*accum)/(MAX_SEEN*3), 0, 1);
/**    if(accum > 7)
            color = vec4(1,1,1,1);
    else color = vec4(0,0,0,0);*/
    //color = vec4(lineCount/1000.0, 0, 0, 1);
    //color = vec4(cells.length()/10.0, 0, 0, 1);
    /*if(color.a == 0)
        discard;*/
}