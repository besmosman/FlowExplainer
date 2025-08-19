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

uniform float heatFilterMin;
uniform float heatFilterMax;
uniform float depthScaling;

// Lighting uniforms
uniform vec3 lightDirection = vec3(0.5, -0.5, 1.0); // Default light direction
uniform float ambientStrength = 0.3;
uniform float diffuseStrength = 0.7;
uniform float gradientThreshold = 0.01; // Minimum gradient magnitude for shading

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
    return int(round(coords.z * gridSize.y * gridSize.x + coords.y * gridSize.x + coords.x));
}

vec4 transferFunction(float density) {
    vec4 c = ColorGradient(density);
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

float sampleVolumeRaw(vec3 pos) {
    vec3 gridPos = (pos - volumeMin) / (volumeMax - volumeMin) * (gridSize - 1.0);
    gridPos.z = gridSize.z - gridPos.z;

    // Boundary check
    if(pos.x < volumeMin.x || pos.y < volumeMin.y || pos.z < volumeMin.z ||
    pos.x > volumeMax.x || pos.y > volumeMax.y || pos.z > volumeMax.z)
    return 0.0;
    
    ivec3 baseCoord = ivec3(floor(gridPos));
    vec3 t = gridPos - vec3(baseCoord);

    ivec3 coord1 = baseCoord + ivec3(1);

    if(any(greaterThanEqual(coord1, ivec3(gridSize))) ||
    any(lessThan(baseCoord, ivec3(0)))) {
        return 0.0;
    }

    // Sample the 8 corners for trilinear interpolation
    float c000 = data[GetIndex(baseCoord)].Heat;
    float c001 = data[GetIndex(ivec3(baseCoord.x, baseCoord.y, coord1.z))].Heat;
    float c010 = data[GetIndex(ivec3(baseCoord.x, coord1.y, baseCoord.z))].Heat;
    float c011 = data[GetIndex(ivec3(baseCoord.x, coord1.y, coord1.z))].Heat;
    float c100 = data[GetIndex(ivec3(coord1.x, baseCoord.y, baseCoord.z))].Heat;
    float c101 = data[GetIndex(ivec3(coord1.x, baseCoord.y, coord1.z))].Heat;
    float c110 = data[GetIndex(ivec3(coord1.x, coord1.y, baseCoord.z))].Heat;
    float c111 = data[GetIndex(coord1)].Heat;

    // Standard trilinear interpolation
    float c00 = mix(c000, c100, t.x);
    float c01 = mix(c001, c101, t.x);
    float c10 = mix(c010, c110, t.x);
    float c11 = mix(c011, c111, t.x);

    float c0 = mix(c00, c10, t.y);
    float c1 = mix(c01, c11, t.y);

    return mix(c0, c1, t.z);
}

float sampleVolume(vec3 pos) {
    return sampleVolumeRaw(pos);
}

// Calculate gradient using central differences
vec3 calculateGradient(vec3 pos) {
    vec3 voxelSize = (volumeMax - volumeMin) / gridSize;
    float delta = 0.001; // Small offset for gradient calculation

    vec3 gradient;
    gradient.x = sampleVolumeRaw(pos + vec3(delta, 0, 0)) - sampleVolumeRaw(pos - vec3(delta, 0, 0));
    gradient.y = sampleVolumeRaw(pos + vec3(0, delta, 0)) - sampleVolumeRaw(pos - vec3(0, delta, 0));
    gradient.z = sampleVolumeRaw(pos + vec3(0, 0, delta)) - sampleVolumeRaw(pos - vec3(0, 0, delta));

    gradient /= (2.0 * delta);
    return gradient;
}

//Example lighting found somewhere
//Apply simple Phong-style lighting
vec3 applyLighting(vec3 baseColor, vec3 gradient, vec3 viewDir) {
    float gradientMagnitude = length(gradient);

    // If gradient is too small, just return ambient lighting
    if (gradientMagnitude < gradientThreshold) {
        return baseColor * ambientStrength;
    }

    // Normalize gradient to get surface normal
    vec3 normal = normalize(gradient);
    vec3 lightDir = normalize(lightDirection);

    // Ambient component
    vec3 ambient = baseColor * ambientStrength;

    // Diffuse component (Lambert)
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = baseColor * diffuseStrength * diff;

    // Optional: Simple specular component
    // vec3 reflectDir = reflect(-lightDir, normal);
    // float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.0);
    // vec3 specular = vec3(0.2) * spec;

    return ambient + diffuse; // + specular if you want specular
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

    for (int i = 0; i < numSteps && accumulatedColor.a < 1; i++) {
        float density = sampleVolume(rayPos);

        if(density > heatFilterMin && density < heatFilterMax && rayPos.z > volumeMin.z+0.05){
            vec4 sampleColor = transferFunction(density);
            // Calculate gradient for lighting
            //vec3 gradient = calculateGradient(rayPos);
            //vec3 viewDir = normalize(cameraPos - rayPos);
            //sampleColor.rgb = applyLighting(sampleColor.rgb, gradient, viewDir);
            sampleColor.a = 1;
            sampleColor.a *= stepSize * depthScaling;
            accumulatedColor.rgb += sampleColor.rgb * sampleColor.a * (1.0 - accumulatedColor.a);
            accumulatedColor.a += sampleColor.a * (1.0 - accumulatedColor.a);
        }
        rayPos += rayDir * stepSize;
    }
    color = accumulatedColor;
}