using ImGuiNET;

namespace FlowExplainer;

public class DensityEstimation : IGridDiagnostic
{

    public int ParticleCount = 10000;
    public double duration;
    RegularGridVectorField<Vec3, Vec3i, double> Density;
    public double RenderZ;

    public void UpdateGridData(GridVisualizer gridVisualizer, CancellationToken token)
    {
        if (Density == null)
        {
            Recompute(gridVisualizer);
        }
        var ConvectiveTemp = gridVisualizer.DataService.LoadedDataset.ScalerFields["Convective Temperature"];

        var renderGrid = gridVisualizer.RegularGrid.Grid;
        var domain = ConvectiveTemp.Domain;
        var spatialBounds = domain.RectBoundary.Reduce<Vec2>();
        
        ParallelGrid.For(renderGrid.GridSize, token, (i, j) =>
        {
            ref var center = ref renderGrid.AtCoords(new Vec2i(i, j));
            var pos = spatialBounds.FromRelative(new Vec2(i, j) / (renderGrid.GridSize.ToVec2() - Vec2.One));
            renderGrid.AtCoords(new Vec2i(i, j)).Value = Density.Evaluate(pos.Up(RenderZ));
        });

    }
    public void OnImGuiEdit(GridVisualizer gridVisualizer)
    {
        ImGuiHelpers.Slider("Duration", ref duration, 0, 4);
        ImGuiHelpers.Slider("ParticleCount", ref ParticleCount, 0, 100_000);
        
        if (ImGui.Button("Recompute Densities"))
        {
            Recompute(gridVisualizer);
        }
        var ConvectiveTemp = gridVisualizer.DataService.LoadedDataset.ScalerFields["Convective Temperature"];
        ImGuiHelpers.Slider("Render Z", ref RenderZ, 0, ConvectiveTemp.Domain.RectBoundary.Max.Z);
    }
    private void Recompute(GridVisualizer gridVisualizer)
    {

        var ConvectiveTemp = gridVisualizer.DataService.LoadedDataset.ScalerFields["Convective Temperature"];
        var vec = gridVisualizer.DataService.VectorField;
        var transportField = new ArbitraryField<Vec3, Vec3>(vec.Domain, x => vec.Evaluate(x).Up(double.Abs(ConvectiveTemp.Evaluate(x))));

        Density = new RegularGridVectorField<Vec3, Vec3i, double>(new Vec3i(32, 16, 32)*4, ConvectiveTemp.Domain.RectBoundary.Min, ConvectiveTemp.Domain.RectBoundary.Max);
        var flowOp = IFlowOperatorSteady<Vec3>.Default;
        var domainRectBoundary = Density.Domain.RectBoundary;
        var domainBounding = ConvectiveTemp.Domain.Bounding;
        Parallel.For(0, ParticleCount, (i) =>
        {
            var pos = Utils.Random(domainRectBoundary);
            var end = flowOp.ComputeEnd(pos, duration, transportField);
            if (end.Z > 0 && end.Z < domainRectBoundary.Max.Z)
            {

                Density.AtPos(domainBounding.Bound(end))++;
            }
        });
    }
}