using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class PresiContext
{
    public FlowExplainer FlowExplainer;
    public View View = null!;
    private Dictionary<int, WidgetData> widgetsById = new();

    private Dictionary<string, View> presiViewsByName = new();
    public Vec2 CanvasSize;
    public Vec2 CanvasCenter => CanvasSize / 2;

    public PresiContext(FlowExplainer flowExplainer)
    {
        FlowExplainer = flowExplainer;
    }

    public IEnumerable<View> ActiveChildViews => presiViewsByName.Values;

    public class WidgetData
    {
        public Vec2 Position;
        public Vec2 Size;
    }

    public void Text(string title, Vec2 pos, float lh, bool centered, Color color, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber]
        int lineNumber = 0)
    {
        var widgetData = GetWidgetData(filePath, lineNumber);
        widgetData.Position = pos;
        widgetData.Size = new Vec2(lh, lh);
        Gizmos2D.AdvText(View.Camera2D, pos, lh, color, title, 1, centered);
    }


    public void Image(Texture texture, Vec2 center, float width, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber]
        int lineNumber = 0)
    {
        var widgetData = GetWidgetData(filePath, lineNumber);
        widgetData.Position = center;
        widgetData.Size.X = width;
        Gizmos2D.ImageCentered(View.Camera2D, texture, center, width);

    }

    public void Checkbox(string name, ref bool value, Vec2 center,
        [System.Runtime.CompilerServices.CallerFilePath]
        string filePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber]
        int lineNumber = 0)
    {
        float height = 50;
        var widgetData = GetWidgetData(filePath, lineNumber);
        widgetData.Position = center;
        widgetData.Size = new Vec2(height, height);
        Gizmos2D.RectCenter(View.Camera2D, center, widgetData.Size, Color.White);
        if (value)
            Gizmos2D.RectCenter(View.Camera2D, center, widgetData.Size * .8f, Color.Grey(.4f));
        Gizmos2D.Text(View.Camera2D, center + new Vec2(50, 0), 48, Color.White, name);
        var rect = new Rect<Vec2>(widgetData.Position - widgetData.Size/2, widgetData.Position + widgetData.Size/2);
        if (View.IsMouseButtonPressedLeft && rect.Contains(View.MousePosition))
        {
            value = !value;
        }
    }

    public void Slider(string name, ref float value, float minValue, float maxValue, Vec2 center, float width,
        [System.Runtime.CompilerServices.CallerFilePath]
        string filePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber]
        int lineNumber = 0)
    {
        float height = 100;
        var widgetData = GetWidgetData(filePath, lineNumber);
        widgetData.Position = center;
        widgetData.Size = new Vec2(width, height);

        var left = new Vec2(widgetData.Position.X - widgetData.Size.X / 2f, widgetData.Position.Y);
        var right = new Vec2(widgetData.Position.X + widgetData.Size.X / 2f, widgetData.Position.Y);
        Gizmos2D.Line(View.Camera2D, left, right, Color.White, 10);
        var t = (value - minValue) / (maxValue - minValue);
        t = float.Clamp(t, 0, 1);
        Gizmos2D.Circle(View.Camera2D, Utils.Lerp(left, right, t), Color.White, 20);
        Gizmos2D.AdvText(View.Camera2D, center + new Vec2(0, -40), 48, Color.White, name + " = " + value.ToString("N2"), centered: true);
        var rect = new Rect<Vec2>(center - new Vec2(width / 2 + 30, height / 2), center + new Vec2(width / 2 + 30, height / 2));
        if (View.IsMouseButtonDownLeft && rect.Contains(View.MousePosition))
        {
            float newT = (View.MousePosition.X - left.X) / width;
            newT = float.Clamp(newT, 0, 1);

            value = float.Lerp(minValue, maxValue, newT);
        }
    }

    public void MainParagraph(string title, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber]
        int lineNumber = 0)
    {
        var widgetData = GetWidgetData(filePath, lineNumber);
        var pos = new Vec2(50, CanvasSize.Y - 250);
        var lh = 64;
        widgetData.Position = pos;
        widgetData.Size = new Vec2(lh, lh);
        if (title.StartsWith("\r\n"))
        {
            title = title[2..];
        }
        Gizmos2D.AdvText(View.Camera2D, pos, lh, Color.White, title, 1);
    }

    public void ViewPanel(string viewname, Vec2 center, Vec2 size, float zoom = 1f, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber]
        int lineNumber = 0)
    {
        var view = GetView(viewname);
        view.Camera2D.Position = -new Vec2(1, .5f) / 2;
        view.Camera2D.Scale = view.PostProcessingTarget.Size.X * zoom;
        view.AltClearColor = new Color(.1f, .1f, .1f);
        view.TargetSize = size;
        //Gizmos2D.ImageCenteredInvertedY(View.Camera2D, Texture.White1x1, center, size);
        GL.Disable(EnableCap.Blend);
        Gizmos2D.ImageCenteredInvertedY(View.Camera2D, view.PostProcessingTarget, center, size);
        GL.Enable(EnableCap.Blend);
    }

    public View GetView(string viewname)
    {
        if (!presiViewsByName.ContainsKey(viewname))
        {
            var world1 = FlowExplainer.GetGlobalService<WorldManagerService>()!.Worlds[0];
            var v = new View(1, 1, world1)
            {
                Controller = new PresiChildViewController()
            };
            presiViewsByName.Add(viewname, v);
        }

        var view = presiViewsByName[viewname];
        return view;
    }

    private int GetId(string filepath, int lineNumber)
    {
        if (filepath.StartsWith('#'))
            return filepath.GetHashCode();
        else
            return HashCode.Combine(filepath.GetHashCode(), lineNumber.GetHashCode());
    }

    private WidgetData GetWidgetData(string filepath, int linenumber)
    {
        var id = GetId(filepath, linenumber);
        if (!widgetsById.TryGetValue(id, out WidgetData w))
        {
            w = new WidgetData();
            widgetsById.Add(id, w);
        }

        return w;
    }

    public void Refresh(PresentationService presentationService)
    {
        CanvasSize = presentationService.CanvasSize;
    }
}