namespace FlowExplainer
{
    internal class Program
    {
        static void Main(string[] _)
        {
            var neuroTrace = new FlowExplainer();
            neuroTrace.AddGlobalService(new PreferencesService());
            neuroTrace.AddGlobalService(new WindowService());

            var visualisations = new VisualisationManagerService();
            neuroTrace.AddGlobalService(visualisations);
            visualisations.NewWorld();
            neuroTrace.AddGlobalService(new ImGUIService());
            neuroTrace.AddGlobalService(new ViewsService());
            neuroTrace.AddGlobalService(new ImGUIRenderService());
            neuroTrace.Run();
        }
    }
}