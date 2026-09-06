using MathNet.Numerics.Data.Matlab;

namespace FlowExplainer;

public class PipeScene : Scene
{
    public override void Load(FlowExplainer flowExplainer)
    {
        var world = flowExplainer.GetGlobalService<WorldManagerService>().Worlds[0];
        world.DataService.SetDataset("PipeFlow Slow+Cool");
        world.DataService.currentSelectedScaler = "Temperature";
        world.DataService.currentSelectedVectorField = "Velocity";
        var ltf = world.AddVisualisationService<LagrangianTransportFormalismService>();
        var dataset = world.DataService.LoadedDataset;
        var u = dataset.GetVectorField<Vec3, Vec2>("Velocity");
        var T = dataset.GetVectorField<Vec3, double>("Temperature");
        var T_noflow = dataset.GetVectorField<Vec3, double>("Temperature_NoFlow");
        var alpha = dataset.GetVectorField<Vec3, double>("Thermal Diffusivity");
        ltf.Recompute(u, T, T_noflow, alpha);
        
        var slicer = world.AddVisualisationService<TimeSliceService>();
        slicer.sliceValue = 20;
        var velSliced = slicer.RegisterSlicedArtifact(world.GetArtifact<IVectorField<Vec3, Vec2>>("dataset/velocity"));
        var fluxSliced = slicer.RegisterSlicedArtifact(world.GetArtifact<IVectorField<Vec3, Vec2>>("ltf/flux/total"));
        var convfluxSliced = slicer.RegisterSlicedArtifact(world.GetArtifact<IVectorField<Vec3, Vec2>>("ltf/flux/conductive"));
        var tempSliced = slicer.RegisterSlicedArtifact(world.GetArtifact<IVectorField<Vec3, double>>("dataset/temperature"));

        var grid = world.AddVisualisationService<GridVisualizer>();
        var scaler = grid.SetGridDiagnostic<MagnitudeGridDiagnostic>();
        var axis = world.AddVisualisationService<AxisVisualizer>();
        axis.DrawTitle = false;
        axis.DrawAxis = false;
        world.DataService.ColorGradient = Gradients.GetGradient("matlab_jet");
        scaler.vectorfield = (Artifact<IVectorField<Vec2, Vec2>>?)convfluxSliced;

        var anim = world.AddVisualisationService<AnimatedGlyphFlowService>();
        anim.VectorField = (Artifact<IVectorField<Vec2, Vec2>>)convfluxSliced;
        anim.opacity = .5;
        anim.speed = .5;
        anim.thickness = 0.001;
    }
}