namespace FlowExplainer;

public class DivergenceAlongTrajectoryGridDiagnostic : IGridDiagnostic
{
    public double T;

    public void UpdateGridData(GridVisualizer gridVisualizer, CancellationToken token)
    {
        var renderGrid = gridVisualizer.RegularGrid.Grid;
        var dat = gridVisualizer.GetWorldService<DataService>()!;
        var vectorField = dat.VectorField;
        var domain = vectorField.Domain;
        var spatialBounds = domain.RectBoundary.Reduce<Vec2>();
        var flowOperator = IFlowOperator<Vec2, Vec3>.Default;

        var t = dat.SimulationTime;
        var tau = dat.SimulationTime + T;

        ParallelGrid.For(gridVisualizer.RegularGrid.GridSize, token, (i, j) =>
        {
            var pos = spatialBounds.FromRelative(new Vec2(i, j) / renderGrid.GridSize.ToVec2());
            var averageDivergence = flowOperator.ComputeTrajectory(t, tau, pos, vectorField).AverageAlong((p, l) =>
            {
                var t = p.Last;
                var delta = 0.001;
                var left = vectorField.Evaluate((p.XY + new Vec2(delta, 0)).Up(t));
                var right = vectorField.Evaluate((p.XY + new Vec2(-delta, 0)).Up(t));
                var up = vectorField.Evaluate((p.XY + new Vec2(0, delta)).Up(t));
                var down = vectorField.Evaluate((p.XY + new Vec2(0, -delta)).Up(t));
                return FD.Divergence(left, right, down, up, new Vec2(delta,delta));
            });
            gridVisualizer.RegularGrid.AtCoords(new Vec2i(i, j)).Value = averageDivergence;
        });
    }

    public void OnImGuiEdit(GridVisualizer vis)
    {
        var dat = vis.GetRequiredWorldService<DataService>()!;
        double period = dat.VectorField.Domain.RectBoundary.Size.Last;
        if (ImGuiHelpers.Slider("T", ref T, -period * 1, period * 1))
            vis.MarkDirty = true;
    }
}