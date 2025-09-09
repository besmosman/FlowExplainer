namespace FlowExplainer
{

    public class WorldManagerService : GlobalService
    {
        public List<World> Worlds = new();

        public override void Draw()
        {
            foreach (var world in Worlds)
                world.Update();
            
            /*            foreach (var v in Visualisation)
                            v.Draw();*/
        }

        public World NewWorld(bool skipInit = false)
        {
            if (FlowExplainer == null)
                throw new Exception();

            World v = new(FlowExplainer);
            v.AddVisualisationService(new DataService());
            v.AddVisualisationService(new HeatSimulationViewData());
            v.AddVisualisationService(new HeatSimulationVisualizer());
            v.AddVisualisationService(new GridVisualizer()
            {
                IsEnabled = false
            });
            v.AddVisualisationService(new FlowDirectionVisualization()
            {
                IsEnabled = true
            });
            v.AddVisualisationService(new HeatSimulation3DVisualizer());
            v.AddVisualisationService(new HeatSimulationService()
            {
                IsEnabled = false
            });
            v.AddVisualisationService(new HeatSimulationReplayer());
            v.AddVisualisationService(new FlowFieldVisualizer()
            {
                IsEnabled = false
            });
            v.AddVisualisationService(new PoincareVisualizer()
            {
                IsEnabled = false
            });
            v.AddVisualisationService(new AxisVisualizer());
            v.AddVisualisationService(new StructureIdentifier()
            {
                IsEnabled = false
            });
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