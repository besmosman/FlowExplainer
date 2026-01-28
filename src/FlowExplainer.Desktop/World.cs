using System.Reflection;
using Microsoft.VisualBasic;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer
{
    public class World
    {
        private static int worldCount;
        public bool IsViewed;
        public string Name;
        public FlowExplainer FlowExplainer;

        public World(FlowExplainer flowExplainer)
        {
            FlowExplainer = flowExplainer;
            Name = "world " + worldCount++;
        }

        public readonly List<WorldService> Services = new();

        public T AddVisualisationService<T>(int? index = null) where T : WorldService
        {
            var worldService = Activator.CreateInstance<T>();
            AddVisualisationService(worldService);
            return worldService;
        }

        public void AddVisualisationService(WorldService service, int? index = null)
        {
            if (index == null)
                Services.Add(service);
            else
                Services.Insert(index.Value, service);

            service.FlowExplainer = FlowExplainer;
            service.World = this;

            if (!service.IsEnabled)
                service.Enable();
        }

        public IEnumerable<ISelectableVectorField<I, O>> GetSelectableVectorFields<I, O>() where I : IVec<I, double>
        {
            if (typeof(I) == typeof(Vec2) && typeof(O) == typeof(double))
                foreach (var s in Services)
                foreach (var f in s.GetSelectableVec2Vec1())
                    yield return (ISelectableVectorField<I, O>)f;
            else if (typeof(I) == typeof(Vec3) && typeof(O) == typeof(Vec2))
                foreach (var s in Services)
                foreach (var f in s.GetSelectableVec3Vec2())
                    yield return (ISelectableVectorField<I, O>)f;
            else if (typeof(I) == typeof(Vec3) && typeof(O) == typeof(double))
                foreach (var s in Services)
                foreach (var f in s.GetSelectableVec3Vec1())
                    yield return (ISelectableVectorField<I, O>)f;
            else throw new NotImplementedException();
        }

        public void ReplaceVisualizationService(WorldService old, WorldService service)
        {
            int index = Services.IndexOf(old);
            RemoveWorldService(old);
            AddVisualisationService(service, index);
        }

        private List<WorldService> toRemove = new();

        public void RemoveWorldService(WorldService service)
        {
            toRemove.Add(service);
        }

        public DataService DataService => GetWorldService<DataService>();

        public T GetWorldService<T>() where T : WorldService
        {
            foreach (var s in Services)
            {
                if (s is T t)
                {
                    return t;
                }
            }

            //Auto add?
            throw new Exception();
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

        public void Update()
        {
            foreach (var service in toRemove)
            {
                service.Deinitialize();
                Services.Remove(service);
            }

            toRemove.Clear();

            foreach (var service in Services)
                if (service.IsEnabled)
                {
                    if (!service.IsInitialzied)
                    {
                        service.Initialize();
                        service.IsInitialzied = true;
                    }

                    service.Update();
                }
        }


        public void Draw(View view)
        {
            Profiler.Begin($"Draw '{view.Name}'");

            IsViewed = true;
            //  if (!view.World.FlowExplainer.GetGlobalService<PresentationService>()?.IsPresenting == true)
            view.ResizeToTargetSize();
            view.RenderTarget.DrawTo(() =>
            {
                var clearColor = view.AltClearColor ?? Style.Current.BackgroundColor;
                GL.ClearColor((float)clearColor.R, (float)clearColor.G, (float)clearColor.B, (float)clearColor.A);
                GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
                foreach (var service in Services)
                {
                    Profiler.Begin(service?.Name ?? service!.GetType().Name);
                    if (service.IsEnabled)
                    {
                        if (!service.IsInitialzied)
                        {
                            service.Initialize();
                            service.IsInitialzied = true;
                        }

                        service.Draw(view.RenderTarget, view);
                    }

                    Profiler.End(service?.Name ?? service!.GetType().Name);
                }

                if (!string.IsNullOrEmpty(ImGuiHelpers.LastMessage))
                {
                    var t = 1.5f + (double)(ImGuiHelpers.MessageTime - DateTime.Now).TotalSeconds;
                    if (t > 0)
                    {
                        Gizmos2D.Text(view.ScreenCamera, new Vec2(view.RenderTarget.Size.X / 2f, view.RenderTarget.Size.Y - 90), 80, new Color(1, 1, 0, 1.5f - (1.5f - t) * (1.5f - t)), ImGuiHelpers.LastMessage, centered: true);
                    }
                }
            });
            RenderTexture.Blit(view.RenderTarget, view.PostProcessingTarget);
            Profiler.End($"Draw '{view.Name}'");
        }
    }
}