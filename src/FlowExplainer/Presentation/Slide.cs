namespace FlowExplainer;

public abstract class Slide
{
    protected World w0 => Presi.GetView("v0").World;
    protected World w1 => Presi.GetView("v1").World;
    protected World w2 => Presi.GetView("v2").World;
    protected World w3 => Presi.GetView("v3").World;
    protected World[] worlds => [w0, w1, w2, w3];

    public PresiContext Presi = null!;

    public bool OverrideNextSlideAction = false;

    protected float topbarHeight => 130f;

    public void Title(string text, string filePath = "", [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
    {
        Presi.Text(text, new Vec2(Presi.CanvasSize.X / 2, Presi.CanvasSize.Y - topbarHeight / 2f), 90, true, Color.White);
    }

    public void TitleTitle(string text, string subText, string filePath = "", [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
    {
        var x = Presi.CanvasSize.X / 2;
        Presi.Text(text, new Vec2(x, Presi.CanvasCenter.Y + topbarHeight / 1.4f), 140, true, Color.White);
        Presi.Text(subText, new Vec2(x, Presi.CanvasCenter.Y - 0), 80, true, Color.White);
    }

    public void MainParagraph(string text, string filePath = "", [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
    {
        Presi.MainParagraph(text, filePath, lineNumber);
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
        Gizmos2D.RectCenter(Presi.View.Camera2D, new Vec2(canvasSize.X / 2, canvasSize.Y / 2 + 80), new Vec2(canvasSize.X, topbarHeight * 1.5f), new Color(.5f, .2f, .7f));
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