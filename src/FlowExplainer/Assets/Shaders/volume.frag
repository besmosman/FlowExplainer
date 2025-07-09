#version 460
in vec2 uv;
in vec3 normal;
in vec4 vertexColor;
uniform vec4 tint = vec4(1, 1, 1, 1);

uniform vec3 gridSize;
out vec4 color;
uniform sampler2D colorgradient;

in vec3 worldPos;
in vec3 cameraPos;
uniform vec3 volumeMin;
uniform vec3 volumeMax;


struct Data {
    float Heat;
};

layout (std430, binding = 2)
buffer InstanceBuffer {
    Data data[];
};
vec4 ColorGradient(float f) {
    return texture(colorgradient, vec2(min(0.9999, max(0, f)), .5));
}
int GetIndex(vec3 coords) {
    return int(coords.z * gridSize.y * gridSize.x + coords.y * gridSize.x + coords.x);
}

vec4 transferFunction(float density) {
    float s =  max(0,(density-.5)) / 1.5;
    vec4 c = ColorGradient(s);
    c.a =.0001;
    //if(density > 1)
    //c.a = 1;
    return c;
}

vec2 rayBoxIntersection(vec3 rayOrigin, vec3 rayDir, vec3 boxMin, vec3 boxMax) {
    vec3 invDir = 1.0 / rayDir;
    vec3 t1 = (boxMin - rayOrigin) * invDir;
    vec3 t2 = (boxMax - rayOrigin) * invDir;

    vec3 tmin = min(t1, t2);
    vec3 tmax = max(t1, t2);

    float enter = max(max(tmin.x, tmin.y), tmin.z);
    float exit = min(min(tmax.x, tmax.y), tmax.z);

    return vec2(max(enter, 0.0), exit);
}


float sampleVolume(vec3 pos) {
    vec3 gridPos = (pos - volumeMin) / (volumeMax - volumeMin) * (gridSize - 1.0);
    //gridPos.z =gridSize.z-2;
    // Clamp to valid sampling range
    gridPos = clamp(gridPos, vec3(0.0), gridSize - 1.0);

    ivec3 baseCoord = ivec3(floor(gridPos));
    vec3 t = gridPos - vec3(baseCoord);

    // Ensure we don't go out of bounds - clamp coordinates to valid range
    ivec3 coord1 = min(baseCoord + ivec3(1), ivec3(gridSize) - 2);

    // Adjust interpolation weights when we hit the boundary
    // If coord1 == baseCoord (meaning we're at the boundary), set t to 0
    vec3 adjustedT = vec3(
    (coord1.x == baseCoord.x) ? 0.0 : t.x,
    (coord1.y == baseCoord.y) ? 0.0 : t.y,
    (coord1.z == baseCoord.z) ? 0.0 : t.z
    );

    // Sample using safe coordinates
    float c000 = data[GetIndex(baseCoord)].Heat;
    float c001 = data[GetIndex(ivec3(baseCoord.x, baseCoord.y, coord1.z))].Heat;
    float c010 = data[GetIndex(ivec3(baseCoord.x, coord1.y, baseCoord.z))].Heat;
    float c011 = data[GetIndex(ivec3(baseCoord.x, coord1.y, coord1.z))].Heat;
    float c100 = data[GetIndex(ivec3(coord1.x, baseCoord.y, baseCoord.z))].Heat;
    float c101 = data[GetIndex(ivec3(coord1.x, baseCoord.y, coord1.z))].Heat;
    float c110 = data[GetIndex(ivec3(coord1.x, coord1.y, baseCoord.z))].Heat;
    float c111 = data[GetIndex(coord1)].Heat;

    // Trilinear interpolation with adjusted weights
    float c00 = mix(c000, c100, adjustedT.x);
    float c01 = mix(c001, c101, adjustedT.x);
    float c10 = mix(c010, c110, adjustedT.x);
    float c11 = mix(c011, c111, adjustedT.x);

    float c0 = mix(c00, c10, adjustedT.y);
    float c1 = mix(c01, c11, adjustedT.y);

    return mix(c0, c1, adjustedT.z);
}
void main()
{
    vec3 rayDir = normalize(worldPos - cameraPos);
    vec2 intersection = rayBoxIntersection(cameraPos, rayDir, volumeMin, volumeMax);
    float enter = intersection.x;
    float exit = intersection.y;

    if (enter >= exit) {
        discard;
    }

    vec3 rayPos = cameraPos + rayDir * enter;
    vec4 accumulatedColor = vec4(0);
    float totalDistance = exit - enter;
    float stepSize = .001;
    int numSteps = int(totalDistance / stepSize);

    for (int i = 0; i < numSteps && accumulatedColor.a < .9999; i++) {
        float density =sampleVolume(rayPos);
        
        if(density > 1.50){
            vec4 sampleColor = transferFunction(density);

            sampleColor.a *= stepSize * 1000000.0;
            accumulatedColor.rgb += sampleColor.rgb * sampleColor.a * (1.0 - accumulatedColor.a);
            accumulatedColor.a += sampleColor.a * (1.0 - accumulatedColor.a);
        }
        rayPos += rayDir * stepSize;
    }
    color = accumulatedColor;
    //color = vec4(worldPos, 1.0);
}