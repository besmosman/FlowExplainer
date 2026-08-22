namespace FlowExplainer;

public class HeatSimScene : Scene
{
    public override void Load(FlowExplainer flowExplainer)
    {
        var world = flowExplainer.GetGlobalService<WorldManagerService>().Worlds[0];
        world.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
        world.DataService.currentSelectedVectorField = "Velocity";
        world.DataService.currentSelectedScaler = "Convective Temperature";
        world.DataService.SimulationTime = 3f;
        world.AddVisualisationService(new OperatorSplittedPullbackTestService() { IsSimulationSource = true });
        
    }
}