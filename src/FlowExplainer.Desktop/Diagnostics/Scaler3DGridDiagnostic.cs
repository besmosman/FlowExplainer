namespace FlowExplainer;

public class Scaler2DGridDiagnostic : IGridDiagnostic
{
    public IVectorField<Vec2, double> ScalerField = IVectorField<Vec2,Double>.Constant(0);

    public void UpdateGridData(GridVisualizer gridVisualizer, CancellationToken token)
    {
        var renderGrid = gridVisualizer.RegularGrid.Grid;
        var dat = gridVisualizer.GetRequiredWorldService<DataService>();
        var scalerfield = ScalerField;
        var spaceBounds = dat.VectorField.Domain.RectBoundary.Reduce<Vec2>();


        Parallel.For(0, renderGrid.GridSize.X * renderGrid.GridSize.Y, c =>
        {
            var i = c % renderGrid.GridSize.X;
            var j = c / renderGrid.GridSize.X;
            var pos = (new Vec2(i, j) / renderGrid.GridSize.ToVec2()) * spaceBounds.Size + spaceBounds.Min;
            renderGrid.AtCoords(new Vec2i(i, j)).Value = scalerfield.Evaluate(pos);
        });
    }
    public string Name(GridVisualizer gridVisualizer)
    {
        //gridVisualizer.DataService.ScalerField.DisplayName
        return "Scaler 2D";
    }
    public void OnImGuiEdit(GridVisualizer gridVisualizer)
    {
        ImGuiHelpers.OptonalVectorFieldSelector(gridVisualizer.World, ref ScalerField);
    }
}

public class Scaler3DGridDiagnostic : IGridDiagnostic
{
    public IVectorField<Vec3, double>? AltScalerField;

    public void UpdateGridData(GridVisualizer gridVisualizer, CancellationToken token)
    {
        var renderGrid = gridVisualizer.RegularGrid.Grid;
        var dat = gridVisualizer.GetRequiredWorldService<DataService>();
        var tempratureField = AltScalerField ?? dat.ScalerField;
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
        //gridVisualizer.DataService.ScalerField.DisplayName
        return "Scaler 3D";
    }
    public void OnImGuiEdit(GridVisualizer gridVisualizer)
    {
        ImGuiHelpers.OptonalVectorFieldSelector(gridVisualizer.World, ref AltScalerField);
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