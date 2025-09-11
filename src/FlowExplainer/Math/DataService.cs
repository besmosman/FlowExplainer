using System.Numerics;
using ImGuiNET;

namespace FlowExplainer;

public class DataService : WorldService
{
    public IVectorField<Vec3, Vec2> VelocityField;
    public IVectorField<Vec3, float> TempratureField;
    public IIntegrator<Vec3, Vec2> Integrator = IIntegrator<Vec3, Vec2>.Rk4;
    public ColorGradient ColorGradient { get; set; } = Gradients.GetGradient("matlab_parula");
    public float SimulationTime;

    public float TimeMultiplier = .0f;

    public override ToolCategory Category => ToolCategory.Data;

    public float DeltaTime;

    public override void Initialize()
    {
        SetGyreDataset();
        /*
        var gribLoader = new GribLoader();
        gribLoader.Load();
        VelocityField = gribLoader.VelocityField;
        TempratureField = gribLoader.HeatField;
        */

        /*
        bubble.Load();
        var bubble = new BubbleMlLoader();
        VelocityField = bubble.VelocityField;
        TempratureField = bubble.TemperatureField;
        */

    }
    private float timeAbove = 0f;
    public override void Update()
    {
        if (SimulationTime > VelocityField.Domain.Boundary.Max.Z)
        {
            timeAbove += FlowExplainer.DeltaTime;
            if (timeAbove > 1)
            {
                SimulationTime = 0;
                timeAbove = 0;
            }
        }
        else
        {
            timeAbove = 0;
        }
        base.Update();
    }
    private bool firstDraw = true;
    public override void Draw(RenderTexture rendertarget, View view)
    {
        if (firstDraw)
        {
            view.Camera2D.Position = -VelocityField.Domain.Boundary.Center.Down();
            view.Camera2D.Scale = float.Min(view.Width / VelocityField.Domain.Boundary.Size.X / 1.4f, view.Height / VelocityField.Domain.Boundary.Size.Y / 1.4f);
            firstDraw = false;
        }
        //VelocityField = new PeriodicDiscritizedField(new AnalyticalEvolvingVelocityField(), new Vec3(.01f, .01f, .01f));
        float dt = FlowExplainer.DeltaTime;
        //dt = 1f / 90f;
        DeltaTime = dt * TimeMultiplier;
        SimulationTime += DeltaTime;
    }

    private string dataset;
    public override void DrawImGuiEdit()
    {
        ImGuiHelpers.SliderFloat("Time Multiplier", ref TimeMultiplier, 0, 10);
        ImGuiHelpers.SliderFloat("Time", ref SimulationTime, 0, VelocityField.Domain.Boundary.Size.Z);

        VelocityField.OnImGuiEdit();

        if (ImGui.BeginCombo("Dataset", dataset))
        {
            if (ImGui.Selectable("Spectral Double Gyre"))
            {
                SetGyreDataset();
            }
            if (ImGui.Selectable("Weather Data 1"))
            {
                var ncLoader = new NcLoader();
                ncLoader.Load();
                VelocityField = ncLoader.VelocityField;
                TempratureField = ncLoader.HeatField;
                dataset = "Weather Data 1";
            }
            ImGui.EndCombo();
        }


        ImGui.Columns(2);
        ImGui.SetColumnWidth(0, ImGui.GetTextLineHeightWithSpacing() * 1.4f);
        ImGui.Image(ColorGradient.Texture.Value.TextureHandle, new Vec2(ImGui.GetTextLineHeightWithSpacing(), ImGui.GetTextLineHeightWithSpacing()), new Vec2(0, 0), new Vec2(1, 1));
        ImGui.NextColumn();
        if (ImGui.BeginCombo("Gradient", ColorGradient.Name))
        {
            foreach (var grad in Gradients.All)
            {
                bool isSelected = ColorGradient == grad;
                ImGui.Image(grad.Texture.Value.TextureHandle, new Vec2(ImGui.GetTextLineHeight(), ImGui.GetTextLineHeight()), new Vec2(0, 0), new Vec2(1, 1));
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
    private void SetGyreDataset()
    {

        VelocityField = new SpeetjensVelocityField()
        {
            epsilon = .1f,
        };
        string folderPath = Config.GetValue<string>("spectral-data-path")!;

        // TempratureField = temprature;
        dataset = "Spectral Double Gyre";
    }



}