uniform int boundType;
uniform vec3 boundsCenter;
uniform vec3 boundsSize;


bool withinBounds(vec3 worldPos){
	vec3 bmin = boundsCenter - boundsSize/2.;
	vec3 bmax = boundsCenter + boundsSize/2.;
	bool inBound = bool(worldPos.x > bmin.x && worldPos.x < bmax.x &&
	worldPos.y > bmin.y && worldPos.y < bmax.y &&
	worldPos.z > bmin.z && worldPos.z < bmax.z);

	if(boundType == 1 && !inBound)
	return false;

	if(boundType == 2 && inBound)
	return false;

	return true;
}


uint setBoundFlags(vec3 in_position, uint flags)
{
   if(boundType != 0)
   {
		vec3 bmin = boundsCenter - boundsSize/2.;
		vec3 bmax = boundsCenter + boundsSize/2.;
		bool inBound = bool(in_position.x > bmin.x && in_position.x < bmax.x &&
						  in_position.y > bmin.y && in_position.y < bmax.y &&
						  in_position.z > bmin.z && in_position.z < bmax.z);

		if(boundType == 1 && !inBound)
			 flags = 2;

		if(boundType == 2 && inBound)
			 flags = 2;
   }
   return flags;
}