namespace FlowExplainer;

public class ScalerGridDiagnostic : IGridDiagnostic
{
    public void UpdateGridData(GridVisualizer gridVisualizer, CancellationToken token)
    {
        var renderGrid = gridVisualizer.RegularGrid.Grid;
        var dat = gridVisualizer.GetRequiredWorldService<DataService>();
        var tempratureField = dat.ScalerField;
        var spaceBounds = dat.VectorField.Domain.RectBoundary.Reduce<Vec2>();


        Parallel.For(0, renderGrid.GridSize.X * renderGrid.GridSize.Y, c =>
        {
            var i = c % renderGrid.GridSize.X;
            var j = c / renderGrid.GridSize.X;
            var pos = (new Vec2(i, j) / renderGrid.GridSize.ToVec2()) * spaceBounds.Size + spaceBounds.Min;
            renderGrid.AtCoords(new Vec2i(i, j)).Value = tempratureField.Evaluate(new Vec3(pos, dat.SimulationTime));
        });
    }
    public string Name(GridVisualizer gridVisualizer)
    {
        return gridVisualizer.DataService.ScalerField.DisplayName;
    }
    public void OnImGuiEdit(GridVisualizer gridVisualizer)
    {

    }
}

public class StagnationCompareGridDiagnostic : IGridDiagnostic
{
    public bool UseCustomColoring => true;
    public double e = 0.025;
    
    public void UpdateGridData(GridVisualizer gridVisualizer, CancellationToken token)
    {
        var renderGrid = gridVisualizer.RegularGrid.Grid;
        var dat = gridVisualizer.GetRequiredWorldService<DataService>();
        var temperatureField = dat.ScalerField;
        var vectorField = dat.VectorField;
        var spaceBounds = dat.VectorField.Domain.RectBoundary.Reduce<Vec2>();

        Parallel.For(0, renderGrid.GridSize.X * renderGrid.GridSize.Y, c =>
        {
            var i = c % renderGrid.GridSize.X;
            var j = c / renderGrid.GridSize.X;
            var pos = (new Vec2(i, j) / renderGrid.GridSize.ToVec2()) * spaceBounds.Size + spaceBounds.Min;
            double temp = temperatureField.Evaluate(new Vec3(pos, dat.SimulationTime));
            double fluxMag = vectorField.Evaluate(new Vec3(pos, dat.SimulationTime)).Length();

            renderGrid.AtCoords(new Vec2i(i, j)).Padding.X = temp;
            renderGrid.AtCoords(new Vec2i(i, j)).Padding.Y = fluxMag;
            renderGrid.AtCoords(new Vec2i(i, j)).Color = Color.Black;


            bool tempStag = double.Abs(temp) < e;
            bool fluxStag = double.Abs(fluxMag) < e;


            if (tempStag)
                renderGrid.AtCoords(new Vec2i(i, j)).Color = Color.Red;
            if (fluxStag)
                renderGrid.AtCoords(new Vec2i(i, j)).Color = Color.Green;
            if (tempStag && fluxStag)
                renderGrid.AtCoords(new Vec2i(i, j)).Color = new Color(1, 1, 0);


        });
    }

    public string Name(GridVisualizer gridVisualizer)
    {
        return $"Both @yellow[(Yellow)] \r\n |Q'| < {e:N3} @green[(Green)],\r\n |T'| < {e:N3} @red[(Red)],";
    }

    public void OnImGuiEdit(GridVisualizer gridVisualizer)
    {
        ImGuiHelpers.Slider("threshold", ref e, 0, .1);
    }
}