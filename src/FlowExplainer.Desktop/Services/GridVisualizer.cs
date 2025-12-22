using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class GridVisualizer : WorldService, IAxisTitle, IGradientScaler
{
    public struct CellData : IMultiplyOperators<CellData, double, CellData>, IAdditionOperators<CellData, CellData, CellData>
    {
        public double Value;
        public double Marked;
        public Vec2 Padding;
        public Color Color;

        public static CellData operator *(CellData left, double right)
        {
            return new CellData
            {
                Value = left.Value * right,
                Color = left.Color * (float)right,
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

    public IGridDiagnostic diagnostic = new ScalerGridDiagnostic();

    // public InterpolatedRenderGrid gridData;
    public bool Continous = true;
    public bool MarkDirty = false;
    public int TargetCellCount = 10000;

    public RegularGridVectorField<Vec2, Vec2i, CellData> RegularGrid;
    public StorageBuffer<CellData> gridbuffer;
    private Material material;

    public bool AutoScale = true;

    public override string? Name => "Grid";
    public override string? CategoryN => "General";
    public override string? Description => "Compute and render diagnostics on a interpolated grid.";

    public List<IGridDiagnostic> Diagnostics =
    [
        //new VelocityMagnitudeGridDiagnostic(),
        new LICGridDiagnostic(),
        new LICS(),
        new LagrangianTemperatureGridDiagnostic(),
        new ScalerGridDiagnostic(),
        new FTLEGridDiagnostic(),
        new LAVDGridDiagnostic(),
        new FunctionGridDiagnostic(),
        new LcsVelocityMagnitudeGridDiagnostic(),
        new HeatStructureGridDiagnostic(),
        new PoincareSmearGridDiagnostic(),
        new DivergenceGridDiagnostic(),
        new StagnationGridDiagnostic(),
        new UlamsGrid(),
        new CriticalPointDiagnostic(),
        // new CustomGridDiagnostic(),
        //new FTLEvsCustomGridDiagnostic(),
    ];

    public override void Initialize()
    {
        SetGridDiagnostic(new ScalerGridDiagnostic());
        material = new Material(Shader.DefaultWorldSpaceVertex, new Shader("Assets/Shaders/grid-reg.frag", ShaderType.FragmentShader));
    }

    public void SetGridDiagnostic(IGridDiagnostic visualizer)
    {
        if (visualizer.GetType() == typeof(LICGridDiagnostic))
            Continous = false;

        diagnostic = visualizer;
        var dat = GetRequiredWorldService<DataService>();
        var aspect = Vec2.Normalize(dat.VectorField.Domain.RectBoundary.Size.Down());
        double scale = Math.Sqrt(TargetCellCount / (aspect.X * aspect.Y));
        int width = Math.Max(1, (int)Math.Round(aspect.X * scale));
        int height = Math.Max(1, (int)Math.Round(aspect.Y * scale));
        RegularGrid = new(new Vec2i(width, height), dat.VectorField.Domain.RectBoundary.Min.XY, dat.VectorField.Domain.RectBoundary.Max.XY);
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
                ResetGridUpdateTask();
                while (!currentUpdateTask.IsCompleted && currentUpdateGridTime.Elapsed.TotalSeconds < 1 / 2f)
                {
                }

                if (!currentUpdateTask.IsCompleted)
                    Continous = false;
            }
            else
            {

            }

            UpdateRenderData();
            MarkDirty = false;
            var camera = view.Camera2D;
            material.Use();
            material.SetUniform("gridSize", RegularGrid.GridSize.ToVec2());
            material.SetUniform("tint", new Color(1, 1, 0, 1));
            material.SetUniform("interpolate", RegularGrid.Interpolate);
            material.SetUniform("view", camera.GetViewMatrix());
            material.SetUniform("projection", camera.GetProjectionMatrix());
            material.SetUniform("useCustomColor", diagnostic.UseCustomColoring);
            material.SetUniform("colorgradient", dat.ColorGradient.Texture.Value);
            material.SetUniform("minGrad", AutoScale ? min : 0.0);
            material.SetUniform("maxGrad", AutoScale ? max : 1f);
            var size = RegularGrid.Domain.RectBoundary.Size;
            var start = RegularGrid.Domain.RectBoundary.Min;
            material.SetUniform("model", Matrix4x4.CreateScale((float)size.X, (float)size.Y, .4f) * Matrix4x4.CreateTranslation((float)start.X, (float)start.Y, 0));
            gridbuffer.Use();

            Gizmos2D.imageQuadInvertedY.Draw();

            if (!currentUpdateTask.IsCompleted)
            {
                var pos = new Vec2(RegularGrid.Domain.RectBoundary.Center.X, RegularGrid.Domain.RectBoundary.Min.Y + RegularGrid.Domain.RectBoundary.Size.Y * 3 / 4);
                double sizeY = RegularGrid.Domain.RectBoundary.Size.Y / 5;
                var alpha = double.Min(1, double.Max(0, currentUpdateGridTime.Elapsed.TotalSeconds - .0f) * 100);
                Gizmos2D.RectCenter(view.Camera2D, pos, new Vec2(sizeY * 6, sizeY), Color.Black.WithAlpha(alpha));
                Gizmos2D.Text(view.Camera2D, pos, sizeY, Color.White.WithAlpha(alpha), "Recomputing", centered: true);
            }
            var boundary = dat.VectorField.Domain.RectBoundary;
        }
    }

    private double min;
    private double max;
    private Stopwatch currentUpdateGridTime = new();

    private CancellationTokenSource cancellationTokenSource = new();
    private Task currentUpdateTask = Task.CompletedTask;

    private void UpdateRenderData()
    {
        if (diagnostic == null)
            throw new Exception();

        gridbuffer.Use();
        gridbuffer.Upload();
        
        var nextMin = double.MaxValue;
        var nextMax = double.MinValue;
        for (int i = 0; i < gridbuffer.Data.Length; i++)
        {
            var v = gridbuffer.Data[i].Value;
            if (double.IsRealNumber(v))
            {
                nextMin = double.Min(nextMin, v);
                nextMax = double.Max(nextMax, v);
            }
        }
        if (double.Abs(min - nextMin) > .01)
        {
            min = double.Lerp(min, nextMin, 1);
            max = double.Lerp(max, nextMax, 1);
        }
        min = double.Lerp(min, nextMin, .1);
        max = double.Lerp(max, nextMax, .1);
        if (!double.IsRealNumber(min))
            min = nextMin;
        if (!double.IsRealNumber(max))
            max = nextMax;
            max = double.Max(max, min);
    }

    private void ResetGridUpdateTask()
    {
        // if (!currentUpdateTask.IsCompleted)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
        }
        currentUpdateGridTime.Restart();
        if (diagnostic.RequireMainThread)
        {
            diagnostic.UpdateGridData(this, CancellationToken.None);
            currentUpdateTask = Task.CompletedTask;
        }
        else
            currentUpdateTask = Task.Run(() => diagnostic.UpdateGridData(this, cancellationTokenSource.Token));
    }

    private Task backgroundTask;

    public void SetUpdateTask(Action action)
    {
        var task = Task.Run(action);
    }

    public override void DrawImGuiSettings()
    {
        ImGui.SliderInt("CellCount", ref TargetCellCount, 16, 512 * 512);

        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            Resize();
            UpdateRenderData();
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
                ResetGridUpdateTask();
        }

        diagnostic?.OnImGuiEdit(this);
        base.DrawImGuiSettings();
    }

    private void Resize()
    {
        var dat = GetRequiredWorldService<DataService>();
        var aspect = Vec2.Normalize(dat.VectorField.Domain.RectBoundary.Size.Down());
        double scale = Math.Sqrt(TargetCellCount / (aspect.X * aspect.Y));
        int width = Math.Max(1, (int)Math.Round(aspect.X * scale));
        int height = Math.Max(1, (int)Math.Round(aspect.Y * scale));
        bool interpolate = RegularGrid.Interpolate;
        RegularGrid = new(new Vec2i(width, height), dat.VectorField.Domain.RectBoundary.Min.XY, dat.VectorField.Domain.RectBoundary.Max.XY);
        RegularGrid.Interpolate = interpolate;
        gridbuffer = new(RegularGrid.Grid.Data);
    }

    public string GetTitle()
    {
        if (diagnostic.GetType() == typeof(LICGridDiagnostic))
            return $"LIC {GetRequiredWorldService<DataService>().currentSelectedVectorField}";
        return diagnostic?.Name ?? "";
    }

    public (double min, double max) GetScale()
    {
        if (AutoScale)
            return (min, max);
        return (0, 1);
    }

    public double ScaleScaler(double value)
    {
        if (AutoScale)
            return (value - min) / (max - min);
        return value;
    }

    public void Save(string path, double t_start, double t_end, int timeSteps)
    {
        MarkDirty = true;
        UpdateRenderData();

        var gridSize = new Vec3i(RegularGrid.Grid.GridSize.X / 1, RegularGrid.Grid.GridSize.Y / 1, timeSteps);
        var domain = new Rect<Vec3>(RegularGrid.Domain.RectBoundary.Min.Up(t_start), RegularGrid.Domain.RectBoundary.Max.Up(t_end));
        var spatialDomain = domain.Reduce<Vec2>();
        var field = new RegularGridVectorField<Vec3, Vec3i, double>(gridSize, new RectDomain<Vec3>(domain));

        for (int i_t = 0; i_t < timeSteps - 1; i_t++)
        {
            double t = double.Lerp(t_start, t_end, i_t / (double)(timeSteps - 1));
            GetRequiredWorldService<DataService>().SimulationTime = t;
            MarkDirty = true;
            UpdateRenderData();
            ParallelGrid.For(gridSize.XY, CancellationToken.None, (i_x, i_y) =>
            {
                var pos = spatialDomain.FromRelative(new Vec2(i_x + .5f, i_y + .5f) / gridSize.XY.ToVec2());
                field.AtCoords(new Vec3i(i_x, i_y, i_t)) = RegularGrid.Evaluate(pos).Value;
            });
        }

        field.Save(path);
    }
    public void WaitForComputation()
    {
        while (!currentUpdateTask.IsCompleted)
        {

        }
    }
}