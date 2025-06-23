#version 330 core
uniform sampler2D mainTex;
in vec2 uv;
uniform float screenPxRange;
in vec2 screenPos;
uniform vec4 tint;
uniform float t;
uniform float line;
uniform float lines;
in vec4 vertexColor;
out vec4 color;
in vec3 normal;

float median(float r, float g, float b)
{
    return max(min(r, g), min(max(r, g), b));
}


void main()
{
    vec3 msd = texture(mainTex, uv).rgb;
    float sd = median(msd.r, msd.g, msd.b);
    float screenPxDistance = screenPxRange * (sd - 0.5);
    float opacity = clamp(screenPxDistance + 0.5, 0.0, 1.0);

    color = mix(vec4(vertexColor.rgb * tint.rgb, 0), vertexColor * tint, opacity);

    float norm = normal.x;
    
    
    float t2 = max(0,min(1,t));
    
    if (norm > t2)
    {
        color = mix(vec4(color.rgb, color.a * max(0,min(1,t*1))), vec4(color.rgb, 0), max(0, (norm - t2) * 5));
    }

    if (color.a < .01)
    discard;
}