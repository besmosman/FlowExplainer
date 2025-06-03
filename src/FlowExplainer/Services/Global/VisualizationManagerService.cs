namespace FlowExplainer
{
    public class VisualisationManagerService : GlobalService
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
            v.AddVisualisationService(new DataService());
            v.AddVisualisationService(new SphSimulationService());
            v.AddVisualisationService(new FlowFieldVisualizer());
            Worlds.Add(v);
            return v;
        }

        public override void Initialize()
        {
        }
    }
}