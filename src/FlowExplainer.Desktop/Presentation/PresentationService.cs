using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FlowExplainer;

public class PresentationService : GlobalService
{
    public bool IsPresenting { get; set; }
    //public Slide[] Slides { get; set; }
    public NewPresentation Presentation;
    //public int CurrentSlideIndex { get; set; }
    // public Slide CurrentSlide => Slides[CurrentSlideIndex];
    public PresiContext Presi = null!;
    public View? PresiView;
    public Vec2 CanvasSize = new Vec2(1920, 1200);

    public override void Initialize()
    {
        Presi = new PresiContext(FlowExplainer);
        //LoadPresentation(new DatasetPresentation());
        //StartPresenting();
    }

    /*
    public void LoadPresentation(Presentation presentation)
    {
        Slides = presentation.GetSlides();
        presentation.Setup(FlowExplainer);
        presentation.Presi = Presi;
        PresiView = GetGlobalService<ViewsService>()!.NewView();
        PresiView.Name = "Presentation";
        PresiView.Controller = new PresentationViewController();
        Presi.View = PresiView;

        foreach (var s in Slides)
        {
            s.Presi = Presi;
            s.Load();
        }
    }
    */


    public void LoadPresentation(NewPresentation presentation)
    {
        Presi.PanelBackgroundColor = Style.Current.BackgroundColor;
        presentation.Presi = Presi;
        PresiView = GetGlobalService<ViewsService>()!.NewView();
        PresiView.Name = "Presentation";
        Presentation = presentation;
        Presentation.Presi = Presi;
        PresiView.Controller = new PresentationViewController();
        Presi.View = PresiView;
    }


    private void NextSlide()
    {
        var window = GetRequiredGlobalService<WindowService>().Window;

        if (window.IsKeyDown(Keys.LeftShift) || Presi.Walk.FinalRenderStep == Presi.CurrentStep)
        {
            Presi.CurrentSlide++;
            Presi.CurrentStep = 0;
        }
        else
            Presi.CurrentStep++;
    }


    private void PrevSlide()
    {
        var window = GetRequiredGlobalService<WindowService>().Window;
        if (window.IsKeyDown(Keys.LeftShift) || Presi.CurrentStep == 0)
        {
            Presi.CurrentSlide--;
            Presi.CurrentStep = 0;
        }
        else
            Presi.CurrentStep--;
        /*CurrentSlide.OnLeave();
        CurrentSlideIndex = int.Max(0, CurrentSlideIndex - 1);
        CurrentSlide.OnEnter();*/
    }

    private void PausePresenting()
    {
        IsPresenting = false;
    }

    public void StartPresenting()
    {
        IsPresenting = true;
        var window = GetRequiredGlobalService<WindowService>().Window;
        PresiView!.Camera2D.Scale = (window.ClientSize.Y / CanvasSize.Y) * .9f;
        //CurrentSlide.OnEnter();
        if (!PresiView.IsFullScreen)
            ToggleFullScreen();
    }

    private List<Vec2> highlighted = new();
    public override void Draw()
    {
        var window = FlowExplainer.GetGlobalService<WindowService>()!.Window;
        if (window.IsKeyPressed(Keys.P))
        {
            if (window.IsKeyDown(Keys.LeftControl))
            {
                //LoadPresentation(new HeatStructuresPresentation());
            }

            if (!IsPresenting)
                StartPresenting();
            else
                PausePresenting();
        }

        if (PresiView != null && window.IsMouseButtonDown(MouseButton.Right))
        {
            if (Vec2.Distance(highlighted.LastOrDefault(), PresiView.MousePosition) > 3)
                highlighted.Add(PresiView.MousePosition);
        }
        else
        {
            highlighted.Clear();
        }


        if (IsPresenting)
        {
            var presiView = PresiView;

            // CurrentSlide.Presi = Presi;

            var max = Presi.presiViews.Keys.MaxBy(m => m.TimeSinceLastFetch)?.TimeSinceLastFetch ?? 10;
            foreach (var pv in Presi.presiViews.ToList())
            {
                //if (pv.Key.TimeSinceLastFetch > max * 10 || pv.Key.TimeSinceLastFetch > 5)
                if (!pv.Value.IsActive)
                {
                    Presi.presiViews.Remove(pv.Key);
                    GetGlobalService<WorldManagerService>().Worlds.Remove(pv.Value.World);
                    Logger.LogDebug("deleted world");
                }
            }

            foreach (var v in Presi.ActiveChildViews)
            {
                v.Controller.UpdateAndDraw(v);
            }

            if (presiView.IsFullScreen)
            {
                presiView.Controller.UpdateAndDraw(presiView);
            }
            Presi.LastCurrentSlide = Presi.CurrentSlide;

            if (window.IsKeyPressed(Keys.Up))
                PrevSlide();

            if (window.IsKeyPressed(Keys.Down))
                NextSlide();


            if (window.IsKeyPressed(Keys.F12))
            {
                ToggleFullScreen();
            }


        }
    }

    public override void AfterDraw()
    {
        var presiView = PresiView;

        if (presiView?.IsFullScreen == true)
        {
            var window = FlowExplainer.GetGlobalService<WindowService>()!.Window;
            var size = new Vec2(window.ClientSize.X, window.ClientSize.Y);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, (int)size.X, (int)size.Y);
            // GL.Clear(ClearBufferMask.ColorBufferBit);
            //   Gizmos2D.RectCenter(new ScreenCamera(size.RoundInt()), size / 2, size, FlowExplainer.GetGlobalService<WindowService>()!.ClearColor);
            GL.Disable(EnableCap.Blend);
            Gizmos2D.ImageCentered(new ScreenCamera(size.RoundInt()), presiView.PostProcessingTarget, size / 2, size);
            GL.Enable(EnableCap.Blend);
        }
        if (presiView != null)
        {
            for (int i = 0; i < highlighted.Count - 1; i++)
            {
                var vec2 = highlighted[i];
                var dir = highlighted[i + 1] - vec2;
                Gizmos2D.Instanced.RegisterLine(highlighted[i] - dir / 10, highlighted[i + 1], new Color(1, 0, 0, 1), 10f);
            }
            Gizmos2D.Instanced.RenderRects(presiView.Camera2D);
        }
        base.AfterDraw();
    }
    private void ToggleFullScreen()
    {
        var window = FlowExplainer.GetGlobalService<WindowService>()!.Window;
        PresiView.IsFullScreen = !PresiView.IsFullScreen;
        if (PresiView.IsFullScreen)
            PresiView.Camera2D.Scale = (window.ClientSize.Y / CanvasSize.Y) * .9f;
        else
            Task.Run(() =>
            {
                Thread.Sleep(50);
                PresiView.Camera2D.Scale = (PresiView.TargetSize.X / CanvasSize.X) * .9f;
            });
    }
}