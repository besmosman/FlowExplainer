using System.Diagnostics;
using System.Reflection;

namespace FlowExplainer
{
    public class FlowExplainer
    {
        private readonly List<GlobalService> Services = new();
        private bool shouldRun = true;


        public TimeSpan Time { get; set; }
        public double DeltaTime { get; private set; }

        public void AddGlobalService(GlobalService service)
        {
            Services.Add(service);
            service.FlowExplainer = this;
            service.Initialize();
        }

        public void AddDefaultGlobalServices()
        {
            ServicesInfo.Init();

            AddGlobalService(new AssetWatcherService());
            AddGlobalService(new PreferencesService());
            AddGlobalService(new WindowService());
            AddGlobalService(new NewImGUIRenderService());
            AddGlobalService(new ImGUIService());
            AddGlobalService(new DatasetsService());

            var visualisations = new WorldManagerService();
            AddGlobalService(visualisations);
            var mainworld = visualisations.NewWorld();
            AddGlobalService(new ViewsService());
            AddGlobalService(new PresentationService());
        }

        public T GetGlobalService<T>() where T : GlobalService
        {
            foreach (var s in Services)
            {
                if (s is T t)
                {
                    return t;
                }
            }

            throw new Exception();
        }

        public GlobalService? GetGlobalService(Type t)
        {
            foreach (var s in Services)
            {
                if (s.GetType() == t)
                {
                    return s;
                }
            }

            return null;
        }


        public void Exit()
        {
            shouldRun = false;
        }

        public void Run()
        {
            //try
            {
                var total = Stopwatch.StartNew();
                var w = Stopwatch.StartNew();
                while (shouldRun)
                {
                    Profiler.Begin("Frame");
                    double startTime = total.Elapsed.TotalSeconds;
                    DeltaTime = w.Elapsed.TotalSeconds;
                    DeltaTime = double.Min(DeltaTime, 1 / 10f);//limits visual artifacts when moving/freezing the window.
                    Time += w.Elapsed;
                    w.Restart();


                    Profiler.Begin("Draw");
                    foreach (var service in Services)
                    {
                        service.Draw();
                    }

                    Profiler.End("Draw");

                    Profiler.Begin("AfterDraw");
                    foreach (var service in Services)
                        service.AfterDraw();
                    Profiler.End("AfterDraw");
                    double endTime = total.Elapsed.TotalSeconds;
                    Profiler.End("Frame");
                    /*while (endTime - startTime < 1 / 144f)
                    {
                        endTime = total.Elapsed.TotalSeconds;
                    }*/
                }

                foreach (var item in Services)
                    if (item is IDisposable disp)
                        disp.Dispose();
            }
            //catch (Exception e)
            //{
            //    File.WriteAllText("error.txt", e.ToString());
            //    throw;
            //}
        }
    }
}