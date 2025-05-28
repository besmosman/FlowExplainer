#version 460

#define MAX_ITERATIONS 64
#define FLOAT_EPSILON 0.0001
#define MAX_LINES_PER_VOXEL 8

//! float simplex3d(vec3 p);
//@simplex.glsl

// many SDF functions taken from https://iquilezles.org/articles/distfunctions/
// thank you, iq

in vec2 uv;
in vec3 normal;
in vec4 vertexColor;
in flat uint id;
in flat uint flags;
in vec3 mesh_center;
in vec3 incoming;
in vec3 worldPos;
in vec3 objPos;
in vec3 cameraPos;
in vec4 viewPos;
in vec4 projPos;
in vec2 screenPos;
in vec3 ndcPos;
in flat int voxelId;

out vec4 color;

uniform float time;
uniform float voxelSize;
uniform sampler2D lineData;

struct RayHit
{
    bool valid;
    vec3 normal;
    vec3 point;
    vec3 color;
};

float sphereSDF(vec3 p, vec3 center, float radius)
{
    return length(p - center) - radius;
}

float sdLineSegment(vec3 p, vec3 a, vec3 b, float r) 
{
    vec3 ab = b - a;
    vec3 ap = p - a;
    float t = clamp(dot(ap, ab) / dot(ab, ab), 0.0, 1.0);
    vec3 closestPoint = a + t * ab;
    return length(p - closestPoint) - r;
}

float queryScene(vec3 p)
{
    float d = 999;

    for (int i = 0; i < MAX_LINES_PER_VOXEL; i++)
    {
        vec3 pointA = mesh_center + voxelSize * 0.5 * (texelFetch(lineData, ivec2(voxelId, i * 2), 0).rgb * 2 - 1);
        vec3 pointB = mesh_center + voxelSize * 0.5 * (texelFetch(lineData, ivec2(voxelId, i * 2 + 1), 0).rgb * 2 - 1);
        d = min(d, sdLineSegment(p, pointA, pointB, 0.1));
        d = min(sphereSDF(p, pointA, 0.2), d);
        d = min(sphereSDF(p, pointB, 0.2), d);
    }

    return d;//sphereSDF(p, mesh_center, voxelSize / 4);
}

float sdCylinder( vec3 p, vec3 c )
{
  return length(p.xz-c.xy)-c.z;
}

vec3 getNormalAt(vec3 p)
{
    const float o = 0.01;
    
    return normalize(vec3(
        queryScene(p + vec3(o,0,0)) - queryScene(p - vec3(o,0,0)),
        queryScene(p + vec3(0,o,0)) - queryScene(p - vec3(0,o,0)),
        queryScene(p + vec3(0,0,o)) - queryScene(p - vec3(0,0,o))
    ));
}

RayHit raymarch(vec3 ro, vec3 rd, bool stayInVoxel)
{
    float t = 0.0;
    for (int i = 0; i < MAX_ITERATIONS; i++)
    {
        vec3 p = ro + rd * t;
        float d = queryScene(p);
        if (d <= FLOAT_EPSILON)
        {
            vec3 normal = getNormalAt(p);
            vec3 point = p;
            vec3 color = mod(abs(normal), 1);

            return RayHit(true, normal, point, color);
        }
        t += d;

        if (stayInVoxel && t > voxelSize * 2) 
            return RayHit(false, vec3(0), vec3(0), vec3(0));
    }
    return RayHit(false, vec3(0), vec3(0), vec3(0));
}

void main()
{
    const vec3 lightPos = vec3(0, 250, 150);

    vec3 rayOrigin = worldPos;
    vec3 rayDirection = normalize(worldPos - cameraPos);
    RayHit hit = raymarch(rayOrigin, rayDirection, true);

    if (!hit.valid)
        discard;

    vec3 lightDir = normalize(lightPos - hit.point);
    color = vec4(hit.color, 1.0);
    color.rgb *= max(0, dot(hit.normal, lightDir));
    // TODO: implement proper depth buffer writng
//    gl_FragDepth = hit.point.z;
}
