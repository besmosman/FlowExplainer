using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer
{
    public class World
    {
        private static int worldCount;

        public string Name;
        public FlowExplainer FlowExplainer;

        public World(FlowExplainer flowExplainer)
        {
            FlowExplainer = flowExplainer;
            Name = "visualisation " + worldCount++;
        }

        public readonly List<WorldService> Services = new();

        public void AddVisualisationService(WorldService service, int? index = null)
        {
            if (index == null)
                Services.Add(service);
            else
                Services.Insert(index.Value, service);

            service.FlowExplainer = FlowExplainer;
            service.World = this;
            service.Initialize();
        }

        public void ReplaceVisualizationService(WorldService old, WorldService service)
        {
            int index = Services.IndexOf(old);
            RemoveWorldService(old);
            AddVisualisationService(service, index);
        }

        public void RemoveWorldService(WorldService service)
        {
            service.Deinitialize();
            Services.Remove(service);
        }

        public T? GetWorldService<T>() where T : WorldService
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

        public WorldService? GetWorldService(Type t)
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

        public void Draw(View view)
        {
          //  if (!view.World.FlowExplainer.GetGlobalService<PresentationService>()?.IsPresenting == true)
                view.ResizeToTargetSize();

            view.RenderTarget.DrawTo(() =>
            {
                GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
                foreach (var service in Services)
                    if (service.IsEnabled)
                        service.Draw(view.RenderTarget, view);
            });

            RenderTexture.Blit(view.RenderTarget, view.PostProcessingTarget);
        }
    }
}