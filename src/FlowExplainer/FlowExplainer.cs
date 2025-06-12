using System.Diagnostics;

namespace FlowExplainer
{
    public class FlowExplainer
    {
        private readonly List<GlobalService> Services = new();
        private bool shouldRun = true;


        public TimeSpan Time { get; set; }
        public float DeltaTime { get; private set; }

        public void AddGlobalService(GlobalService service)
        {
            Services.Add(service);
            service.FlowExplainer = this;
            service.Initialize();
        }

        public T? GetGlobalService<T>() where T : GlobalService
        {
            foreach (var s in Services)
            {
                if (s is T t)
                {
                    return t;
                }
            }

            return null;
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
                    double startTime = total.Elapsed.TotalSeconds;
                    DeltaTime = (float)w.Elapsed.TotalSeconds;
                    Time += w.Elapsed;
                    w.Restart();


                    foreach (var service in Services)
                        service.Draw();
                    double endTime = total.Elapsed.TotalSeconds;

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