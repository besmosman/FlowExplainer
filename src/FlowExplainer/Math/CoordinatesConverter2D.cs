using System.Numerics;

namespace FlowExplainer;

public  static class CoordinatesConverter2D
{
    //source: claude
    public static Vec2 ViewToWorld(View v, Vec2 windowRelativeCoords)
    {
        var screenCoord = windowRelativeCoords;
        // 1. Convert to NDC (-1 to 1)
        var ndcX = (screenCoord.X / v.Width) * 2f - 1f;
        var ndcY = 1f - (screenCoord.Y / v.Height) * 2f; // Invert Y

        // 2. Homogeneous clip space
        var clipPos = new Vector4(ndcX, ndcY, 0, 1);

        Matrix4x4.Invert(v.Camera2D.GetViewMatrix() * v.Camera2D.GetProjectionMatrix(), out var invViewProj);

        var worldPos = Vector4.Transform(clipPos, invViewProj);

        return new Vec2(worldPos.X, worldPos.Y);
    }
}