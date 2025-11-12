using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class Gizmos2DInstanced
{
    internal Gizmos2DInstanced()
    {
        
    }
    
    [StructLayout(LayoutKind.Sequential)]
    private struct CircleRenderInfo
    {
        public Vec2 Position;
        public float Radius;
        public float Padding;
        public Color Color;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RectCenteredRenderInfo
    {
        public Vec2 Center;
        public Vec2 Size;
        public Color Color;
        public Vec3 padding;
        public float Rotation;
    }


    private Mesh circleMesh => Gizmos2D.circleMesh;
    private Mesh rectCenteredMesh => Gizmos2D.quadMeshCentered;
    private AutoExpandStorageBuffer<CircleRenderInfo> circleStorage = new();
    private AutoExpandStorageBuffer<RectCenteredRenderInfo> rectCenteredStorage = new();
    private Material matCircleInstanced = new Material(new Shader("Assets/Shaders/instancedCircle.vert", ShaderType.VertexShader), Shader.DefaultUnlitFragment);
    private Material matRectInstanced = new Material(new Shader("Assets/Shaders/instancedRectCentered.vert", ShaderType.VertexShader), Shader.DefaultUnlitFragment);


    public void RegisterCircle(Vec2 center, float radius, Color color)
    {
        circleStorage.Register(new CircleRenderInfo()
        {
            Position = center,
            Radius = radius,
            Color = color
        });
    }

    public void RenderCircles(ICamera camera)
    {
        matCircleInstanced.Use();
        matCircleInstanced.SetUniform("view", camera.GetViewMatrix());
        matCircleInstanced.SetUniform("tint", new Color(1, 1, 1));
        matCircleInstanced.SetUniform("projection", camera.GetProjectionMatrix());

        circleStorage.Use();
        circleStorage.Upload();

        circleMesh.DrawInstanced(circleStorage.GetCurrentIndex());
        circleStorage.Reset();
    }
    
    public void RegisterRectCenterd(Vec2 center, Vec2 size, Color color)
    {
        rectCenteredStorage.Register(new RectCenteredRenderInfo
        { 
            Center = center, 
            Size = size,
            Color = color
        });
    }

    public void RegisterLineCentered(Vec2 pos, Vec2 dir, Color color, float thickness)
    {
        rectCenteredStorage.Register(new RectCenteredRenderInfo
        { 
            Center = pos, 
            Size =  new Vec2(dir.Length(), thickness),
            Color = color,
            Rotation = MathF.Atan2(dir.Y, dir.X)
        });
    }
    
    public void RegisterLine(Vec2 start, Vec2 end, Color color, float thickness)
    {
        var dir = end - start;
        var length = dir.Length();
        var s2 = start + dir/2;

        rectCenteredStorage.Register(new RectCenteredRenderInfo
        { 
            Center = s2, 
            Size =  new Vec2(length, thickness),
            Color = color,
            Rotation = MathF.Atan2(dir.Y, dir.X)
        });
    }

    public void RenderRects(ICamera camera)
    {
        matRectInstanced.Use();
        matRectInstanced.SetUniform("view", camera.GetViewMatrix());
        matRectInstanced.SetUniform("tint", new Color(1, 1, 1));
        matRectInstanced.SetUniform("projection", camera.GetProjectionMatrix());

        rectCenteredStorage.Use();
        rectCenteredStorage.Upload();

        rectCenteredMesh.DrawInstanced(rectCenteredStorage.GetCurrentIndex());
        rectCenteredStorage.Reset();
    }


  
}