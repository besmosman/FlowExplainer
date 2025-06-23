using ImGuiNET;

namespace FlowExplainer;

public class DataService : WorldService
{
    public IEditabalePeriodicVectorField<Vec3, Vec2> VelocityField = new SpeetjensAdaptedVelocityField();
    public IIntegrator<Vec3, Vec2> Integrator = new RungeKutta4Integrator();
    public ColorGradient ColorGradient { get; set; } = Gradients.GetGradient("matlab_jet");
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
        ColorGradient = Gradients.GetGradient("matlab_cool");
    }

    public override void DrawImGuiEdit()
    {
        ImGui.SliderFloat("Time Multiplier", ref TimeMultiplier, 0, 10);
        ImGui.SeparatorText("Velocity field");
        VelocityField.OnImGuiEdit();
        base.DrawImGuiEdit();
    }


    public override void Initialize()
    {
    }
}