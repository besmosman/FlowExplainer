using MathNet.Numerics.Data.Matlab;

namespace FlowExplainer;

public class PipeScene : Scene
{
    public override void Load(FlowExplainer flowExplainer)
    {
        var world = flowExplainer.GetGlobalService<WorldManagerService>().Worlds[0];
        world.DataService.SetDataset("PipeSegment case 1");
        var grid = world.AddVisualisationService<GridVisualizer>();
        var scaler = grid.SetGridDiagnostic<Scaler3DGridDiagnostic>();
        scaler.scalerField = world.DataService.Artifacts.Get<IVectorField<Vec3, double>>("T");
    }
}