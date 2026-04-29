namespace FlowExplainer;

public class TrajectoryScene : Scene
{
    public override void Load(FlowExplainer flowExplainer)
    {
        var world = flowExplainer.GetGlobalService<WorldManagerService>().Worlds[0];
        world.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
        world.DataService.currentSelectedVectorField = "Total Flux";
        world.DataService.currentSelectedScaler = "Convective Temperature";
        world.DataService.SimulationTime = 3f;

        var view3D = world.FlowExplainer.GetGlobalService<ViewsService>().Views[0];
        view3D.Is3DCamera = true;
        world.AddVisualisationService<AxisVisualizer>();
        world.AddVisualisationService<Axis3D>();
        world.AddVisualisationService<DensityParticlesData>();
        world.AddVisualisationService<DensityParticles3DVisualizer>();
        world.AddVisualisationService<SpacetimePathVisualizer>();
        //world.AddVisualisationService<Slice3DVisualizer>();
        //world.AddVisualisationService<DensityPathStructuresSpaceTime>();
        //world.AddVisualisationService<DensityStructuresSpaceTime3DUI>();
        //var examples = world.AddVisualisationService<DensityPathStructuresExamples>();
        //examples.LoadExample(examples.Entries[0]);
        //var view2D = world.FlowExplainer.GetGlobalService<ViewsService>().NewView();
        //view2D.Name = "Slice View";
    }
}

public class SpacetimeDensityStructureScene : Scene
{
    public override void Load(FlowExplainer flowExplainer)
    {
        var world = flowExplainer.GetGlobalService<WorldManagerService>().Worlds[0];
        world.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
        world.DataService.currentSelectedVectorField = "Total Flux";
        world.DataService.currentSelectedScaler = "Convective Temperature";
        world.DataService.SimulationTime = 3f;

        var view3D = world.FlowExplainer.GetGlobalService<ViewsService>().Views[0];
        view3D.Is3DCamera = true;
        view3D.Name = "Spacetime View";
        world.AddVisualisationService<AxisVisualizer>();
        world.AddVisualisationService<Axis3D>();
        world.AddVisualisationService<DensityParticlesData>();
        world.AddVisualisationService<DensityParticles3DVisualizer>();
        //world.AddVisualisationService<DensityPathVisualizer>();
        world.AddVisualisationService<Slice3DVisualizer>();
        world.AddVisualisationService<DensityPathStructuresSpaceTime>();
        world.AddVisualisationService<DensityStructuresSpaceTime3DUI>();
        var examples = world.AddVisualisationService<DensityPathStructuresExamples>();
        examples.LoadExample(examples.Entries[0]);
        var view2D = world.FlowExplainer.GetGlobalService<ViewsService>().NewView();
        view2D.Name = "Texture View";
    }
}