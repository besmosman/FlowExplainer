using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FlowExplainer;

public class TestSlide : Slide
{
    public int n = 9;

    public override void Draw()
    {
        n++;
        Presi.ViewPanel("main", new Vec2(Presi.View.Size.X/2f, Presi.View.Size.Y/2f), new Vec2(1920, 1920/2f));
        Presi.Text($"Double Gyre", new Vec2(20, 20f), 110, false, Color.White);
        base.Draw();
    }
}

public class TestPresentation : Presentation
{
    public override Slide[] GetSlides()
    {
        return [new TestSlide()];
    }
}

public abstract class Presentation
{
    public abstract Slide[] GetSlides();
}

public class PresentatationWorldManagerService : WorldService
{
    public override void Initialize()
    {
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        var pre = GetRequiredGlobalService<PresentationService>();
        pre.CurrentSlide.Presi = pre.Presi;
        pre.CurrentSlide.Draw();
    }
}

public class PresentationService : GlobalService
{
    public bool IsPresenting { get; set; }
    public float RenderScale { get; set; }
    public Slide[] Slides { get; set; }

    public int CurrentSlideIndex { get; set; }
    public Slide CurrentSlide => Slides[CurrentSlideIndex];
    public PresiContext Presi = new PresiContext();
    public View PresiView;
    public World PresiWorld;

    public override void Initialize()
    {
        LoadPresentation(new TestPresentation());
        StartPresenting();
    }

    public void LoadPresentation(Presentation slides)
    {
        Slides = slides.GetSlides();

        PresiView = GetGlobalService<ViewsService>()!.NewView();
        PresiView.Name = "Presentation";
        PresiWorld = new World(FlowExplainer);
        PresiWorld.AddVisualisationService(new PresentatationWorldManagerService());
        PresiWorld.Name = "Presi World";
        PresiView.World = PresiWorld;
        Presi.View = PresiView;
        //Presi.View.Camera2D.Position = view.Camera2D.Position;
        //Presi.View.Camera2D.Scale = view.Camera2D.Scale;
        foreach (var s in Slides)
        {
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
        var baseSize = new Vec2(1920, 1080);
        var window = GetRequiredGlobalService<WindowService>().Window;
        RenderScale = (window.ClientSize.X / baseSize.X);
        CurrentSlide.OnEnter();
    }

    public override void Draw()
    {
        var window = FlowExplainer.GetGlobalService<WindowService>()!.Window;
        if (window.IsKeyPressed(Keys.P))
        {
            if (window.IsKeyDown(Keys.LeftControl))
            {
                LoadPresentation(new TestPresentation());
            }

            if (!IsPresenting)
                StartPresenting();
            else
                PausePresenting();
        }


        if (IsPresenting)
        {
            var view = PresiView;

            CurrentSlide.Presi = Presi;
            Presi.Refresh(this);

            /*
                ImGui.SetNextWindowBgAlpha(1f);
                ImGui.SetNextWindowPos(new Vec2(0, 0));
                var windowSize = window.Size.ToVector2();
                ImGui.SetNextWindowSize(new Vec2(windowSize.X, windowSize.Y));

                ImGui.Begin("Presi");
                ImGui.SetCursorPos(new(0, 0));

                var baseSize = new Vec2(1920, 1200);
                /*view.Camera2D.Position = -baseSize / 2;
                view.Camera2D.Position += new Vec2(0, -30);
                view.Camera2D.Scale = 1;#1#
                view.TargetSize = new Vec2(windowSize.X, windowSize.Y);
                view.ResizeToTargetSize();
                view.World.Draw(view);
                //CurrentSlide.Presi.View = GetRequiredGlobalService<ViewsService>().Views.First();
                view.RenderTarget.DrawTo(() =>
                {
                CurrentSlide.Draw();
                //GL.Disable(EnableCap.DepthTest);
                });
                //GL.Enable(EnableCap.DepthTest);
                var viewrt = view.PostProcessingTarget;
                var viewId = viewrt.TextureHandle;
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vec2(0, 0));
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vec2(0, 0));
                ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vec2(0, 0));
                ImGui.Image(viewId, new Vec2(0, 1), new Vec2(1, 0));
                ImGui.PopStyleVar();
                ImGui.PopStyleVar();
                ImGui.PopStyleVar();
                ImGui.End();*/

            foreach (var v in Presi.ActiveChildViews)
            {
                v.ResizeToTargetSize();
                v.World.Draw(v);
            }

            if (view.IsFullScreen)
            {
                
                var baseSize = new Vec2(1920, 1200);
                view.Camera2D.Position = -baseSize / 2;
                view.Camera2D.Position += new Vec2(0, -30);
                view.Camera2D.Scale = 1;
                
                var size = new Vec2i(window.ClientSize.X, window.ClientSize.Y);
                view.TargetSize = size.ToVec2();
                view.ResizeToTargetSize();
                view.World.Draw(view);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.Viewport(0, 0, size.X, size.Y);
                
                Gizmos2D.RectCenter(new ScreenCamera(size), size.ToVec2() / 2, size.ToVec2(), FlowExplainer.GetGlobalService<WindowService>()!.ClearColor);
                Gizmos2D.ImageCentered(new ScreenCamera(size), view.PostProcessingTarget, size.ToVec2() / 2, size.ToVec2());
            }

            if (window.IsKeyPressed(Keys.Up))
                PrevSlide();

            if (window.IsKeyPressed(Keys.Down))
                NextSlide();


            if (window.IsKeyPressed(Keys.F12))
                PresiView.IsFullScreen = !PresiView.IsFullScreen;

            if (window.MouseState.ScrollDelta.Y != 0)
            {
                if (window.IsKeyDown(Keys.LeftShift))
                {
                    RenderScale *= 1f + ((window.MouseState.Scroll - window.MouseState.PreviousScroll).Y) / 60f;
                }
            }
        }
    }
}