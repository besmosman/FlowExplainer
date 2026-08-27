using MathNet.Numerics.Data.Matlab;

namespace FlowExplainer;

public class PipeScene : Scene
{
    public override void Load(FlowExplainer flowExplainer)
    {
        var world = flowExplainer.GetGlobalService<WorldManagerService>().Worlds[0];
        world.DataService.SetDataset("PipeFlowCase1");
        world.DataService.currentSelectedScaler = "Temperature";
        world.DataService.currentSelectedVectorField = "Velocity";
    
        var slicer = world.AddVisualisationService<TimeSliceService>();
        var velSliced = slicer.RegisterSlicedArtifact(world.GetArtifact<IVectorField<Vec3, Vec2>>("Velocity"));
        var tempSliced =slicer.RegisterSlicedArtifact(world.GetArtifact<IVectorField<Vec3, double>>("Temperature"));
        
        var grid = world.AddVisualisationService<GridVisualizer>();
        var scaler = grid.SetGridDiagnostic<Scaler2DGridDiagnostic>();
        var axis = world.AddVisualisationService<AxisVisualizer>();
        axis.DrawTitle = false;
        axis.DrawAxis = false;
        world.DataService.ColorGradient = Gradients.GetGradient("matlab_jet");
        scaler.ScalerField = (Artifact<IVectorField<Vec2, double>>?)tempSliced;
    }
}