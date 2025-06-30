using ImGuiNET;

namespace FlowExplainer;

public class GridVisualizer : WorldService, IAxisTitle
{
    private IGridDiagnostic? diagnostic;
    private InterpolatedRenderGrid gridData;
    public bool Continous = true;
    public int TargetCellCount = 4000;

    public List<IGridDiagnostic> Diagnostics = [new FTLEGridDiagnostic(), new VelocityMagnitudeGridDiagnostic(), new CustomGridDiagnostic(), new FTLEvsCustomGridDiagnostic()];

    public InterpolatedRenderGrid<T> GetRenderGrid<T>() where T : struct
    {
        return (InterpolatedRenderGrid<T>)gridData;
    }

    public override void Initialize()
    {
        SetGridDiagnostic(new FTLEGridDiagnostic());
    }

    public void SetGridDiagnostic(IGridDiagnostic visualizer)
    {
        diagnostic = visualizer;
        var type = typeof(InterpolatedRenderGrid<>).MakeGenericType(diagnostic.DataType);
        gridData = (InterpolatedRenderGrid)Activator.CreateInstance(type, new Vec2i(1, 1))!;
        Resize();
    }

    public void Resize()
    {
        var dat = GetRequiredWorldService<DataService>();
        var aspect = Vec2.Normalize(dat.VelocityField.Domain.Size);

        float scale = MathF.Sqrt(TargetCellCount / (aspect.X * aspect.Y));
        int width = Math.Max(1, (int)Math.Round(aspect.X * scale));
        int height = Math.Max(1, (int)Math.Round(aspect.Y * scale));

        gridData.Resize(new Vec2i(width, height));
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        if (!view.Is2DCamera)
            return;

        if (diagnostic != null)
        {
            var dat = GetRequiredWorldService<DataService>();
            gridData.UploadColorGradient(dat.ColorGradient);

            if (Continous)
                UpdateData();

            gridData.Draw(view.Camera2D, dat.VelocityField.Domain.Min, dat.VelocityField.Domain.Size);
        }
    }

    private void UpdateData()
    {
        if (diagnostic == null)
            throw new Exception();

        diagnostic.UpdateGridData(this);

     
        
        gridData.UploadData();
        
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
        var bil = gridData.BilinearInterpolation;
        ImGui.Checkbox("Bilinear", ref bil);
        gridData.BilinearInterpolation = bil;
        if (!Continous)
        {
            if (ImGui.Button("Recompute"))
                UpdateData();
        }


        diagnostic?.OnImGuiEdit(this);
        base.DrawImGuiEdit();
    }

    public string GetTitle()
    {
        return "LCS: " + diagnostic?.Name ?? "";
    }
}