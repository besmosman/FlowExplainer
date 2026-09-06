namespace FlowExplainer;

public class DoubleGyreSpacetimeDensityStructureScene : Scene
{
    public override void Load(FlowExplainer flowExplainer)
    {
        var world = flowExplainer.GetGlobalService<WorldManagerService>().Worlds[0];
        world.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
        world.DataService.currentSelectedScaler = "Temperature";
        world.DataService.currentSelectedVectorField = "Velocity";

        var view3D = world.FlowExplainer.GetGlobalService<ViewsService>().Views[0];
        view3D.Is3DCamera = true;
        view3D.Name = "Spacetime View";
        var ltf = world.AddVisualisationService<LagrangianTransportFormalismService>();

        var dataset = world.DataService.LoadedDataset;
        var u = dataset.GetVectorField<Vec3, Vec2>("Velocity");
        var T = dataset.GetVectorField<Vec3, double>("Total Temperature");
        var T_noflow = dataset.GetVectorField<Vec3, double>("No Flow Temperature");
        var pe = double.Parse(dataset.Properties["Pe"]);
        var Pe = new ArbitraryField<Vec3, double>(IDomain<Vec3>.Infinite, _ => pe);
        ltf.RecomputeNonDimensional(u, T, T_noflow, Pe);

        world.AddVisualisationService<AxisVisualizer>();
        world.AddVisualisationService<Axis3D>();
        world.AddVisualisationService<ParticleSystem>();
        world.AddVisualisationService<DensityParticles3DVisualizer>();
        //world.AddVisualisationService<DensityPathVisualizer>();
        world.AddVisualisationService<Slice3DVisualizer>();
        world.AddVisualisationService<DensityPathStructuresSpaceTime>();
        world.AddVisualisationService<DensityStructuresSpaceTime3DUI>();
        /*var examples = world.AddVisualisationService<DensityPathStructuresExamples>();*/
        /*examples.LoadExample(examples.Entries[0]);*/
        var view2D = world.FlowExplainer.GetGlobalService<ViewsService>().NewView();
        view2D.Name = "Texture View";
    }
}

public class PipeFlowSpacetimeDensityStructureScene : Scene
{
    public override void Load(FlowExplainer flowExplainer)
    {
        var world = flowExplainer.GetGlobalService<WorldManagerService>().Worlds[0];
        world.DataService.SetDataset("PipeFlow Slow");
        world.DataService.currentSelectedScaler = "Temperature";
        world.DataService.currentSelectedVectorField = "Velocity";

        var view3D = world.FlowExplainer.GetGlobalService<ViewsService>().Views[0];
        view3D.Is3DCamera = true;
        view3D.Name = "Spacetime View";
        var ltf = world.AddVisualisationService<LagrangianTransportFormalismService>();
        var dataset = world.DataService.LoadedDataset;
        var u = dataset.GetVectorField<Vec3, Vec2>("Velocity");
        var T = dataset.GetVectorField<Vec3, double>("Temperature");
        var T_noflow = dataset.GetVectorField<Vec3, double>("Temperature_NoFlow");
        var alpha = dataset.GetVectorField<Vec3, double>("Thermal Diffusivity");
        ltf.Recompute(u, T, T_noflow, alpha);

        var axis = world.AddVisualisationService<AxisVisualizer>();
        
        //world.AddVisualisationService<Axis3D>();
        var particleSystem = world.AddVisualisationService<ParticleSystem>();
        world.AddVisualisationService<DensityParticles3DVisualizer>();
        //world.AddVisualisationService<DensityPathVisualizer>();
        world.AddVisualisationService<Slice3DVisualizer>();
        var densityPathStructuresSpaceTime = world.AddVisualisationService<DensityPathStructuresSpaceTime>();
        world.AddVisualisationService<DensityStructuresSpaceTime3DUI>();

        particleSystem.SeedRect.Min.Z = 00;
        particleSystem.SeedRect.Max.Z = 100_00;
        particleSystem.ReseedRate = .01;
        particleSystem.dFictitious = .9;
        densityPathStructuresSpaceTime.SliceValue = 50_00;
        densityPathStructuresSpaceTime.Tau = 50_00;
        densityPathStructuresSpaceTime.AccumelationFactor = .5;
        densityPathStructuresSpaceTime.Decay = .3;
        densityPathStructuresSpaceTime.InfluenceRadius = 0.0005;
        
        particleSystem.Particles.ResizeIfNeeded(1024);
        /*var examples = world.AddVisualisationService<DensityPathStructuresExamples>();*/
        /*examples.LoadExample(examples.Entries[0]);*/
        var view2D = world.FlowExplainer.GetGlobalService<ViewsService>().NewView();
        view2D.Name = "Texture View";
    }
}