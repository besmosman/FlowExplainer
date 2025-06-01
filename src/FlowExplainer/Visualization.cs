using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer
{
    public class Visualisation
    {
        private static int visualisationCount;

        public string Name;
        public FlowExplainer FlowExplainer;

        public Visualisation(FlowExplainer flowExplainer)
        {
            FlowExplainer = flowExplainer;
            Name = "visualisation " + visualisationCount++;
        }

        public readonly List<VisualisationService> Services = new();

        public void AddVisualisationService(VisualisationService service, int? index = null)
        {
            if (index == null)
                Services.Add(service);
            else
                Services.Insert(index.Value, service);

            service.FlowExplainer = FlowExplainer;
            service.Visualisation = this;
            service.Initialize();
        }

        public void ReplaceVisualizationService(VisualisationService old, VisualisationService service)
        {
            int index = Services.IndexOf(old);
            RemoveVisualizationService(old);
            AddVisualisationService(service, index);
        }

        public void RemoveVisualizationService(VisualisationService service)
        {
            service.Deinitialize();
            Services.Remove(service);
        }

        public T? GetVisualisationService<T>() where T : VisualisationService
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

        public VisualisationService? GetVisualisationService(Type t)
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