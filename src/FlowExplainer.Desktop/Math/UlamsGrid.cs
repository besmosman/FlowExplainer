using ImGuiNET;

namespace FlowExplainer;

public class UlamsGrid : IGridDiagnostic
{
    public PerronFrobeniusOperatorUlamsMethod method = new();
    public void UpdateGridData(GridVisualizer gridVisualizer, CancellationToken token)
    {
        var renderGrid = gridVisualizer.RegularGrid.Grid;
        var dat = gridVisualizer.GetWorldService<DataService>()!;
        var vectorField = dat.VectorField;
        var domain = vectorField.Domain;
        var spatialBounds = domain.RectBoundary.Reduce<Vec2>();

        ParallelGrid.For(renderGrid.GridSize, token, (i, j) =>
        {
            var pos = spatialBounds.FromRelative(new Vec2(i+.5f, j+.5f) / renderGrid.GridSize.ToVec2());
            renderGrid.AtCoords(new Vec2i(i, j)).Value = method.TransitionMatrix.Column(method.WorldToMatrixIndex(pos)).Sum();
        });
    }

    public void OnImGuiEdit(GridVisualizer gridVisualizer)
    {
        if (ImGui.Button("Recompute"))
        {
            Recompute(gridVisualizer);
        }
    }
    public void Recompute(GridVisualizer gridVisualizer)
    {
        var dat = gridVisualizer.GetWorldService<DataService>()!;
        method.Compute(dat.VectorField);
    }
}