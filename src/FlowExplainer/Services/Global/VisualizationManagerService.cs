namespace FlowExplainer
{
    public class VisualisationManagerService : GlobalService
    {
        public List<Visualisation> Visualisations = new();

        public override void Draw()
        {
            /*            foreach (var v in Visualisation)
                            v.Draw();*/
        }

        public Visualisation NewVisualisation(bool skipInit = false)
        {
            if (FlowExplainer == null)
                throw new Exception();

            Visualisation v = new(FlowExplainer);
            v.AddVisualisationService(new Test2Service());
            Visualisations.Add(v);
            return v;
        }

        public override void Initialize()
        {
        }
    }
}