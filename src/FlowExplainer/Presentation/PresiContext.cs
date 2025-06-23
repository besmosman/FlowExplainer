using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class PresiContext
{
    public View View = null!;
    private Dictionary<int, WidgetData> widgetsById = new();

    private Dictionary<string, View> presiViewsByName = new();


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
        Gizmos2D.Text(View.ScreenCamera, pos, lh, color, title, 1, centered);
    }


    public void ViewPanel(string viewname, Vec2 center, Vec2 size, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber]
        int lineNumber = 0)
    {
        GL.Enable(EnableCap.Blend);
        var view = GetView(viewname);
        view.Camera2D.Position = -new Vec2(1, .5f)/2;
        view.Camera2D.Scale = view.PostProcessingTarget.Size.X/1.4f;
        view.TargetSize = size;
        var ratio = view.PostProcessingTarget.Size.ToVec2();
        ratio /= ratio.X;
        
        Gizmos2D.ImageCenteredInvertedY(View.Camera2D, view.PostProcessingTarget, center, size);
    }

    private View GetView(string viewname)
    {
        if (!presiViewsByName.ContainsKey(viewname))
        {
            var world1 = View.World.FlowExplainer.GetGlobalService<WorldManagerService>()!.Worlds[0];
            presiViewsByName.Add(viewname, new View(1, 1, world1));
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
    }
}