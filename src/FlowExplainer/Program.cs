using Newtonsoft.Json.Linq;

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

    public class AssetWatcherService : GlobalService
    {
        public override void Initialize()
        {
        }

        public override void Draw()
        {
            AssetWatcher.Execute();
        }
    }

    internal class Program
    {
        static void Main(string[] _)
        {
            if (File.Exists("config.json"))
                Config.Load("config.json");
            else
                Config.Load(new Dictionary<string, JValue>());


            var neuroTrace = new FlowExplainer();
            neuroTrace.AddGlobalService(new AssetWatcherService());
            neuroTrace.AddGlobalService(new PreferencesService());
            neuroTrace.AddGlobalService(new WindowService());

            var visualisations = new WorldManagerService();
            neuroTrace.AddGlobalService(visualisations);
            visualisations.NewWorld();
            neuroTrace.AddGlobalService(new ImGUIService());
            neuroTrace.AddGlobalService(new ViewsService());
            neuroTrace.AddGlobalService(new ImGUIRenderService());
            neuroTrace.AddGlobalService(new PresentationService());
            neuroTrace.Run();
        }
    }
}