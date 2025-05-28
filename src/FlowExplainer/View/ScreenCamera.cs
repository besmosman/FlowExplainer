using System.Numerics;

namespace FlowExplainer;

public struct ScreenCamera : ICamera
{
    public Vector2 Size;

    public bool InvertedY()
    {
        return true;
    }

    public ScreenCamera(Vector2 size)
    {
        Size = size;
    }

    public Matrix4x4 GetProjectionMatrix()
    {
        return Matrix4x4.CreateOrthographicOffCenter(0, Size.X, Size.Y, 0, 0, 1);
    }

    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.Identity;
    }
}