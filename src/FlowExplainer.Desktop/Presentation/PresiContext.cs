using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class PresiContext
{
    public FlowExplainer FlowExplainer;
    public View View = null!;
    private Dictionary<int, WidgetData> widgetsById = new();

    public Dictionary<WidgetData, View> presiViews = new();
    public Vec2 CanvasSize;
    public Vec2 CanvasCenter => CanvasSize / 2;
    public Rect<Vec2> CanvasRect;
    public bool MouseLeftPressUsed;
    public WidgetData? SelectedWidget;


    public int CurrentSlide = 0;
    public int LastCurrentSlide = 0;
    public int CurrentStep = 0;

    public class WalkInfo
    {
        public int RenderSlide;
        public int FinalRenderStep;

        public void Reset()
        {
            RenderSlide = -1;
            FinalRenderStep = 0;
        }
    }


    public WalkInfo Walk = new();
    public PresiContext(FlowExplainer flowExplainer)
    {
        FlowExplainer = flowExplainer;
    }

    public IEnumerable<View> ActiveChildViews => presiViews.Values;

    public class WidgetData
    {
        public Vec2 RelPosition;
        public Vec2 Size;
        public object ConnectedObject;
        public double TimeSinceLastFetch;
        public bool CapturesScroll;
    }




    public void Text(string title, Vec2 relPos, double lh, bool centered, Color color, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber]
        int lineNumber = 0)
    {
        var widgetData = GetWidgetData(filePath, lineNumber);
        widgetData.RelPosition = relPos;
        widgetData.Size = new Vec2(lh, lh);
        Gizmos2D.AdvText(View.Camera2D, RelToSceen(relPos), CanvasRect.FromRelative(new Vec2(lh, lh)).X, color, title, 1, centered);
    }

    public Vec2 RelToSceen(Vec2 rel)
    {
        return CanvasRect.FromRelative(rel);
    }

    public void Image(Texture texture, Vec2 relCenter, double relWidth, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber]
        int lineNumber = 0)
    {
        var widgetData = GetWidgetData(filePath, lineNumber);
        widgetData.RelPosition = relCenter;
        widgetData.Size.X = relWidth;
        Gizmos2D.ImageCentered(View.Camera2D, texture, RelToSceen(relCenter), RelToSceen(relWidth));

    }

    public void Checkbox(string name, ref bool value, Vec2 relCenter,
        [System.Runtime.CompilerServices.CallerFilePath]
        string filePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber]
        int lineNumber = 0)
    {
        double height = .05;
        var widgetData = GetWidgetData(filePath, lineNumber);
        widgetData.RelPosition = relCenter;
        var center = RelToSceen(relCenter);
        widgetData.Size = new Vec2(height, height);
        var size = RelToSceen(widgetData.Size);
        size.X = size.Y;
        Gizmos2D.RectCenter(View.Camera2D, center, size, Color.White);
        if (value)
            Gizmos2D.RectCenter(View.Camera2D, center, size * .8f, Color.Grey(.4f));
        Gizmos2D.Text(View.Camera2D, center + new Vec2(50, 0), 48, Color.White, name);
        var rect = new Rect<Vec2>(center - size / 2, center + size / 2);
        if (View.IsMouseButtonPressedLeft && rect.Contains(View.MousePosition))
        {
            value = !value;
        }
    }

    public void Slider(string name, ref double value, double minValue, double maxValue, Vec2 relCenter, double relWidth,
        [System.Runtime.CompilerServices.CallerFilePath]
        string filePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber]
        int lineNumber = 0)
    {
        var widgetData = GetWidgetData(filePath, lineNumber);
        widgetData.RelPosition = relCenter;
        var center = RelToSceen(relCenter);
        double height = RelToSceen(.04f);
        widgetData.Size = new Vec2(relWidth, height);
        double width = RelToSceen(relWidth);

        var left = RelToSceen(new Vec2(widgetData.RelPosition.X - widgetData.Size.X / 2f, widgetData.RelPosition.Y));
        var right = RelToSceen(new Vec2(widgetData.RelPosition.X + widgetData.Size.X / 2f, widgetData.RelPosition.Y));
        Gizmos2D.Line(View.Camera2D, left, right, Color.White, 10);
        var t = (value - minValue) / (maxValue - minValue);
        t = double.Clamp(t, 0, 1);
        Gizmos2D.Circle(View.Camera2D, Utils.Lerp(left, right, t), Color.White, 20);
        Gizmos2D.AdvText(View.Camera2D, center + new Vec2(0, -40), 48, Color.White, name + " = " + value.ToString("N2"), centered: true);
        var rect = new Rect<Vec2>(center - new Vec2(width / 2, height / 2), center + new Vec2(width / 2, height / 2));
        if (View.IsMouseButtonDownLeft && rect.Contains(View.MousePosition))
        {
            double newT = (View.MousePosition.X - left.X) / width;
            newT = double.Clamp(newT, 0, 1);
            value = double.Lerp(minValue, maxValue, newT);
            SelectWidget(widgetData);
            MouseLeftPressUsed = true;
        }
    }
    private double RelToSceen(double width)
    {
        return RelToSceen(new Vec2(width, width)).X;
    }

    public void SelectWidget(WidgetData widgetData)
    {
        SelectedWidget = widgetData;
    }


    public void SliderCustomTitle(string title, ref double value, double minValue, double maxValue, Vec2 center, double width,
        [System.Runtime.CompilerServices.CallerFilePath]
        string filePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber]
        int lineNumber = 0)
    {
        double height = 100;
        var widgetData = GetWidgetData(filePath, lineNumber);
        widgetData.RelPosition = center;
        widgetData.Size = new Vec2(width, height);

        var left = new Vec2(widgetData.RelPosition.X - widgetData.Size.X / 2f, widgetData.RelPosition.Y);
        var right = new Vec2(widgetData.RelPosition.X + widgetData.Size.X / 2f, widgetData.RelPosition.Y);
        Gizmos2D.Line(View.Camera2D, left, right, Color.White, 10);
        var t = (value - minValue) / (maxValue - minValue);
        t = double.Clamp(t, 0, 1);
        Gizmos2D.Circle(View.Camera2D, Utils.Lerp(left, right, t), Color.White, 20);
        Gizmos2D.AdvText(View.Camera2D, center + new Vec2(0, -40), 48, Color.White, title, centered: true);
        var rect = new Rect<Vec2>(center - new Vec2(width / 2 + 30, height / 2), center + new Vec2(width / 2 + 30, height / 2));
        if (View.IsMouseButtonDownLeft && rect.Contains(View.MousePosition))
        {
            double newT = (View.MousePosition.X - left.X) / width;
            newT = double.Clamp(newT, 0, 1);

            value = double.Lerp(minValue, maxValue, newT);
            SelectWidget(widgetData);
            MouseLeftPressUsed = true;

        }
    }

    public void MainParagraph(string title, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber]
        int lineNumber = 0)
    {
        var widgetData = GetWidgetData(filePath, lineNumber);
        var pos = new Vec2(50, CanvasSize.Y - 250);
        var lh = 64;
        widgetData.RelPosition = pos;
        widgetData.Size = new Vec2(lh, lh);
        if (title.StartsWith("\r\n"))
        {
            title = title[2..];
        }
        Gizmos2D.AdvText(View.Camera2D, pos, lh, Color.White, title, 1);
    }

    public void ViewPanel(string viewname, Vec2 center, Vec2 size, double zoom = 1f, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber]
        int lineNumber = 0)
    {
       
    }

    public Color PanelBackgroundColor = new Color(1,0,0,1);
  
    public View GetView(WidgetData widget, Action<World>? load)
    {
        if (!presiViews.ContainsKey(widget))
        {
            var w = View.World.FlowExplainer.GetGlobalService<WorldManagerService>().NewWorld();
            load?.Invoke(w);
            w.Update();
            var v = new View(1, 1, w)
            {
                Controller = new PresiChildViewController(),
                AltClearColor = PanelBackgroundColor,
                Name = $"Presentation view",
            };
            presiViews.Add(widget, v);
        }

        var view = presiViews[widget];
        return view;
    }

    public int GetId(string filepath, int lineNumber)
    {
        if (filepath.StartsWith('#'))
            return filepath.GetHashCode();
        else
            return HashCode.Combine(filepath.GetHashCode(), lineNumber.GetHashCode());
    }

    public WidgetData GetWidgetData(string filepath, int linenumber)
    {
        var id = GetId(filepath, linenumber);
        if (!widgetsById.TryGetValue(id, out WidgetData w))
        {
            w = new WidgetData();
            widgetsById.Add(id, w);
        }
        w.TimeSinceLastFetch = 0;
        return w;
    }

    public void Refresh(PresentationService presentationService)
    {
        CanvasSize = presentationService.CanvasSize;
        CanvasRect = new Rect<Vec2>(Vec2.Zero, CanvasSize);
        if (View.IsMouseButtonDownLeft && !MouseLeftPressUsed)
            SelectedWidget = null;

        foreach (var w in widgetsById.Values)
        {
            w.TimeSinceLastFetch += presentationService.FlowExplainer.DeltaTime;
        }

        foreach (var view in presiViews.Values)
        {
            view.IsActive = false;
        }
        MouseLeftPressUsed = false;
    }
}