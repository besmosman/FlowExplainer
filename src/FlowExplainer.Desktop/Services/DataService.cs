using System.Numerics;
using ImGuiNET;

namespace FlowExplainer;

public class DataService : WorldService
{
    public Dataset LoadedDataset = null!;

    public IVectorField<Vec3, Vec2> VectorField => LoadedDataset.VectorFields[currentSelectedVectorField];
    public IVectorField<Vec3, Vec2> VectorFieldInstant => new InstantField<Vec3, Vec2>(VectorField, SimulationTime);

    public IVectorField<Vec3, double> ScalerField => LoadedDataset.ScalerFields[currentSelectedScaler];
    public IVectorField<Vec3, double> ScalerFieldInstant => new InstantField<Vec3, double>(ScalerField, SimulationTime);

    public string currentSelectedScaler = "Total Temperature";
    public string currentSelectedVectorField = "Velocity";

    public ColorGradient ColorGradient { get; set; } = Gradients.GetGradient("matlab_parula");
    public double SimulationTime;

    public double TimeMultiplier = 0;


    public double MultipliedDeltaTime { get; private set; }

    public override string? Name => "Dataset";
    public override string? CategoryN => "General";
    public override string? Description => "Global dataset settings";



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

    private double timeAbove = 0.0;

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

    public void SetDataset(string name)
    {
        LoadedDataset = GetRequiredGlobalService<DatasetsService>().Datasets[name];
        if (!LoadedDataset.Loaded)
        {
            LoadedDataset.Load(LoadedDataset);
            LoadedDataset.Loaded = true;
        }
    }

    public override IEnumerable<ISelectableVectorField<Vec3, double>> GetSelectableVec3Vec1()
    {
        foreach (var f in LoadedDataset.ScalerFields)
            yield return new SelectableVectorField<Vec3, double>(f.Key, f.Value);
    }

    public override IEnumerable<ISelectableVectorField<Vec3, Vec2>> GetSelectableVec3Vec2()
    {
        foreach (var f in LoadedDataset.VectorFields)
            yield return new SelectableVectorField<Vec3, Vec2>(f.Key, f.Value);
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        if (firstDraw)
        {
            view.Camera2D.Position = -VectorField.Domain.RectBoundary.Center.Down();
            view.Camera2D.Scale = double.Min(view.Width / VectorField.Domain.RectBoundary.Size.X / 1.4f, view.Height / VectorField.Domain.RectBoundary.Size.Y / 1.4f);
            firstDraw = false;
        }

        //VelocityField = new PeriodicDiscritizedField(new AnalyticalEvolvingVelocityField(), new Vec3(.01f, .01f, .01f));
        double dt = FlowExplainer.DeltaTime;
        //dt = 1f / 90f;
        MultipliedDeltaTime = dt * TimeMultiplier;
        SimulationTime += MultipliedDeltaTime;
    }


    public override void DrawImGuiSettings()
    {
        ImGuiHelpers.Slider("Time Multiplier", ref TimeMultiplier, 0, 10);
        ImGuiHelpers.Slider("Time", ref SimulationTime, 0, LoadedDataset.VectorFields[currentSelectedVectorField].Domain.RectBoundary.Size.Z);

        VectorField.OnImGuiEdit();

        if (ImGui.BeginCombo("Dataset", LoadedDataset.Name))
        {
            foreach (var v in GetRequiredGlobalService<DatasetsService>().Datasets)
                if (ImGui.Selectable(v.Key))
                    SetDataset(v.Key);
            ImGui.EndCombo();
        }

        if (ImGui.BeginCombo("Scaler Field", currentSelectedScaler))
        {
            foreach (var v in LoadedDataset.ScalerFields)
                if (ImGui.Selectable(v.Key))
                    currentSelectedScaler = v.Key;
            ImGui.EndCombo();
        }

        if (ImGui.BeginCombo("Vector Field", currentSelectedVectorField))
        {
            foreach (var v in LoadedDataset.VectorFields)
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
        base.DrawImGuiSettings();
    }


    public void LoadScalerField(string name, string path)
    {
        var regularGridVectorField = RegularGridVectorField<Vec3, Vec3i, double>.Load(path);
        LoadedDataset.ScalerFields.Add(name, regularGridVectorField);
    }
}