vec3 getIncoming(mat4x4 viewMat)
{
	mat4x4 worldToView = inverse(viewMat);
	vec3 v = normalize(worldToView[2].xyz);
    return v;
}

mat2x3 getPos(vec4 clip, mat4 projMat, mat4 viewMat, mat4 modelMat)
{
    vec4 viewPosition = inverse(projMat) * clip;
    viewPosition /= viewPosition.w;

    vec4 worldPosition = inverse(viewMat) * viewPosition;
    vec4 objPosition = inverse(modelMat) * worldPosition;

    mat2x3 result;

    result[0] = worldPosition.xyz;
    result[1] = objPosition.xyz;

    return result;
}
