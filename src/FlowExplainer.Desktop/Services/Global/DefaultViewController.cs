using System.Numerics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FlowExplainer;

public class PresiChildViewController : IViewController
{
    public void UpdateAndDraw(View view)
    {
        var window = view.World.FlowExplainer.GetGlobalService<WindowService>()?.Window;
        var presi = view.World.FlowExplainer.GetGlobalService<PresentationService>()!.Presi;
        view.IsSelected = presi.SelectedWidget?.ConnectedObject == view;
        ViewController2D.Update(view, window);
        DefaultViewController.Update3DCamera(view, window);
        view.ResizeToTargetSize();
        view.World.Draw(view);
    }
}

public class DefaultViewController : IViewController
{
    public void UpdateAndDraw(View view)
    {
        var window = view.World.FlowExplainer.GetGlobalService<WindowService>()?.Window;
        if (window != null)
        {
            Update3DCamera(view, window);
            ViewController2D.Update(view, window);
            ImGUIViewRenderer.Render(view, view.World.FlowExplainer);
            view.World.Draw(view);
        }
    }

    public static void Update3DCamera(View view, NativeWindow window)
    {
        var nt = view.World.FlowExplainer;

        if (view.IsSelected && !view.CameraLocked && view.CameraSync == null && view.Is3DCamera)
        {
            if (window.IsMouseButtonDown(MouseButton.Left))
            {
                view.CameraYaw += window.MouseState.Delta.X * 0.005f;
                view.CameraPitch -= window.MouseState.Delta.Y * 0.005f;
            }


            if (window.IsMouseButtonDown(MouseButton.Right))
            {
                view.CameraOffset -= new Vec3(window.MouseState.Delta.X * 0.15f / view.CameraZoom, window.MouseState.Delta.Y * 0.15f / view.CameraZoom, 0);
            }

            view.CameraZoom *= (1f + window.MouseState.ScrollDelta.Y * .03f);
            view.CameraZoom = double.Clamp(view.CameraZoom, 0.1f, 2000);
        }

        var matrix = Matrix4x4.CreateTranslation((float)view.CameraOffset.X, (float)view.CameraOffset.Y, (float)view.CameraOffset.Z) *
                     Matrix4x4.CreateRotationY((float)view.CameraYaw) *
                     Matrix4x4.CreateRotationX((float)view.CameraPitch) *
                     Matrix4x4.CreateLookAt(new Vec3(0, 100 / view.CameraZoom, 0), Vec3.Zero, Vec3.UnitZ);

        Matrix4x4.Decompose(matrix, out var _, out var r, out var p);

        view.Camera.Rotation = r;
        view.Camera.Position = (Vec3)p;
        if (view.CameraSync != null)
            view.Camera = view.CameraSync.Camera;
    }
}