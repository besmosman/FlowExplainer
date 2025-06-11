namespace FlowExplainer
{
    /*
    public struct FragmentInput
    {
        public Vec2 Uv;
        public Vec3 Normal;
        public Vec4 VertexColor;
    }

    public class NShader
    {
        public Vec4 Main(FragmentInput input)
        {
            return new Vec4(input.Normal, 1);
        }
    }
    */
    
    

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