using System.Numerics;
using ImGuiNET;

namespace FlowExplainer;

public class DataService : WorldService
{
    public IVectorField<Vec3, Vec2> VectorField => VectorFields[currentSelectedVectorField];
    public IVectorField<Vec3, Vec2> VectorFieldInstant => new InstantField<Vec3, Vec2>(VectorField, SimulationTime);

    public IVectorField<Vec3, float> TempratureField => ScalerFields[currentSelectedScaler];
    public IVectorField<Vec3, float> TempratureFieldInstant => new InstantField<Vec3, float>(TempratureField, SimulationTime);

    public string currentSelectedScaler = "Total Temperature";
    public string currentSelectedVectorField = "Velocity";
    public Dictionary<string, IVectorField<Vec3, float>> ScalerFields = new();
    public Dictionary<string, IVectorField<Vec3, Vec2>> VectorFields = new();

    public ColorGradient ColorGradient { get; set; } = Gradients.GetGradient("matlab_parula");
    public float SimulationTime;

    public float TimeMultiplier = .06f;

    public override ToolCategory Category => ToolCategory.Data;

    public float MultipliedDeltaTime { get; private set; }

    public override void Initialize()
    {
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
        /*
        if (SimulationTime > VectorField.Domain.RectBoundary.Max.Z)
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
        */

        base.Update();
    }

    private bool firstDraw = true;

    public override void Draw(RenderTexture rendertarget, View view)
    {
        if (firstDraw)
        {
            view.Camera2D.Position = -VectorField.Domain.RectBoundary.Center.Down();
            view.Camera2D.Scale = float.Min(view.Width / VectorField.Domain.RectBoundary.Size.X / 1.4f, view.Height / VectorField.Domain.RectBoundary.Size.Y / 1.4f);
            firstDraw = false;
        }

        //VelocityField = new PeriodicDiscritizedField(new AnalyticalEvolvingVelocityField(), new Vec3(.01f, .01f, .01f));
        float dt = FlowExplainer.DeltaTime;
        //dt = 1f / 90f;
        MultipliedDeltaTime = dt * TimeMultiplier;
        SimulationTime += MultipliedDeltaTime;
    }


    public override void DrawImGuiEdit()
    {
        ImGuiHelpers.SliderFloat("Time Multiplier", ref TimeMultiplier, 0, 10);
        ImGuiHelpers.SliderFloat("Time", ref SimulationTime, 0, ScalerFields[currentSelectedScaler].Domain.RectBoundary.Size.Z);

        VectorField.OnImGuiEdit();
        
        if (ImGui.BeginCombo("Scaler Field", currentSelectedScaler))
        {
            foreach (var v in ScalerFields)
                if (ImGui.Selectable(v.Key))
                    currentSelectedScaler = v.Key;
            ImGui.EndCombo();
        }

        if (ImGui.BeginCombo("Vector Field", currentSelectedVectorField))
        {
            foreach (var v in VectorFields)
                if (ImGui.Selectable(v.Key))
                    currentSelectedVectorField = v.Key;
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


    public void LoadScalerField(string name, string path)
    {
        var regularGridVectorField = RegularGridVectorField<Vec3, Vec3i, float>.Load(path);
        ScalerFields.Add(name, regularGridVectorField);
    }
}