using System.Numerics;

namespace FlowExplainer;

public struct DirectCamera : ICamera
{
    private Matrix4x4 Projection;
    private Matrix4x4 View;

    public DirectCamera(Matrix4x4 projection, Matrix4x4 view)
    {
        Projection = projection;
        View = view;
    }

    public Matrix4x4 GetProjectionMatrix()
    {
        return Projection;
    }

    public Matrix4x4 GetViewMatrix()
    {
        return View;
    }
}

public class Camera2D : ICamera
{
    public Vec2 RenderTargetSize;
    public Vec2 Position;
    public double Scale;

    public bool InvertedY()
    {
        return false;
    }

    public ICamera RenderTargetRelative(RenderTexture renderTexture,  Rect<Vec2> bounds)
    {
        return new DirectCamera(
            Matrix4x4.CreateOrthographicOffCenter((float)bounds.Min.X, (float)bounds.Max.X, (float)bounds.Min.Y,(float) bounds.Max.Y, 0, 1),
            Matrix4x4.CreateScale(1)
        );
    }

    public Matrix4x4 GetProjectionMatrix()
    {
        return Matrix4x4.CreateOrthographic((float)RenderTargetSize.X, (float)RenderTargetSize.Y, 0, 1);
    }

    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.CreateTranslation((float)Position.X, (float)Position.Y, 0) *
               Matrix4x4.CreateScale((float)Scale);
    }
}