#version 460

in vec2 uv;
in vec3 normal;
in vec4 vertexColor;

out vec4 color;

uniform vec4 tint;
uniform sampler2D mainTex;

void main()
{
    color = tint * vertexColor * texture(mainTex, uv);
   // color = vec4(1,1,1,1);
    if(uv.x <= .001 || uv.y<.001|| uv.x > .999 || uv.y > .999)
            discard;
}
