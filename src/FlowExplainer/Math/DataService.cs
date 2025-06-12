using ImGuiNET;

namespace FlowExplainer;

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