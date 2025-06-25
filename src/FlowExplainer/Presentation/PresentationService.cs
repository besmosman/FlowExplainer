using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FlowExplainer;

public class TestSlide : Slide
{
    public int n = 9;

    public override void Draw()
    {
        LayoutMain();
        Title("FTLE");
        Presi.ViewPanel("main", (Presi.CanvasSize - new Vec2(0, topbarHeight)) / 2, new Vec2(1920, 1000));
        base.Draw();
    }
}



public class FirstPresentation : Presentation
{
    
    public class VelocityFieldVisualizerSlide : Slide
    {
        public override void OnEnter()
        {
            MainWorld.GetWorldService<FlowFieldVisualizer>()!.IsEnabled = true;
            base.OnEnter();
        }

        public override void Draw()
        {
            LayoutMain();
            Title("Double Gyre Flow");
            MainWorld.GetWorldService<FlowFieldVisualizer>()!.IsEnabled = true;
            if (ImGui.Begin("edit", ImGuiWindowFlags.NoDecoration))
            {
              MainWorld.GetWorldService<FlowFieldVisualizer>()!.DrawImGuiEdit();
            }
            ImGui.End();
            base.Draw();
        }

        public override void OnLeave()
        {
            MainWorld.GetWorldService<FlowFieldVisualizer>()!.IsEnabled = false;
            base.OnLeave();
        }
    }
    
    public override Slide[] GetSlides()
    {
        return [
            new TestSlide(),
            new VelocityFieldVisualizerSlide(),
        ];
    }
}

public abstract class Presentation
{
    public abstract Slide[] GetSlides();
}

public class PresentationViewController : IViewController
{
    public void UpdateAndDraw(View presiView)
    {
        var FlowExplainer = presiView.World.FlowExplainer;
        var window = FlowExplainer.GetGlobalService<WindowService>()!.Window;
        var baseSize = FlowExplainer.GetGlobalService<PresentationService>()!.CanvasSize;
        presiView.Camera2D.Position = -baseSize / 2;

        var size = new Vec2(window.ClientSize.X, window.ClientSize.Y);
        if (presiView.IsFullScreen)
        {
            presiView.TargetSize = size;
        }
        else
        {
            ImGUIViewRenderer.Render(presiView, FlowExplainer);
        }

        presiView.ResizeToTargetSize();

        var pre = FlowExplainer.GetGlobalService<PresentationService>()!;
        pre.CurrentSlide.Presi = pre.Presi;

        presiView.RenderTarget.DrawTo(() =>
        {
            GL.ClearColor(presiView.ClearColor.R, presiView.ClearColor.G, presiView.ClearColor.B, presiView.ClearColor.A);
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
            pre.CurrentSlide.Draw();
        });

        RenderTexture.Blit(presiView.RenderTarget, presiView.PostProcessingTarget);


        if (presiView.IsFullScreen)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, (int)size.X, (int)size.Y);
            Gizmos2D.RectCenter(new ScreenCamera(size.RoundInt()), size / 2, size, FlowExplainer.GetGlobalService<WindowService>()!.ClearColor);
            Gizmos2D.ImageCentered(new ScreenCamera(size.RoundInt()), presiView.PostProcessingTarget, size / 2, size);
        }

        //presiView.Camera2D.Scale = 1;
        if (presiView.IsFullScreen || presiView.IsSelected)
        {
            if (window.MouseState.ScrollDelta.Y != 0)
            {
                presiView.Camera2D.Scale *= (1f + window.MouseState.ScrollDelta.Y * .01f);
            }
        }
    }
}

public class PresentationService : GlobalService
{
    public bool IsPresenting { get; set; }
    public Slide[] Slides { get; set; }

    public int CurrentSlideIndex { get; set; }
    public Slide CurrentSlide => Slides[CurrentSlideIndex];
    public PresiContext Presi = new PresiContext();
    public View PresiView;
    public Vec2 CanvasSize = new Vec2(1920, 1200);

    public override void Initialize()
    {
       // LoadPresentation(new FirstPresentation());
       // StartPresenting();
    }

    public void LoadPresentation(Presentation slides)
    {
        Slides = slides.GetSlides();

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


    private void NextSlide()
    {
        var window = GetRequiredGlobalService<WindowService>().Window;

        if (CurrentSlide.OverrideNextSlideAction && !window.IsKeyDown(Keys.LeftShift))
            CurrentSlide.Next();
        else
        {
            CurrentSlide.OnLeave();
            CurrentSlideIndex = int.Min(Slides.Length - 1, CurrentSlideIndex + 1);
            CurrentSlide.OnEnter();
        }
    }


    private void PrevSlide()
    {
        CurrentSlide.OnLeave();
        CurrentSlideIndex = int.Max(0, CurrentSlideIndex - 1);
        CurrentSlide.OnEnter();
    }

    private void PausePresenting()
    {
        IsPresenting = false;
    }

    public void StartPresenting()
    {
        IsPresenting = true;
        var window = GetRequiredGlobalService<WindowService>().Window;
        PresiView.Camera2D.Scale = (window.ClientSize.Y / CanvasSize.Y) * .9f;
        CurrentSlide.OnEnter();
    }

    public override void Draw()
    {
        var window = FlowExplainer.GetGlobalService<WindowService>()!.Window;
        if (window.IsKeyPressed(Keys.P))
        {
            if (window.IsKeyDown(Keys.LeftControl))
            {
                LoadPresentation(new FirstPresentation());
            }

            if (!IsPresenting)
                StartPresenting();
            else
                PausePresenting();
        }


        if (IsPresenting)
        {
            var presiView = PresiView;

            CurrentSlide.Presi = Presi;
            Presi.Refresh(this);

            foreach (var v in Presi.ActiveChildViews)
            {
                v.Controller.UpdateAndDraw(v);
            }

            if (presiView.IsFullScreen)
            {
                presiView.Controller.UpdateAndDraw(presiView);
            }

            if (window.IsKeyPressed(Keys.Up))
                PrevSlide();

            if (window.IsKeyPressed(Keys.Down))
                NextSlide();


            if (window.IsKeyPressed(Keys.F12))
            {
                PresiView.IsFullScreen = !PresiView.IsFullScreen;
                PresiView.Camera2D.Scale = (window.ClientSize.Y / CanvasSize.Y) * .9f;
            }
        }
    }
}