using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class GridVisualizer : WorldService, IAxisTitle, IGradientScaler
{
    public struct CellData : IMultiplyOperators<CellData, float, CellData>, IAdditionOperators<CellData, CellData, CellData>
    {
        public float Value;
        public float Marked;
        public Vec2 Padding;
        public Color Color;

        public static CellData operator *(CellData left, float right)
        {
            return new CellData
            {
                Value = left.Value * right,
                Color = left.Color * right,
                Marked = left.Marked * right,
            };
        }
        public static CellData operator +(CellData left, CellData right)
        {
            return new CellData
            {
                Value = left.Value + right.Value,
                Color = left.Color + right.Color,
                Marked = left.Marked + right.Marked,
            };
        }
    }

    private IGridDiagnostic? diagnostic;
    // public InterpolatedRenderGrid gridData;
    public bool Continous = true;
    public bool MarkDirty = false;
    public int TargetCellCount = 10000;

    public RegularGridVectorField<Vec2, Vec2i, CellData> RegularGrid;
    public StorageBuffer<CellData> gridbuffer;
    private Material material;

    public bool AutoScale = true;
    public List<IGridDiagnostic> Diagnostics =
    [
        //new VelocityMagnitudeGridDiagnostic(),
        new LICGridDiagnostic(),
        new LagrangianTemperatureGridDiagnostic(),
        new TemperatureGridDiagnostic(),
        new FTLEGridDiagnostic(),
        new LAVDGridDiagnostic(),
        new FunctionGridDiagnostic(),
        new LcsVelocityMagnitudeGridDiagnostic(),
        new HeatStructureGridDiagnostic(),
        // new CustomGridDiagnostic(),
        //new FTLEvsCustomGridDiagnostic(),
    ];

    public override void Initialize()
    {
        SetGridDiagnostic(new TemperatureGridDiagnostic());
        material = new Material(Shader.DefaultWorldSpaceVertex, new Shader("Assets/Shaders/grid-reg.frag", ShaderType.FragmentShader));
    }

    public void SetGridDiagnostic(IGridDiagnostic visualizer)
    {
        if (visualizer.GetType() == typeof(LICGridDiagnostic))
            Continous = false;

        diagnostic = visualizer;
        var dat = GetRequiredWorldService<DataService>();
        var aspect = Vec2.Normalize(dat.VectorField.Domain.Boundary.Size.Down());
        float scale = MathF.Sqrt(TargetCellCount / (aspect.X * aspect.Y));
        int width = Math.Max(1, (int)Math.Round(aspect.X * scale));
        int height = Math.Max(1, (int)Math.Round(aspect.Y * scale));
        RegularGrid = new(new Vec2i(width, height), dat.VectorField.Domain.Boundary.Min.XY, dat.VectorField.Domain.Boundary.Max.XY);
        gridbuffer = new StorageBuffer<CellData>(RegularGrid.Grid.Data);
        MarkDirty = true;
    }

    private object lastVelField;
    public override void Draw(RenderTexture rendertarget, View view)
    {

        if (!view.Is2DCamera)
            return;

        if (diagnostic != null)
        {
            var dat = GetRequiredWorldService<DataService>();
            if (lastVelField != dat.VectorField)
            {
                MarkDirty = true;
                lastVelField = dat.VectorField;
                Resize();
            }

            if (Continous || MarkDirty)
            {
                UpdateData();
                MarkDirty = false;
            }
            var camera = view.Camera2D;
            material.Use();
            material.SetUniform("gridSize", RegularGrid.GridSize.ToVec2());
            material.SetUniform("tint", new Color(1, 1, 0, 1));
            material.SetUniform("interpolate", RegularGrid.Interpolate);
            material.SetUniform("view", camera.GetViewMatrix());
            material.SetUniform("projection", camera.GetProjectionMatrix());
            material.SetUniform("useCustomColor", diagnostic.UseCustomColoring);
            material.SetUniform("colorgradient", dat.ColorGradient.Texture.Value);
            material.SetUniform("minGrad", AutoScale ? min : 0f);
            material.SetUniform("maxGrad", AutoScale ? max : 1f);
            var size = RegularGrid.Domain.Boundary.Size;
            var start = RegularGrid.Domain.Boundary.Min;
            material.SetUniform("model", Matrix4x4.CreateScale(size.X, size.Y, .4f) * Matrix4x4.CreateTranslation(start.X, start.Y, 0));
            gridbuffer.Use();

            Gizmos2D.imageQuadInvertedY.Draw();
            var boundary = dat.VectorField.Domain.Boundary;
        }
    }

    private float min;
    private float max;
    private Stopwatch s = new();
    private void UpdateData()
    {
        if (diagnostic == null)
            throw new Exception();

        s.Restart();
        diagnostic.UpdateGridData(this);
        if (s.Elapsed.TotalSeconds > 1 / 5f)
            Continous = false;
        gridbuffer.Use();
        gridbuffer.Upload();
        min = float.MaxValue;
        max = float.MinValue;
        for (int i = 0; i < gridbuffer.Data.Length; i++)
        {
            var v = gridbuffer.Data[i].Value;
            if (float.IsRealNumber(v))
            {
                min = float.Min(min, v);
                max = float.Max(max, v);
            }
        }
        max = float.Max(max, min + .00001f);
    }

    public override void DrawImGuiEdit()
    {
        ImGui.SliderInt("CellCount", ref TargetCellCount, 16, 512 * 512);

        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            Resize();
            UpdateData();
        }

        if (ImGui.BeginCombo("Diagnostic", diagnostic!.Name))
        {
            foreach (var dia in Diagnostics)
            {
                bool selected = dia == diagnostic;
                if (ImGui.Selectable(dia.Name, ref selected))
                {
                    SetGridDiagnostic(dia);
                }
            }

            ImGui.EndCombo();
        }

        ImGui.Checkbox("Continous", ref Continous);
        ImGui.Checkbox("Auto scale", ref AutoScale);
        ImGui.Checkbox("Bilinear", ref RegularGrid.Interpolate);
        if (!Continous)
        {
            if (ImGui.Button("Recompute"))
                UpdateData();
        }

        diagnostic?.OnImGuiEdit(this);
        base.DrawImGuiEdit();
    }

    private void Resize()
    {
        var dat = GetRequiredWorldService<DataService>();
        var aspect = Vec2.Normalize(dat.VectorField.Domain.Boundary.Size.Down());
        float scale = MathF.Sqrt(TargetCellCount / (aspect.X * aspect.Y));
        int width = Math.Max(1, (int)Math.Round(aspect.X * scale));
        int height = Math.Max(1, (int)Math.Round(aspect.Y * scale));
        bool interpolate = RegularGrid.Interpolate;
        RegularGrid = new(new Vec2i(width, height), dat.VectorField.Domain.Boundary.Min.XY, dat.VectorField.Domain.Boundary.Max.XY);
        RegularGrid.Interpolate = interpolate;
        gridbuffer = new(RegularGrid.Grid.Data);
    }

    public string GetTitle()
    {
        if (diagnostic.GetType() == typeof(LICGridDiagnostic))
            return $"LIC {GetRequiredWorldService<DataService>().currentSelectedVectorField}";
        return diagnostic?.Name ?? "";
    }
    public (float min, float max) GetScale()
    {
        if (AutoScale)
            return (min, max);
        return (0, 1);
    }
    public float ScaleScaler(float value)
    {
        if (AutoScale)
            return (value - min) / (max - min);
        return value;
    }
    public void Save(string path, float t_start, float t_end, int timeSteps)
    {
        MarkDirty = true;
        UpdateData();
        
        var gridSize = new Vec3i(RegularGrid.Grid.GridSize.X /1, RegularGrid.Grid.GridSize.Y / 1, timeSteps);
        var domain = new Rect<Vec3>(RegularGrid.Domain.Boundary.Min.Up(t_start), RegularGrid.Domain.Boundary.Max.Up(t_end));
        var spatialDomain = domain.Reduce<Vec2>();
        var field = new RegularGridVectorField<Vec3, Vec3i, float>(gridSize, new RectDomain<Vec3>(domain));

        for (int i_t = 0; i_t < timeSteps-1; i_t++)
        {
            float t = float.Lerp(t_start, t_end, i_t / (float)(timeSteps - 1));
            GetRequiredWorldService<DataService>().SimulationTime = t;
            MarkDirty = true;
            UpdateData();
            ParallelGrid.For(gridSize.XY, (i_x, i_y) =>
            {
                var pos = spatialDomain.Relative(new Vec2(i_x + .5f, i_y + .5f) / gridSize.XY.ToVec2());
                field.AtCoords(new Vec3i(i_x, i_y, i_t)) = RegularGrid.Evaluate(pos).Value;
            });
        }

        field.Save(path);
    }
}