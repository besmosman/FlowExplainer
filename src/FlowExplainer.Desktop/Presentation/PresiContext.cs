using System.Numerics;
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
        public bool Dropdown;
        public Vec2 RenderMin;
        public Vec2 RenderMax;
        public double TimeSinceLastMovement;


        public Vec2 LastTargetPosition;
        public Vec2 TargetPosition;
        public double AnimSpeed = 2;
        public double AnimT => Math.Min(TimeSinceLastMovement * 1, 1);

        public void UpdateTransform(Vec2 position, Vec2 size)
        {
            if (TargetPosition != position)
            {
                LastTargetPosition = RelPosition;
                TargetPosition = position;
                TimeSinceLastMovement = 0;
            }
            Size = size;
        }
    }


    public void Text(string title, Vec2 relPos, double lh, bool centered, Color color, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber]
        int lineNumber = 0)
    {
        
        var widgetData = GetWidgetData(filePath, lineNumber);
        widgetData.UpdateTransform(relPos, new Vec2(lh, lh));
        var p = widgetData.RelPosition;
        p = new Vec2(double.Sin(FlowExplainer.Time.TotalSeconds*4)/4 + .4f, .4f);
        //Gizmos2D.Rect(View.Camera2D, RelToSceen(p),RelToSceen(p+widgetData.Size), new Vec4(1));
        Gizmos2D.AdvText(View.Camera2D, RelToSceen(widgetData.RelPosition), CanvasRect.FromRelative(new Vec2(lh, lh)).X, color, title, 1, centered);
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


    public void DropdownEnum<T>(string name, ref T value, Vec2 relCenter,
        [System.Runtime.CompilerServices.CallerFilePath]
        string filePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber]
        int lineNumber = 0) where T : struct, Enum
    {
        var widgetData = GetWidgetData(filePath, lineNumber);
        widgetData.RelPosition = relCenter;
        var lh = .05;
        var th = lh * .55;

        var entries = Enum.GetValues<T>();
        widgetData.Size.X = .2;
        widgetData.Size.Y = widgetData.Dropdown ? lh * entries.Length : lh;
        if (widgetData.Dropdown)
            relCenter += new Vec2(0, -lh * (entries.Length - 1) / 2.0);
        var center = RelToSceen(relCenter);
        var size = RelToSceen(widgetData.Size);
        Gizmos2D.RectCenter(View.Camera2D, center, size + new Vec2(4, 4), Color.Grey(1f));
        Gizmos2D.RectCenter(View.Camera2D, center, size, Color.Grey(.0f));
        var rect = new Rect<Vec2>(center - size / 2, center + size / 2);

        if (widgetData.Dropdown)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                Gizmos2D.Text(View.Camera2D, RelToSceen(new Vec2(widgetData.RelPosition.X, widgetData.RelPosition.Y - lh * i)), RelToSceen(th), Color.White, Enum.GetName(entry), centered: true);
            }
        }
        else
        {
            Gizmos2D.Text(View.Camera2D, center, RelToSceen(th), Color.White, Enum.GetName(value), centered: true);
        }

        if (View.IsMouseButtonPressedLeft)
        {
            if (rect.Contains(View.MousePosition))
            {
                if (!widgetData.Dropdown)
                    widgetData.Dropdown = true;
                else
                {
                    var index = entries.Length - 1 - (int)double.Floor(rect.ToRelative(View.MousePosition).Y * entries.Length);
                    index = int.Clamp(index, 0, entries.Length);
                    value = entries[index];

                    widgetData.Dropdown = false;
                }
            }
            else
            {
                widgetData.Dropdown = false;
            }
        }

        if (widgetData.Dropdown)
        {
        }
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
        Gizmos2D.RectCenter(View.Camera2D, center, size, Color.Grey(.8f));
        if (value)
            Gizmos2D.RectCenter(View.Camera2D, center, size * .8f, Color.Grey(.4f));
        Gizmos2D.Text(View.Camera2D, center + new Vec2(50, 0), 48, Color.Black, name);
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
        double width = RelToSceen(relWidth) + 20;

        var left = RelToSceen(new Vec2(widgetData.RelPosition.X - widgetData.Size.X / 2f, widgetData.RelPosition.Y));
        var right = RelToSceen(new Vec2(widgetData.RelPosition.X + widgetData.Size.X / 2f, widgetData.RelPosition.Y));
        Gizmos2D.Line(View.Camera2D, left, right, Color.Black, 10);
        var t = (value - minValue) / (maxValue - minValue);
        t = double.Clamp(t, 0, 1);
        Gizmos2D.Circle(View.Camera2D, Utils.Lerp(left, right, t), Color.Black, 20);
        Gizmos2D.AdvText(View.Camera2D, center + new Vec2(0, -40), 48, Color.Black, name + " = " + value.ToString("N2"), centered: true);
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

    public Color PanelBackgroundColor = new Color(1, 0, 0, 1);

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
            w.TimeSinceLastMovement += presentationService.FlowExplainer.DeltaTime;
        }

        foreach (var widget in widgetsById.Values)
        {
            widget.RelPosition = Utils.Lerp(widget.LastTargetPosition, widget.TargetPosition, widget.AnimT);
        }

        foreach (var view in presiViews)
        {
            var subRenderMin = view.Key.RenderMin;
            var subRenderMax = view.Key.RenderMax;
            var subRenderRect = new Rect<Vec2>(subRenderMin, subRenderMax);
            var mainwindowSize = presentationService.PresiView.Size.ToVec2();
            var mainwindowCoord = presentationService.PresiView.RelativeMousePosition;
            var mouseRelInParent = mainwindowCoord / mainwindowSize;

            /*var subwindowCenter = RelToSceen(view.Key.RelPosition);
            var subwindowSize = RelToSceen(view.Key.Size);
            subwindowSize.Y = subwindowSize.X * (view.Key.Size / view.Key.Size.X).Y;
            var subwindowPos = subwindowCenter /*- subwindowSize / 2#1#;
            var relSubPos = subwindowPos / subwindowSize;*/

            var subSize = subRenderMax - subRenderMin;
            var mouseRelInSub = (mouseRelInParent - subRenderMin) / subSize;

            var localPosPixels = mainwindowCoord - subRenderMin;
            var localPosNormalized = mainwindowCoord - subRenderMin;
            Logger.LogDebug((subRenderRect.ToRelative(mainwindowCoord) / view.Key.Size).ToString());
            view.Value.RelativeMousePosition = subRenderRect.ToRelative(mainwindowCoord + new Vec2(0, -100)) / new Vec2(.69f, .5f) * view.Value.Size.ToVec2();

            // Logger.LogDebug(view.Value.RelativeMousePosition.ToString());
            /*var subRect = new Rect<Vec2>(RelToSceen(view.Key.RelPosition) - RelToSceen(view.Key.Size) / 2, RelToSceen(view.Key.Size));
var localMousePos = mainwindowCoord - subwindowPos;
            var localPixelPos = mainwindowCoord - subwindowPos;

            var subrelpos = mouseRelInParent - subwindowSize / 2;
            subrelpos /= CanvasSize;
            var size = view.Value.Size.ToVec2();
            view.Value.RelativeMousePosition = (mainwindowCoord) / mainwindowSize * size;
            var relInSub = (mouseRelInParent) / view.Key.Size;
            view.Value.RelativeMousePosition = subRect.FromRelative(relInSub);*/

            view.Value.IsActive = false;
        }

        MouseLeftPressUsed = false;
    }
}