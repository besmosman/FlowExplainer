using System.Runtime.InteropServices;
using ImGuiNET;


namespace FlowExplainer;

public class MagnitudeGridDiagnostic : IGridDiagnostic
{
    public double T = 1;

    public void UpdateGridData(GridVisualizer gridVisualizer, CancellationToken token)
    {
        var renderGrid = gridVisualizer.RegularGrid.Grid;

        var dat = gridVisualizer.GetRequiredWorldService<DataService>()!;
        var domain = dat.VectorField.Domain;

        var t = dat.SimulationTime;
        var tau = dat.SimulationTime + T;

        var rectBound = domain.RectBoundary.Reduce<Vec2>();
        Parallel.For(0, renderGrid.GridSize.X * renderGrid.GridSize.Y, c =>
        {
            var i = c % renderGrid.GridSize.X;
            var j = c / renderGrid.GridSize.X;
            renderGrid.AtCoords(new Vec2i(i, j)).Value = 0;
            var pos = rectBound.FromRelative(new Vec2(i, j) / renderGrid.GridSize.ToVec2());
            renderGrid.AtCoords(new Vec2i(i, j)).Value = dat.VectorField.Evaluate(pos.Up(t)).Length();
        });
    }

    public void OnImGuiEdit(GridVisualizer vis)
    {
    
    }
}

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
        var reduce = domain.RectBoundary.Reduce<Vec2>();
        var flowOperator = IFlowOperator<Vec2, Vec3>.Default;

        Parallel.For(0, renderGrid.GridSize.X * renderGrid.GridSize.Y, c =>
        {
            var i = c % renderGrid.GridSize.X;
            var j = c / renderGrid.GridSize.X;
            renderGrid.AtCoords(new Vec2i(i, j)).Value = 0;
            var pos = reduce.FromRelative(new Vec2(i, j) / renderGrid.GridSize.ToVec2());
            var center = flowOperator.ComputeTrajectory(t, tau, pos, dat.VectorField);
            renderGrid.AtCoords(new Vec2i(i, j)).Value = center.AverageAlong((prev, cur) => ((prev.XY - cur.XY) / (cur.Z - prev.Z)).Length());
        });
    }

    public void OnImGuiEdit(GridVisualizer vis)
    {
        var dat = vis.GetRequiredWorldService<DataService>()!;
        double period = dat.VectorField.Domain.RectBoundary.Size.Last;
        ImGuiHelpers.Slider("T", ref T, -period * 1, period * 1);
    }
}