using ImGuiNET;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FlowExplainer;

public class PresentationService : GlobalService
{
    public bool IsPresenting { get; set; }
    public Slide[] Slides { get; set; }

    public int CurrentSlideIndex { get; set; }
    public Slide CurrentSlide => Slides[CurrentSlideIndex];
    public PresiContext Presi = null!;
    public View PresiView;
    public Vec2 CanvasSize = new Vec2(1920, 1200);

    public override void Initialize()
    {
        Presi = new PresiContext(FlowExplainer);
        //LoadPresentation(new DatasetPresentation());
        //StartPresenting();
    }

    public void LoadPresentation(Presentation presentation)
    {
        Slides = presentation.GetSlides();
        presentation.Setup(FlowExplainer);

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
    }
}