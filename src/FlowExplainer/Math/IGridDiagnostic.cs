namespace FlowExplainer;

public interface IGridDiagnostic
{
    Type DataType { get; }
    void UpdateGridData(GridVisualizer gridVisualizer);
    void OnImGuiEdit(GridVisualizer gridVisualizer);
    public string Name => this.GetType().Name.Replace("GridDiagnostic", "");
}