namespace FlowExplainer
{

    public class WorldManagerService : GlobalService
    {
        public List<World> Worlds = new();

        public override void Draw()
        {
            foreach (var world in Worlds)
            {
                if (world.IsViewed)
                    world.Update();

                world.IsViewed = false;
            }
            /*            foreach (var v in Visualisation)
                            v.Draw();*/
        }

        public World NewWorld(bool skipInit = false)
        {
            if (FlowExplainer == null)
                throw new Exception();

            World v = new(FlowExplainer);
            v.AddVisualisationService(new DataService()
            {
                IsEnabled = true,
            });
            v.AddVisualisationService(new Axis3D()
            {
                IsEnabled = true,
            });
            //v.AddVisualisationService(new HeatSimTest(){ IsEnabled = true});
            v.AddVisualisationService(new HeatSimulationViewData());
            v.AddVisualisationService(new HeatSimulationVisualizer());
            v.AddVisualisationService(new GridVisualizer());
            v.AddVisualisationService(new Poincare3DVisualizer());
            v.AddVisualisationService(new ParticleLagrangianTest());
            v.AddVisualisationService(new FlowDirectionVisualization());
            v.AddVisualisationService(new HeatSimulation3DVisualizer());
            v.AddVisualisationService(new HeatSimulationService());
            v.AddVisualisationService(new StochasticPoincare());
            v.AddVisualisationService(new CriticalPointIdentifier());
            v.AddVisualisationService(new HeatSimulationReplayer());
            v.AddVisualisationService(new FlowFieldVisualizer());
            v.AddVisualisationService(new PoincareVisualizer());
            v.AddVisualisationService(new AxisVisualizer()
            {
                IsEnabled = true,
            });
            v.AddVisualisationService(new StructureIdentifier());
            //v.AddVisualisationService(new FDTest(){ IsEnabled = true});
            v.AddVisualisationService(new FlowVisService());
            //v.AddVisualisationService(new FDTest());
            //v.AddVisualisationService(new Heat3DViewer());
            Worlds.Add(v);
            return v;
        }

        public override void Initialize()
        {
        }
    }
}