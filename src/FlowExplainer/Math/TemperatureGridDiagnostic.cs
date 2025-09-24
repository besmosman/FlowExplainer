using ImGuiNET;

namespace FlowExplainer;

public class HeatStructureGridDiagnostic : IGridDiagnostic
{

    public string Name => "Heat Structures";
    public bool Reverse;
    public float T;
    public float M;
    public int K = 1;

    public void UpdateGridData(GridVisualizer gridVisualizer)
    {
        var renderGrid = gridVisualizer.RegularGrid;
        var dat = gridVisualizer.GetRequiredWorldService<DataService>();
        var spaceBounds = dat.VectorField.Domain.Boundary.Reduce<Vec2>();
        float t = dat.SimulationTime;
        var tempratureField = dat.TempratureFieldInstant;
        var datVectorFieldInstant = new ArbitraryField<Vec3, Vec2>(dat.VectorField.Domain, p => dat.VectorField.Evaluate(p) * (M / T));
        if (Reverse)
            datVectorFieldInstant = new ArbitraryField<Vec3, Vec2>(dat.VectorField.Domain, p => -dat.VectorField.Evaluate(p) * (M / T) );

        ParallelGrid.For(renderGrid.GridSize, (i, j) => { renderGrid.AtCoords(new Vec2i(i, j)).Value = 0; });
        ParallelGrid.For(renderGrid.GridSize, (i, j) =>
        {
            //for (int k = 0; k < K; k++)
            {
                var pos = (new Vec2(i, j) / renderGrid.GridSize.ToVec2()) * spaceBounds.Size + spaceBounds.Min;
               // pos = Utils.Random(spaceBounds);
                var trajectory = IFlowOperator<Vec2, Vec3>.Default.Compute(t, t + T, pos, datVectorFieldInstant);
                var endPos = trajectory.Entries.Last().Down();
                for (int index = trajectory.Entries.Length-2; index < trajectory.Entries.Length; index++)
                {
                    var e = trajectory.Entries[index];
                    if (spaceBounds.Contains(e.XY) /*&&  Vec2.Distance(pos, endPos) > .00f */ /*&& trajectory.Entries.Length == 64 */)
                    {
                        renderGrid.AtPos(e.XY).Value = float.Lerp(renderGrid.AtPos(e.XY).Value, 1, .001f * ((float)index / trajectory.Entries.Length));
                        //renderGrid.AtPos(endPos).Value += 1;
                    }
                }
            }
        });

        /*
        ParallelGrid.Run(renderGrid.GridSize, (i, j) =>
        {
            ref var p = ref renderGrid.AtCoords(new Vec2i(i, j));
            if (p.Value > 2)
                p.Value = 1;
            else p.Value = 0;
        });
        */

    }
    public void OnImGuiEdit(GridVisualizer gridVisualizer)
    {
        ImGui.Checkbox("Reverse", ref Reverse);
        ImGuiHelpers.SliderFloat("T", ref T, 0, 1);
        ImGuiHelpers.SliderInt("K", ref K, 1, 100);
        ImGuiHelpers.SliderFloat("M", ref M, 1, 1000);
    }
}

public class TemperatureGridDiagnostic : IGridDiagnostic
{
    public void UpdateGridData(GridVisualizer gridVisualizer)
    {
        var renderGrid = gridVisualizer.RegularGrid.Grid;
        var dat = gridVisualizer.GetRequiredWorldService<DataService>();
        var tempratureField = dat.TempratureField;
        var spaceBounds = dat.VectorField.Domain.Boundary.Reduce<Vec2>();


        Parallel.For(0, renderGrid.GridSize.X * renderGrid.GridSize.Y, c =>
        {
            var i = c % renderGrid.GridSize.X;
            var j = c / renderGrid.GridSize.X;
            var pos = (new Vec2(i, j) / renderGrid.GridSize.ToVec2()) * spaceBounds.Size + spaceBounds.Min;
            renderGrid.AtCoords(new Vec2i(i, j)).Value = tempratureField.Evaluate(new Vec3(pos, dat.SimulationTime));
        });
    }
    public void OnImGuiEdit(GridVisualizer gridVisualizer)
    {

    }
}