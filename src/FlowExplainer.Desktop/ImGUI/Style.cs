namespace FlowExplainer;

public class Style
{
    public Color BackgroundColor;
    public Color HighlightColor;
    public Color TextColor;

    public static Style Current => Dark;

    private static readonly Style Dark = new Style()
    {
        BackgroundColor = Color.Grey(0.0f),
        HighlightColor = new Color(.0f, .5f, 1f),
        TextColor = Color.White,
    };
    
    private static readonly Style Light = new Style()
    {
        BackgroundColor = Color.Grey(1f).WithAlpha(0),
        HighlightColor = new Color(.0f, .5f, 1f),
        TextColor = Color.Black,
    };
    
}