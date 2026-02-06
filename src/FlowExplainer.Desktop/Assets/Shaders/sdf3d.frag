#version 460
in vec2 uv;
in vec3 normal;
in vec4 vertexColor;
uniform vec4 tint = vec4(1,1,1,1);
out vec4 color;

in vec3 worldPos;
in vec3 cameraPos;
uniform vec3 volumeMin;
uniform vec3 volumeMax;
uniform vec3 gridSize;

uniform sampler2D colorgradient;
uniform sampler3D data;


// Lighting uniforms
uniform vec3 lightDirection = vec3(0.5, -0.9, -1.0); // Default light direction
uniform float ambientStrength = 0.5;
uniform float diffuseStrength = 0.7;
uniform float gradientThreshold = 0.01; // Minimum gradient magnitude for shading

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


vec4 ColorGradient(float f) {
	return texture(colorgradient, vec2(min(0.9999, max(0, f)), .5));
}

vec4 transferFunction(float density) {
	vec4 c = ColorGradient(density);
	return c;
}

vec3 calculateGradient(vec3 p) {
	vec3 voxelSize = (volumeMax - volumeMin) / gridSize;
	float delta = 0.001; // Small offset for gradient calculation

	vec3 gradient;
	vec3 pos = (p - volumeMin) / (volumeMax - volumeMin);
	gradient.x = texture(data, pos + vec3(delta, 0, 0)).r - texture(data, pos - vec3(delta, 0, 0)).r;
	gradient.y = texture(data, pos + vec3(0, delta, 0)).r - texture(data, pos - vec3(0, delta, 0)).r;
	gradient.z = texture(data, pos + vec3(0, 0, delta)).r - texture(data, pos - vec3(0, 0, delta)).r;

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

	
	color = vertexColor * tint;

	vec3 rayPos = cameraPos + rayDir * enter;
	vec4 accumulatedColor = vec4(0);
	float totalDistance = exit - enter;
	float stepSize = .002;
	float depthScaling = 1;
	int numSteps = int(totalDistance / stepSize);

	for (int i = 0; i < numSteps && accumulatedColor.a < 1; i++) {
		float density = texture(data,(rayPos - volumeMin) / (volumeMax - volumeMin)).r*1;
		if(rayPos.z > volumeMin.z+0.05){
			vec4 sampleColor = ColorGradient(.5);
			// Calculate gradient for lighting
			vec3 gradient = calculateGradient(rayPos);
			vec3 viewDir = normalize(cameraPos - rayPos);
			sampleColor.rgb = applyLighting(sampleColor.rgb, gradient, viewDir);
			sampleColor.a = density;
			//sampleColor.a *= stepSize * depthScaling;
			accumulatedColor.rgb += sampleColor.rgb * sampleColor.a * (1.0 - accumulatedColor.a);
			accumulatedColor.a += sampleColor.a * (1.0 - accumulatedColor.a);
		}
		rayPos += rayDir * stepSize;
	}
	color = accumulatedColor;
}