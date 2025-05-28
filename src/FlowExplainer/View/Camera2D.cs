using System.Numerics;

namespace FlowExplainer;

public class Camera2D : ICamera
{
    public Vector2 RenderTargetSize;
    public Vector2 Position;
    public float Scale;

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