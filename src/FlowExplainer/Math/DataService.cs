using ImGuiNET;

namespace FlowExplainer;

public class DataService : WorldService
{
    public IPeriodicVectorField<Vec3, Vec2> VelocityField = new AnalyticalEvolvingVelocityField();
    public IIntegrator<Vec3, Vec2> Integrator = new RungeKutta4Integrator();
    public Rect Domain = new Rect(new Vec2(0, 0), new Vec2(2, 1));
    public float SimulationTime;

    public float TimeMultiplier = .1f;

    public override ToolCategory Category => ToolCategory.Simulation;
    public float DeltaTime;

    public override void Draw(RenderTexture rendertarget, View view)
    {
        VelocityField = new AnalyticalEvolvingVelocityField();
        float dt = (float)FlowExplainer.DeltaTime.TotalSeconds;
        dt = 1f / 90f;
        DeltaTime = dt * TimeMultiplier;
        SimulationTime += DeltaTime;
    }

    public override void DrawImGuiEdit()
    {
        ImGui.SliderFloat("Time Multiplier", ref TimeMultiplier, 0, 10);
        base.DrawImGuiEdit();
    }


    public override void Initialize()
    {
    }
}