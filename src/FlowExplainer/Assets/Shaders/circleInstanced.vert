#version 460
//! vec3 getIncoming(mat4x4 viewMat);
//! mat2x3 getPos(vec4 clip, mat4x4 projMat, mat4x4 viewMat, mat4x4 modelMat);
//@transformVert.glsl


layout(location = 0) in vec3 in_position;
layout(location = 1) in vec2 in_texcoords;
layout(location = 2) in vec3 in_normal;
layout(location = 3) in vec4 in_color;

out vec2 uv;
out vec3 normal;
out vec4 vertexColor;

out float progress;
out vec3 incoming;
out vec3 worldPos;
out vec3 objPos;
out vec3 cameraPos;

uniform sampler2D mainTex;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

struct Particle {
   vec2 position;
   float radius;
   float padding;
   vec4 color;
};

layout(std430, binding = 2) buffer InstanceBuffer {
   Particle particles[];
};

void main()
{
   uv = in_texcoords;
   vertexColor = in_color;
   normal = in_normal;

   mat4 model = mat4(1.0);
   
   model[0][0] = particles[gl_InstanceID].radius;
   model[1][1] = particles[gl_InstanceID].radius;
   model[2][2] = 1;

   model[3][0] = particles[gl_InstanceID].position.x;
   model[3][1] = particles[gl_InstanceID].position.y;
   model[3][2] = 0;
   
   gl_Position = projection * view * model * vec4(in_position, 1.0);
   
   incoming = getIncoming(view * model);

   mat2x3 pp = getPos(gl_Position, projection, view, model);
   worldPos = pp[0];
   objPos = pp[1];
   vertexColor = particles[gl_InstanceID].color;
   cameraPos = (inverse(view) * model * vec4(0, 0, 0, 1)).xyz;
}