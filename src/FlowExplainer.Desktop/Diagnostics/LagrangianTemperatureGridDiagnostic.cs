namespace FlowExplainer;

public class LagrangianTemperatureGridDiagnostic : IGridDiagnostic
{
    public float T = 1;

    public void UpdateGridData(GridVisualizer gridVisualizer)
    {
        var renderGrid = gridVisualizer.RegularGrid.Grid;
        var dat = gridVisualizer.GetRequiredWorldService<DataService>();
        var tempratureField = dat.TempratureField;
        var spaceBounds = dat.VectorField.Domain.RectBoundary.Reduce<Vec2>();
        var t = dat.SimulationTime;
        var tau = dat.SimulationTime + T;
        Parallel.For(0, renderGrid.GridSize.X * renderGrid.GridSize.Y, c =>
        {
            var i = c % renderGrid.GridSize.X;
            var j = c / renderGrid.GridSize.X;
            var pos = (new Vec2(i, j) / renderGrid.GridSize.ToVec2()) * spaceBounds.Size + spaceBounds.Min;
            var center = IFlowOperator<Vec2, Vec3>.Default.Compute(t, tau, pos, dat.VectorField);
            //change in temprature compared to neighbros doe..
            var first = center.Entries.First();
            var last = center.Entries.Last();
            renderGrid.AtCoords(new Vec2i(i, j)).Value = (tempratureField.Evaluate(last) - tempratureField.Evaluate(first)) / (last - first).Z;
        });
        var average = renderGrid.Data.Average(d => d.Value);
        for (int i = 0; i < renderGrid.Data.Length; i++)
        {
            renderGrid.Data[i].Value -= average;
            renderGrid.Data[i].Value *= 10;

        }
    }
    public void OnImGuiEdit(GridVisualizer gridVisualizer)
    {
        var dat = gridVisualizer.GetRequiredWorldService<DataService>()!;
        float period = dat.VectorField.Domain.RectBoundary.Size.Last;
        ImGuiHelpers.SliderFloat("T", ref T, -period, period);
    }

}