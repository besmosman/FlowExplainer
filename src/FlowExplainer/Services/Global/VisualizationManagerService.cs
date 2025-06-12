namespace FlowExplainer
{
    public class WorldManagerService : GlobalService
    {
        public List<World> Worlds = new();

        public override void Draw()
        {
            /*            foreach (var v in Visualisation)
                            v.Draw();*/
        }

        public World NewWorld(bool skipInit = false)
        {
            if (FlowExplainer == null)
                throw new Exception();

            World v = new(FlowExplainer);
            v.AddVisualisationService(new ViewController2D());
            v.AddVisualisationService(new DataService());
            v.AddVisualisationService(new HeatSimulationViewData());
            v.AddVisualisationService(new HeatSimulationVisualizer());
            v.AddVisualisationService(new HeatSimulationService());
            v.AddVisualisationService(new HeatSimulationReplayer());
            v.AddVisualisationService(new FlowFieldVisualizer());
            v.AddVisualisationService(new PoincareVisualizer());
            Worlds.Add(v);
            return v;
        }

        public override void Initialize()
        {
        }
    }
}