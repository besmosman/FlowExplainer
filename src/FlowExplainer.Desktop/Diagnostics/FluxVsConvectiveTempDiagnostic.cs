namespace FlowExplainer;

public class FluxVsConvectiveTempDiagnostic : IGridDiagnostic
{

    public bool UseCustomColoring => true;
    public void UpdateGridData(GridVisualizer gridVisualizer, CancellationToken token)
    {
        var convectiveTemp = gridVisualizer.DataService.Artifacts.Get<IVectorField<Vec3, double>>("Convective Temperature").Value;
        var totalFlux = gridVisualizer.DataService.Artifacts.Get<IVectorField<Vec3, Vec2>>("Total Flux").Value;

        var grid = gridVisualizer.RegularGrid;
        var rect = grid.RectDomain.RectBoundary;
        var t = gridVisualizer.DataService.SimulationTime;
        ParallelGrid.For(grid.GridSize, token, (i, j) =>
        {
            var pos = rect.FromRelative(new Vec2(i, j) / grid.GridSize.ToVec2());
            grid.AtCoords(new Vec2i(i, j)).Padding.X = convectiveTemp.Evaluate(pos.Up(t));
            grid.AtCoords(new Vec2i(i, j)).Padding.Y = totalFlux.Evaluate(pos.Up(t)).Length();
        });

        ParallelGrid.For(grid.GridSize, token, (i, j) =>
        {
            var pos = rect.FromRelative(new Vec2(i, j) / grid.GridSize.ToVec2());
            ref var at = ref grid.Grid.AtCoordsClamped(new Vec2i(i, j));
            var left = grid.Grid.AtCoordsClamped(new Vec2i(i - 1, j));
            var right = grid.Grid.AtCoordsClamped(new Vec2i(i + 1, j));
            var up = grid.Grid.AtCoordsClamped(new Vec2i(i, j + 1));
            var down = grid.Grid.AtCoordsClamped(new Vec2i(i, j - 1));


            bool CrossesZeroTemp = double.Abs(double.Sign(right.Padding.X) + double.Sign(left.Padding.X) + double.Sign(up.Padding.X) + double.Sign(down.Padding.X)) < 4;

            var minFlux = double.Min(left.Padding.Y, double.Min(right.Padding.Y, double.Min(up.Padding.Y, down.Padding.Y)));
            bool MinimaFlux = at.Padding.Y <= minFlux + 0.4 /  grid.GridSize.X;
            if (CrossesZeroTemp)
                at.Color = Color.Green;
            else
                at.Color = Color.Black;

            if (MinimaFlux)
                at.Color = Color.Red;

            if (double.Abs(at.Padding.X) < .01 && at.Padding.Y > .01f)
            {
                at.Color = Color.Blue;

            }
        });

    }

    public void OnImGuiEdit(GridVisualizer gridVisualizer)
    {

    }
}