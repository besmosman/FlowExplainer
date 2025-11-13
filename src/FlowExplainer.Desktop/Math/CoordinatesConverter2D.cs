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
        var clipPos = new Vector4((float)ndcX, (float)ndcY, 0, 1);

        Matrix4x4.Invert(v.Camera2D.GetViewMatrix() * v.Camera2D.GetProjectionMatrix(), out var invViewProj);

        var worldPos = Vector4.Transform(clipPos, invViewProj);

        return new Vec2(worldPos.X, worldPos.Y);
    }
    public static Vec2 WorldToView(View v, Vec2 worldCoords)
    {
        // 1. Convert world coordinates to homogeneous coordinates
        var worldPos = new Vector4((float)worldCoords.X, (float)worldCoords.Y, 0, 1);
    
        // 2. Transform to clip space using view-projection matrix
        var viewProjMatrix = v.Camera2D.GetViewMatrix() * v.Camera2D.GetProjectionMatrix();
        var clipPos = Vector4.Transform(worldPos, viewProjMatrix);
    
        // 3. Convert from clip space to NDC (normalized device coordinates)
        var ndcX = clipPos.X;
        var ndcY = clipPos.Y;
    
        // 4. Convert from NDC (-1 to 1) to window coordinates
        var screenX = (ndcX + 1f) * 0.5f * v.Width;
        var screenY = (1f - ndcY) * 0.5f * v.Height; // Invert Y back to screen space
    
        return new Vec2(screenX, screenY);
    }
}