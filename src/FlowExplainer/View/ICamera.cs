using System.Numerics;

namespace FlowExplainer;

public interface ICamera
{
    public bool InvertedY() => false;
    public Matrix4x4 GetProjectionMatrix();
    public Matrix4x4 GetViewMatrix();
}