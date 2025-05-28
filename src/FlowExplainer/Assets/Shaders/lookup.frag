#version 460

#define MAX_ITERATIONS 10000


in vec2 uv;
in vec3 normal;
in vec3 lineTangent;
in vec4 vertexColor;
in flat uint id;
in flat uint flags;
in vec3 worldPos;
in vec3 cameraPos;

uniform vec3 caminverttranslation;
uniform vec4 viewport;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform vec3 boundsMin;

uniform vec3 gridSize;
uniform float voxelSize;
uniform vec4 tint = vec4(1,1,1,1);
out vec4 color;


layout (std430, binding=2) buffer voxelStartIndexlayout
{ 
   int f[];
} voxelStartIndex;




void main()
{
	
	
	//while(voxelCoordsIndex >= 0 && voxelCoordsIndex <  gridSize.x * gridSize.y * gridSize.z)


	vec3 rayOrigin = worldPos*1;
    vec3 rayDirection = normalize(caminverttranslation- worldPos);
	vec3 curPos = rayOrigin;
	
	vec3 coord = ceil((curPos - boundsMin)/voxelSize);
	int voxelCoordsIndex = int(coord.x + (coord.y * gridSize.x) + (coord.z * gridSize.x * gridSize.y));
	/*
	color = vec4(1,0,0,1);
	for (int i = 0; i < MAX_ITERATIONS; i++)
    {
		curPos += rayDirection*.1;
		if(distance(curPos, vec3(0,0,-30)) < 3)
		{
			color = vec4(1,0,1,1);
			return;
		}
	}
	discard;
	//color = vec4(abs(rayDirection.x),abs(rayDirection.y),0,1);
	*/

		color = vec4(0,0,0,1);
	bool hit = false;
	for (int i = 0; i < MAX_ITERATIONS; i++)
	{
		int id = 0;

		if(voxelCoordsIndex >= 0 && coord.x >  0 && coord.y > 0 && coord.z > 0 && coord.x < gridSize.x && coord.y < gridSize.y  && coord.z < gridSize.z )
		{
			id = voxelStartIndex.f[voxelCoordsIndex];
			if(id != 0)
			{
				color = vec4(1,1,1,1);
				hit =true;
			}
		} else
		{
		
		}
		curPos += rayDirection * voxelSize/10.;
		coord = ceil((curPos - boundsMin)/voxelSize);
		voxelCoordsIndex = int(coord.x + (coord.y * gridSize.x) + (coord.z * gridSize.x * gridSize.y));
	}
	if(!hit)
	discard;
//	color = vec4(uv.x, uv.y,1,1);
}

