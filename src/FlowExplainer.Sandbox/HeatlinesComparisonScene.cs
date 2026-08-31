namespace FlowExplainer;

public class HeatlinesComparisonScene : Scene
{
    public override void Load(FlowExplainer flowExplainer)
    {
        var world = flowExplainer.GetGlobalService<WorldManagerService>().Worlds[0];
        world.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
        world.DataService.currentSelectedScaler = "Convective Temperature";

        world.AddVisualisationService(new GridVisualizer());
        world.AddVisualisationService(new HeatlinesService());
        world.DataService.SimulationTime = 3f;
    }
}