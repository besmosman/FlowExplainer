using MathNet.Numerics.Data.Matlab;

namespace FlowExplainer;

public class PipeScene : Scene
{
    public override void Load(FlowExplainer flowExplainer)
    {
        var world = flowExplainer.GetGlobalService<WorldManagerService>().Worlds[0];
        world.DataService.SetDataset("PipeSegment case 1");
        world.DataService.currentSelectedScaler = "T";
        world.DataService.currentSelectedVectorField = "u";
        var grid = world.AddVisualisationService<GridVisualizer>();
        var scaler = grid.SetGridDiagnostic<Scaler3DGridDiagnostic>();
        
        var axis = world.AddVisualisationService<AxisVisualizer>();
        axis.DrawTitle = false;
        axis.DrawAxis = false;
        world.DataService.ColorGradient = Gradients.GetGradient("matlab_jet");
        scaler.scalerField = world.DataService.Artifacts.Get<IVectorField<Vec3, double>>("T");
    }
}