namespace FlowExplainer;

public abstract class Slide
{
    public PresiContext Presi = null!;

    public bool OverrideNextSlideAction = false;

    protected float topbarHeight => 150f;

    public void Title(string text, string filePath = "", [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
    {
        Presi.Text(text, new Vec2(Presi.CanvasSize.X / 2, Presi.CanvasSize.Y - topbarHeight / 2f), 110, true, Color.White);
    }

    public void TitleTitle(string text, string subText, string filePath = "", [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
    {
        var x = Presi.CanvasSize.X / 4;
        Presi.Text(text, new Vec2(x, Presi.CanvasCenter.Y + topbarHeight / 2f), 110, false, Color.White);
        Presi.Text(subText, new Vec2(x, Presi.CanvasCenter.Y - 20 ), 80, false, Color.White);
    }

    public void LayoutMain()
    {
        var canvasSize = Presi.CanvasSize;
        Gizmos2D.RectCenter(Presi.View.Camera2D, canvasSize / 2, canvasSize * 5, new Color(.0f, .0f, .0f));
        Gizmos2D.RectCenter(Presi.View.Camera2D, canvasSize / 2, canvasSize, new Color(.1f, .1f, .1f));
        Gizmos2D.RectCenter(Presi.View.Camera2D, new Vec2(canvasSize.X / 2, canvasSize.Y - topbarHeight / 2f), new Vec2(canvasSize.X, topbarHeight), new Color(.4f, .0f, .9f));
    }

    public void LayoutTitle()
    {
        var canvasSize = Presi.CanvasSize;
        Gizmos2D.RectCenter(Presi.View.Camera2D, canvasSize / 2, canvasSize * 5, new Color(.0f, .0f, .0f));
        Gizmos2D.RectCenter(Presi.View.Camera2D, canvasSize / 2, canvasSize, new Color(.1f, .1f, .1f));
        Gizmos2D.RectCenter(Presi.View.Camera2D, new Vec2(canvasSize.X / 2, canvasSize.Y / 2 + 80), new Vec2(canvasSize.X, topbarHeight), new Color(.4f, .0f, .9f));
    }


    public World MainWorld => Presi.GetView("main").World;

    public virtual void Draw()
    {
    }

    public virtual void Next()
    {
    }

    public virtual void Load()
    {
    }

    public virtual void OnLeave()
    {
    }

    public virtual void OnEnter()
    {
    }
}