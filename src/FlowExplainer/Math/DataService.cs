using ImGuiNET;

namespace FlowExplainer;

public class DataService : WorldService
{
    public IEditabalePeriodicVectorField<Vec3, Vec2> VelocityField = new SpeetjensAdaptedVelocityField();
    public IIntegrator<Vec3, Vec2> Integrator = new RungeKutta4Integrator();
    public ColorGradient ColorGradient { get; set; } = Gradients.GetGradient("matlab_jet");
    public float SimulationTime;

    public float TimeMultiplier = .0f;

    public override ToolCategory Category => ToolCategory.Data;
    
    public float DeltaTime;

    public override void Draw(RenderTexture rendertarget, View view)
    {
        //VelocityField = new PeriodicDiscritizedField(new AnalyticalEvolvingVelocityField(), new Vec3(.01f, .01f, .01f));
        float dt = FlowExplainer.DeltaTime;
        //dt = 1f / 90f;
        DeltaTime = dt * TimeMultiplier ;
        SimulationTime += DeltaTime;
    }

    public override void DrawImGuiEdit()
    {
        ImGuiHelpers.SliderFloat("Time Multiplier", ref TimeMultiplier, 0, 10);
        VelocityField.OnImGuiEdit();
        ImGui.Columns(2);
        ImGui.SetColumnWidth(0, ImGui.GetTextLineHeightWithSpacing()*1.4f);
        ImGui.Image(ColorGradient.Texture.Value.TextureHandle, new Vec2(ImGui.GetTextLineHeightWithSpacing(),ImGui.GetTextLineHeightWithSpacing()), new Vec2(0,0), new Vec2(1,1));
        ImGui.NextColumn();
        if (ImGui.BeginCombo("Gradient", ColorGradient.Name))
        {
            foreach (var grad in Gradients.All)
            {
                bool isSelected = ColorGradient == grad;
                ImGui.Image(grad.Texture.Value.TextureHandle, new Vec2(ImGui.GetTextLineHeight(),ImGui.GetTextLineHeight()), new Vec2(0,0), new Vec2(1,1));
                ImGui.SameLine();
                if (ImGui.Selectable(grad.Name, ref isSelected))
                {
                    ColorGradient = grad;
                }
            }
            ImGui.EndCombo();
        }
        ImGui.Columns(1);
        base.DrawImGuiEdit();
    }


    public override void Initialize()
    {
    }
}