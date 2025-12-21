using System.Globalization;
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
            /*CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            if (File.Exists("config.json"))
                Config.Load("config.json");
            else
                Config.Load(new Dictionary<string, JValue>());

            ServicesInfo.Init();

            var app = new FlowExplainer();
            app.AddDefaultGlobalServices();
            Scripting.Startup(app.GetGlobalService<WorldManagerService>().Worlds[0]);
            app.Run();*/
        }
    }
}