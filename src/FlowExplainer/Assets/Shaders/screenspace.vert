#version 460

layout(location = 0) in vec3 in_position;
layout(location = 1) in vec2 in_texcoords;
layout(location = 2) in vec3 in_normal;
layout(location = 3) in vec4 in_color;

out vec2 uv;
out vec3 normal;
out vec4 vertexColor;

uniform sampler2D mainTex;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
   uv = in_texcoords;
   vertexColor = in_color;
   normal = in_normal;
   gl_Position = projection * view * model * vec4(in_position, 1.0);
}