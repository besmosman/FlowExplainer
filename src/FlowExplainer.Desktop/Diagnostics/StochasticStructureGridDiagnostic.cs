namespace FlowExplainer;

public class StochasticStructureGridDiagnostic : IGridDiagnostic
{
    public void UpdateGridData(GridVisualizer gridVisualizer, CancellationToken token)
    {
        var rendergrid = gridVisualizer.RegularGrid;
        var field = gridVisualizer.GetRequiredWorldService<DataService>().VectorField;
        var domainRect = field.Domain.RectBoundary;
        var positionBounds = domainRect.Reduce<Vec2>();

        var curT = 0;
        var dt = .05;
        ParallelGrid.For(rendergrid.GridSize, token, (i, j) =>
        {
            var pos = positionBounds.FromRelative(new Vec2(i, j) / rendergrid.GridSize.ToVec2());
            var t0 = curT - Utils.Random(1, 10);
            var t1 = curT;

            var phase = pos.Up(t0);
            while (phase.Last < t1)
            {
                phase = IIntegrator<Vec3, Vec2>.Rk4.Integrate(field, phase, dt);
            }
        });
    }

    public void OnImGuiEdit(GridVisualizer gridVisualizer)
    {

    }
}