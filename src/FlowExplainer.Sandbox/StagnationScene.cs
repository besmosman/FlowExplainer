namespace FlowExplainer;

public class StagnationScene : Scene
{
    public override void Load(FlowExplainer flowExplainer)
    {
        var world = flowExplainer.GetGlobalService<WorldManagerService>().Worlds[0];
        world.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
        world.DataService.currentSelectedVectorField = "Total Flux";
        world.DataService.currentSelectedScaler = "Convective Temperature";
        world.DataService.SimulationTime = 3f;

        var axis = world.AddVisualisationService<AxisVisualizer>();
        axis.DrawGradient = false;

        var grid = world.AddVisualisationService<GridVisualizer>();
        grid.SetGridDiagnostic(new StagnationCompareGridDiagnostic());
        grid.TargetCellCount = 52_000;
        grid.Bilinear = false;
    }
}