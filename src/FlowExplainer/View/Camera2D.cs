using System.Numerics;

namespace FlowExplainer;

public class Camera2D : ICamera
{
    public Vec2 RenderTargetSize;
    public Vec2 Position;
    public float Scale;
    public bool InvertedY()
    {
        return false;
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