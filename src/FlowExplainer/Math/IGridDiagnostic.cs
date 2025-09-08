namespace FlowExplainer;

public interface IGridDiagnostic
{
    void UpdateGridData(GridVisualizer gridVisualizer);
    void OnImGuiEdit(GridVisualizer gridVisualizer);
    public string Name => this.GetType().Name.Replace("GridDiagnostic", "");
    public bool UseCustomColoring => false;
}