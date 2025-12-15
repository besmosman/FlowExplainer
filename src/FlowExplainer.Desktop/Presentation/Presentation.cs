using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public abstract class Presentation
{
    public PresiContext Presi { get; set; }
    public abstract Slide[] GetSlides();
    public abstract void Setup(FlowExplainer flowExplainer);
    public virtual void Prepare(FlowExplainer flowExplainer)
    {
        foreach (var slide in GetSlides())
        {
            slide.Prepare(flowExplainer);
        }
    }
}

public abstract class NewPresentation
{
    public PresiContext Presi { get; set; }


    public Action<NewPresentation> CurrentLayout;

    public bool BeginSlide(string title)
    {
        Presi.Walk.RenderSlide++;
        bool isCur = Presi.Walk.RenderSlide == Presi.CurrentSlide;
        if (isCur && CurrentLayout != null)
            CurrentLayout(this);
        return isCur;
    }
    public bool SlideEnter()
    {
        return Presi.LastCurrentSlide != Presi.CurrentSlide && Presi.CurrentSlide == Presi.Walk.RenderSlide;
    }

    public bool IsFirstStep()
    {
        return Presi.CurrentStep == 0;
    }

    public bool BeginStep()
    {
        Presi.Walk.FinalRenderStep++;
        bool isCur = Presi.Walk.FinalRenderStep == Presi.CurrentStep;
        return isCur;
    }

    public void Title(string text)
    {
        Presi.Text(text, new Vec2(.5f, .94f), .05, true, Color.White);
    }


    public View DrawWorldPanel(Vec2 relCenterPos, Vec2 relSize, double zoom = 1, Action<World>? load = null,
        [System.Runtime.CompilerServices.CallerFilePath]
        string filePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber]
        int lineNumber = 0)
    {
        var widgetData = Presi.GetWidgetData(filePath, lineNumber);
        var view = Presi.GetView(widgetData, load);
        view.IsActive = true;
        widgetData.RelPosition = relCenterPos;
        widgetData.Size = relSize;
        widgetData.ConnectedObject = view;
        widgetData.CapturesScroll = true;

        //Gizmos2D.ImageCenteredInvertedY(View.Camera2D, Texture.White1x1, center, size);

        var center = Presi.RelToSceen(relCenterPos);
        var size = Presi.RelToSceen(relSize);
        size.Y = size.X * (relSize / relSize.X).Y;
        var rect = new Rect<Vec2>(center - size / 2, center + size / 2);
        if (Presi.View.IsMouseButtonDownLeft && rect.Contains(Presi.View.MousePosition))
        {
            Presi.SelectWidget(widgetData);
            Presi.MouseLeftPressUsed = true;
        }
        if (view.IsSelected)
        {
            var s = size + new Vec2(5, 5);
            Gizmos2D.Rect(Presi.View.Camera2D, center - s / 2, center + s / 2, new Vec4(0, 1, 0, 1f));
        }
        else
        {
            view.Camera2D.Position = -new Vec2(1, .5f) / 2;
            view.Camera2D.Scale = view.PostProcessingTarget.Size.X * zoom;
            view.TargetSize = size;
        }

        GL.Disable(EnableCap.Blend);
        widgetData.RenderMin = center - size / 2;
        widgetData.RenderMax = center + size / 2;
        Gizmos2D.ImageCenteredInvertedY(Presi.View.Camera2D, view.PostProcessingTarget, center, size);
        GL.Enable(EnableCap.Blend);
        return view;
    }


    protected double topbarHeight => 130f;

    public static void LayoutMain(NewPresentation presentation)
    {
        var Presi = presentation.Presi;
        var canvasSize = Presi.CanvasSize;
        Gizmos2D.RectCenter(Presi.View.Camera2D, canvasSize / 2, canvasSize * 5, new Color(.0f, .0f, .0f));
        Gizmos2D.RectCenter(Presi.View.Camera2D, canvasSize / 2, canvasSize, new Color(.1f, .1f, .1f));
        Gizmos2D.RectCenter(Presi.View.Camera2D, new Vec2(canvasSize.X / 2, canvasSize.Y - presentation.topbarHeight / 2f), new Vec2(canvasSize.X, presentation.topbarHeight), new Color(.4f, .0f, .9f));
    }

    public static void LayoutTitle(NewPresentation presentation)
    {
        var Presi = presentation.Presi;
        var canvasSize = Presi.CanvasSize;
        Gizmos2D.RectCenter(Presi.View.Camera2D, canvasSize / 2, canvasSize * 5, new Color(.0f, .0f, .0f));
        var color = new Color(.04f, .04f, .04f);
        Gizmos2D.RectCenter(Presi.View.Camera2D, canvasSize / 2, canvasSize, color);
        var high = new Color(.2f, .3f, .9f);
        Gizmos2D.RectCenter(Presi.View.Camera2D, new Vec2(canvasSize.X / 2, canvasSize.Y / 2 + 60), new Vec2(canvasSize.X, presentation.topbarHeight * 2.1f), high);
    }


    public abstract void Draw();
}