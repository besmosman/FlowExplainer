using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FlowExplainer;

public class ContextPresentation : Presentation
{
    
    public class VelocityFieldSlide : Slide
    {
        
    }
    
    public override Slide[] GetSlides()
    {
        return [];
    }
}

public class FirstPresentation : Presentation
{
    public class ProgressSlide : Slide
    {
        public override void Draw()
        {
            LayoutMain();
            Title("3D visualizations");
            Presi.MainParagraph(
                @"
PhD Project (30%)
    - Reading
    - Experimenting
    - Double Gyre / Bickley Jet
    - Poincaré sections

    - 3D visualizations
    - Simple Heat Simulation
    - Diagnostics: FTLE / Velocity magnitude

Graduation Paper (70%)
    - IST abstract submitted/accepted
    - Modifications for new fetal dataset
    - Validation
");
            //Presi.ViewPanel("main", (Presi.CanvasSize - new Vec2(0, topbarHeight)) / 2, new Vec2(1920, 1000));
            base.Draw();
        }
    }


    public class RecapSlide : Slide
    {

        public override void Draw()
        {
            LayoutMain();
            Title("Recap");
            Presi.MainParagraph(
                @"
- Focus on graduation paper
- Replicate Speetjens 2012
- Reading
");
            base.Draw();
        }
    }

    public class DemoSlide : Slide
    {
        public override void Load()
        {
            /*var newWorld = Presi.View.World.FlowExplainer.GetGlobalService<WorldManagerService>().NewWorld();
            var newView = Presi.View.World.FlowExplainer.GetGlobalService<ViewsService>().NewView();
            Presi.GetView("second").World = newWorld;
            newView.World = newWorld;*/
            base.Load();
        }
        public override void Draw()
        {
            LayoutMain();
            Presi.ViewPanel("second", Presi.CanvasSize / 2, new Vec2(1,.5f) * Presi.CanvasSize.X*.9f, 1.3f);
            Title("Demo");
            base.Draw();
        }
    }
    
    public class Demo2Slide : Slide
    {
        public override void Load()
        {
            /*var newWorld = Presi.View.World.FlowExplainer.GetGlobalService<WorldManagerService>().NewWorld();
            var newView = Presi.View.World.FlowExplainer.GetGlobalService<ViewsService>().NewView();
            Presi.GetView("second").World = newWorld;
            newView.World = newWorld;*/
            base.Load();
        }

        public override void OnEnter()
        {
            base.OnEnter();
        }
        public override void Draw()
        {
            LayoutMain();
            Title("Demo 2");
            var width = 800;
            var spacing = 100f;
            Presi.ViewPanel("left", Presi.CanvasSize / 2 - new Vec2(width/2f + spacing/2f,00), new Vec2(1,.5f) * width, 1f);
            Presi.ViewPanel("right", Presi.CanvasSize / 2 + new Vec2(width/2f + spacing/2f,00), new Vec2(1,.5f) * width, 1f);
            base.Draw();
        }
    }


    public override Slide[] GetSlides()
    {
        return
        [
            new RecapSlide(),
            new DemoSlide(),
            new Demo2Slide(),
            new ProgressSlide(),
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
         //   Gizmos2D.RectCenter(new ScreenCamera(size.RoundInt()), size / 2, size, FlowExplainer.GetGlobalService<WindowService>()!.ClearColor);
            GL.Disable(EnableCap.Blend);
            Gizmos2D.ImageCentered(new ScreenCamera(size.RoundInt()), presiView.PostProcessingTarget, size / 2, size);
            GL.Enable(EnableCap.Blend);
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
        LoadPresentation(new FirstPresentation());
        StartPresenting();
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