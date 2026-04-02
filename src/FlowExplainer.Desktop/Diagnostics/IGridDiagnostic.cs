namespace FlowExplainer;

public interface IGridDiagnostic
{
    void UpdateGridData(GridVisualizer gridVisualizer, CancellationToken token);
    void OnImGuiEdit(GridVisualizer gridVisualizer);
    public string Name(GridVisualizer gridVisualizer) => this.GetType().Name.Replace("GridDiagnostic", "");
    public bool UseCustomColoring => false;
    public bool RequireMainThread => false;
}