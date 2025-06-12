using System.Numerics;
using ImGuiNET;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FlowExplainer;

public  static class CoordinatesConverter2D
{
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

public class ViewController2D : WorldService
{
    public override void Initialize()
    {
    }

    private Vec2 lastClickPos = Vec2.Zero;
    private Vec2 startCamPos = Vec2.Zero;

    public override void Draw(RenderTexture rendertarget, View view)
    {
        var window = GetRequiredGlobalService<WindowService>().Window;
        if (window.IsMouseButtonPressed(MouseButton.Right))
        {
            lastClickPos = CoordinatesConverter2D.ViewToWorld(view, view.RelativeMousePosition);;
            startCamPos  = view.Camera2D.Position;
        }

        if (window.IsMouseButtonDown(MouseButton.Right))
        {
            view.Camera2D.Position = startCamPos;
            var cur = CoordinatesConverter2D.ViewToWorld(view, view.RelativeMousePosition);
            view.Camera2D.Position = startCamPos - (lastClickPos - cur);
            //Gizmos2D.Rect(view.Camera2D, lastClickPos, cur, new Vec4(1, 1, 1, .2f));
        }
        Logger.LogDebug(view.Camera2D.Position.ToString());

        if (window.MouseState.ScrollDelta.Y != 0)
        {
            view.Camera2D.Scale *= 1f + (window.MouseState.ScrollDelta.Y)*.02f;
        }
    }
}

public class DataService : WorldService
{
    public AnalyticalEvolvingVelocityField VelocityField = new AnalyticalEvolvingVelocityField();
    public IIntegrator<Vec3, Vec2> Integrator = new RungeKutta4Integrator();
    public Rect Domain = new Rect(new Vec2(0, 0), new Vec2(2, 1));
    public float SimulationTime;

    public float TimeMultiplier = .1f;

    public override ToolCategory Category => ToolCategory.Simulation;
    public float DeltaTime;

    public override void Draw(RenderTexture rendertarget, View view)
    {
        //VelocityField = new PeriodicDiscritizedField(new AnalyticalEvolvingVelocityField(), new Vec3(.01f, .01f, .01f));
        float dt = FlowExplainer.DeltaTime;
        //dt = 1f / 90f;
        DeltaTime = dt * TimeMultiplier;
        SimulationTime += DeltaTime;
    }

    public override void DrawImGuiEdit()
    {
        ImGui.SliderFloat("Time Multiplier", ref TimeMultiplier, 0, 10);
        ImGui.SliderFloat("A", ref VelocityField.A, 0, 10);
        ImGui.SliderFloat("Elipson", ref VelocityField.elipson, 0, 2);
        ImGui.SliderFloat("w", ref VelocityField.w, 0, 2);
        base.DrawImGuiEdit();
    }


    public override void Initialize()
    {
    }
}