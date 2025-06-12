using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FlowExplainer;

public class ViewController2D : WorldService
{
    public override void Initialize()
    {
    }

    private Vec2 lastClickPos = Vec2.Zero;
    private Vec2 startCamPos = Vec2.Zero;

    public override void Draw(RenderTexture rendertarget, View view)
    {
        var window = GetRequiredGlobalService<WindowService>().Window;
        if (window.IsMouseButtonPressed(MouseButton.Right))
        {
            lastClickPos = CoordinatesConverter2D.ViewToWorld(view, view.RelativeMousePosition);;
            startCamPos  = view.Camera2D.Position;
        }

        if (window.IsMouseButtonDown(MouseButton.Right))
        {
            view.Camera2D.Position = startCamPos;
            var cur = CoordinatesConverter2D.ViewToWorld(view, view.RelativeMousePosition);
            view.Camera2D.Position = startCamPos - (lastClickPos - cur);
            //Gizmos2D.Rect(view.Camera2D, lastClickPos, cur, new Vec4(1, 1, 1, .2f));
        }
        Logger.LogDebug(view.Camera2D.Position.ToString());

        if (window.MouseState.ScrollDelta.Y != 0)
        {
            view.Camera2D.Scale *= 1f + (window.MouseState.ScrollDelta.Y)*.02f;
        }
    }
}