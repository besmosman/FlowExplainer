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
    public float Scale;

    public bool InvertedY()
    {
        return false;
    }

    public ICamera RenderTargetRelative(RenderTexture renderTexture,  Rect<Vec2> bounds)
    {
        return new DirectCamera(
            Matrix4x4.CreateOrthographicOffCenter(bounds.Min.X, bounds.Max.X, bounds.Min.Y, bounds.Max.Y, 0, 1),
            Matrix4x4.CreateScale(1)
        );
    }

    public Matrix4x4 GetProjectionMatrix()
    {
        return Matrix4x4.CreateOrthographic(RenderTargetSize.X, RenderTargetSize.Y, 0, 1);
    }

    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.CreateTranslation(Position.X, Position.Y, 0) *
               Matrix4x4.CreateScale(Scale);
    }
}