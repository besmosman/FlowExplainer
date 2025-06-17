using ImGuiNET;

namespace FlowExplainer;

public class DataService : WorldService
{
    public IEditabalePeriodicVectorField<Vec3, Vec2> VelocityField = new SpeetjensAdaptedVelocityField();
    public IIntegrator<Vec3, Vec2> Integrator = new RungeKutta4Integrator();
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

       // var f = (SpeetjensAdaptedVelocityField)VelocityField;
       // var dx = f.elipson * float.Sin(2 * float.Pi * SimulationTime) / 4;
       // var x_plus = new Vec2(1 / 4f + dx, 1 / 4f);
       // Gizmos2D.Circle(view.Camera2D, x_plus, new Color(1,1,0,1), 0.01f);
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