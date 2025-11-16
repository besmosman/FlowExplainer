using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FlowExplainer;

public static class ViewController2D
{

    public static void Update(View view, NativeWindow window )
    {
        if (view.Is2DCamera && view.IsSelected)
        {
            var mousePos = CoordinatesConverter2D.ViewToWorld(view, view.RelativeMousePosition);
            view.MousePosition = mousePos;
            if (window.IsMouseButtonPressed(MouseButton.Right))
            {
                view.lastClickPos = mousePos;
                view.startCamPos = view.Camera2D.Position;
            }

            if (window.IsMouseButtonDown(MouseButton.Right))
            {
                view.Camera2D.Position = view.startCamPos;
                var cur = CoordinatesConverter2D.ViewToWorld(view, view.RelativeMousePosition);
                view.Camera2D.Position = view.startCamPos - (view.lastClickPos - cur);
                //Gizmos2D.Rect(view.Camera2D, lastClickPos, cur, new Vec4(1, 1, 1, .2f));
            }

            if (window.MouseState.ScrollDelta.Y != 0)
            {
                view.Camera2D.Scale *= 1f + (window.MouseState.ScrollDelta.Y) * .02f;
            }
        }
    }
}