using System.Numerics;

namespace FlowExplainer;

public interface ICamera
{
    public Vec2 LastRenderSize { get; set; }
    public Vec2 EffectiveRenderPixels { get; set; }
    public bool InvertedY() => false;
    public Matrix4x4 GetProjectionMatrix();
    public Matrix4x4 GetViewMatrix();
}