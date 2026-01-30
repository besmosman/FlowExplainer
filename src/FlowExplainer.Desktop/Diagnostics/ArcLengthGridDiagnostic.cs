namespace FlowExplainer;

public class ArcLengthGridDiagnostic : IGridDiagnostic
{
    public string Name => "Trajectory Arc Length";
    public double T = 1;
    public void UpdateGridData(GridVisualizer gridVisualizer, CancellationToken token)
    {
        var rk4 = IIntegrator<Vec3, Vec2>.Rk4;
        int steps = 64;
        var dat = gridVisualizer.GetRequiredWorldService<DataService>()!;
        var vectorfield = dat.VectorField;
        var time = dat.SimulationTime;
        var renderGrid = gridVisualizer.RegularGrid;
        var domain = vectorfield.Domain.RectBoundary.Reduce<Vec2>();
        var dt = T / steps;
        ParallelGrid.For(renderGrid.GridSize, token, (i, j) =>
        {
            var x = domain.FromRelative(new Vec2(i + .5, j + .5) / renderGrid.GridSize.ToVec2());
            var length = 0.0;
            for (int step = 0; step < steps; step++)
            {
                var x_j = rk4.Integrate(vectorfield, x.Up(time + dt * step), dt);
                x_j = vectorfield.Domain.Bounding.Bound(x_j);
                length += vectorfield.Domain.Bounding.ShortestSpatialDistance(x_j, x.Up(x_j.Z));
            }
            renderGrid.AtCoords(new Vec2i(i, j)).Value = length;
        });
    }

    public void OnImGuiEdit(GridVisualizer gridVisualizer)
    {
        ImGuiHelpers.Slider("T", ref T, 0, 1);
    }
}