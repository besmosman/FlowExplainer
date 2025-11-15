using System.Runtime.InteropServices;
using ImGuiNET;


namespace FlowExplainer;

public class LcsVelocityMagnitudeGridDiagnostic : IGridDiagnostic
{
    public double T = 1;

    public void UpdateGridData(GridVisualizer gridVisualizer, CancellationToken token)
    {
        var renderGrid = gridVisualizer.RegularGrid.Grid;

        var dat = gridVisualizer.GetRequiredWorldService<DataService>()!;
        var domain = dat.VectorField.Domain;

        var t = dat.SimulationTime;
        var tau = dat.SimulationTime + T;

        Parallel.For(0, renderGrid.GridSize.X * renderGrid.GridSize.Y, c =>
        {
            var i = c % renderGrid.GridSize.X;
            var j = c / renderGrid.GridSize.X;
            renderGrid.AtCoords(new Vec2i(i, j)).Value = 0;
            var pos = domain.RectBoundary.Reduce<Vec2>().Relative(new Vec2(i, j) / renderGrid.GridSize.ToVec2());
            var center = IFlowOperator<Vec2, Vec3>.Default.Compute(t, tau, pos, dat.VectorField);
            renderGrid.AtCoords(new Vec2i(i, j)).Value = center.AverageAlong((prev, cur) => ((prev.XY - cur.XY) / (cur.Z - prev.Z)).Length());
        });
    }

    public void OnImGuiEdit(GridVisualizer vis)
    {
        var dat = vis.GetRequiredWorldService<DataService>()!;
        double period = dat.VectorField.Domain.RectBoundary.Size.Last;
        ImGuiHelpers.SliderFloat("T", ref T, -period * 1, period * 1);
    }
}