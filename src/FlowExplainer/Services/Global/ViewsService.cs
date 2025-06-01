using System.Numerics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FlowExplainer
{
    public class ViewsService : GlobalService
    {
        public List<View> Views = new();

        public override void Draw()
        {
            foreach (var view in Views)
            {
                UpdateView(view);
                ImGUIViewRenderer.Render(view, FlowExplainer);
            }

            for (int i = Views.Count - 1; i >= 0; i--)
            {
                if (!Views[i].IsOpen)
                    Views.RemoveAt(i);
            }
        }

        public void NewView()
        {
            Views.Add(new View(1, 1, GetRequiredGlobalService<VisualisationManagerService>().Visualisations[0]));
            Views.Last().CameraOffset = new Vec3(0, -.004f, .02f);
            Views.Last().CameraOffset = new Vec3(0, 0, 0);
            Views.Last().CameraZoom = 500;
            Views[Views.Count-1].Camera2D.Scale = 14f;
            
        }

        public void UpdateView(View view)
        {
            if (!TryGetGlobalService<WindowService>(out var windowService))
                return;

            var window = windowService.Window;


            var nt = view.Visualisation.FlowExplainer;

            if (view.IsSelected && !view.CameraLocked && view.CameraSync == null)
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
                view.CameraZoom = float.Clamp(view.CameraZoom, 0.1f, 2000);
            }

            var matrix = Matrix4x4.CreateTranslation(view.CameraOffset.X, view.CameraOffset.Z, view.CameraOffset.Y) *
                         Matrix4x4.CreateRotationZ(view.CameraYaw) *
                         Matrix4x4.CreateRotationX(view.CameraPitch) *
                         Matrix4x4.CreateLookAt(new Vec3(0, 100 / view.CameraZoom, 0), Vec3.Zero, Vec3.UnitZ);

            Matrix4x4.Decompose(matrix, out var _, out var r, out var p);

            view.Camera.Rotation = r;
            view.Camera.Position = (Vec3)p;
            if (view.CameraSync != null)
                view.Camera = view.CameraSync.Camera;
        }

        public override void Initialize()
        {
            NewView();
        }
    }
}