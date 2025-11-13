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
        public float PositionX;
        public float PositionY;
        public float Radius;
        public float Padding;

        public float ColorR;
        public float ColorG;
        public float ColorB;
        public float ColorA;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RectCenteredRenderInfo
    {
        public float CenterX;
        public float CenterY;
        public float SizeX;
        public float SizeY;

        public float ColorR;
        public float ColorG;
        public float ColorB;
        public float ColorA;

        public float paddingX;
        public float paddingY;
        public float paddingZ;
        public float Rotation;
    }


    private Mesh circleMesh => Gizmos2D.circleMesh;
    private Mesh rectCenteredMesh => Gizmos2D.quadMeshCentered;
    private AutoExpandStorageBuffer<CircleRenderInfo> circleStorage = new();
    private AutoExpandStorageBuffer<RectCenteredRenderInfo> rectCenteredStorage = new();
    private Material matCircleInstanced = new Material(new Shader("Assets/Shaders/instancedCircle.vert", ShaderType.VertexShader), Shader.DefaultUnlitFragment);
    private Material matRectInstanced = new Material(new Shader("Assets/Shaders/instancedRectCentered.vert", ShaderType.VertexShader), Shader.DefaultUnlitFragment);


    public void RegisterCircle(Vec2 center, double radius, Color color)
    {
        circleStorage.Register(new CircleRenderInfo()
        {
            PositionX = (float)center.X,
            PositionY = (float)center.Y,
            Radius = (float)radius,
            ColorR = (float)color.R,
            ColorG = (float)color.G,
            ColorB = (float)color.B,
            ColorA = (float)color.A,
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
            CenterX = (float)center.X,
            CenterY = (float)center.Y,
            SizeX = (float)size.X,
            SizeY = (float)size.Y,
            ColorR = (float)color.R,
            ColorG = (float)color.G,
            ColorB = (float)color.B,
            ColorA = (float)color.A,

        });
    }

    public void RegisterLineCentered(Vec2 pos, Vec2 dir, Color color, double thickness)
    {
        rectCenteredStorage.Register(new RectCenteredRenderInfo
        {
            CenterX = (float)pos.X,
            CenterY = (float)pos.Y,
            SizeX = (float)dir.Length(),
            SizeY = (float)thickness,
            ColorR = (float)color.R,
            ColorG = (float)color.G,
            ColorB = (float)color.B,
            ColorA = (float)color.A,
            Rotation = (float)Math.Atan2(dir.Y, dir.X)
        });
    }

    public void RegisterLine(Vec2 start, Vec2 end, Color color, double thickness)
    {
        var dir = end - start;
        var length = dir.Length();
        var s2 = start + dir / 2;

        rectCenteredStorage.Register(new RectCenteredRenderInfo
        {
            CenterX = (float)s2.X,
            CenterY = (float)s2.Y,
            SizeX = (float)length,
            SizeY = (float)thickness,
            ColorR = (float)color.R,
            ColorG = (float)color.G,
            ColorB = (float)color.B,
            ColorA = (float)color.A,
            Rotation = (float)Math.Atan2(dir.Y, dir.X)
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