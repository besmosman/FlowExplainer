using OpenTK.Graphics.OpenGL4;
using System.Numerics;

namespace FlowExplainer
{
    public class View : IDisposable
    {
        private static int viewsCreated;
        public const int SuperSamplingLevel = 1;
        public readonly int Id;
        public string Name;

        //Camera settings, should be moved I think.
        public bool CameraLocked;
        public float CameraYaw;
        public float CameraPitch;
        public bool DemoCameraSwitch;
        public Vector3 CameraOffset;
        public float CameraZoom = .5f;
        public View? CameraSync;

        public Vector2 RelativeMousePosition;
        public ICamera ScreenCamera => new ScreenCamera(RenderTarget.Size.ToNumerics());

        public bool IsOpen = true;
        public bool Is2DCamera = false;
        public Camera Camera = new();
        public Camera2D Camera2D = new();
        public Visualisation Visualisation;
        public readonly RenderTexture RenderTarget;
        public readonly RenderTexture PostProcessingTarget;
        public bool IsSelected;

        /// <summary>
        /// As in the target size, not the the current rendertarget size.
        /// </summary>
        public Vector2 TargetSize;
        public OpenTK.Mathematics.Vector2i Size => RenderTarget.Size;
        public int Width => RenderTarget.Size.X;
        public int Height => RenderTarget.Size.Y;

        public View(int w, int h, Visualisation visualisation)
        {
            Id = viewsCreated++;
            Name = $"view {Id}";
            GL.GetInteger(GetPName.DrawFramebufferBinding, out int previouslyBound);
            RenderTarget = new RenderTexture(w, h, true, true);
            PostProcessingTarget = new RenderTexture(w, h, true, false);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, previouslyBound);
            Visualisation = visualisation;
        }

        public void ResizeToTargetSize()
        {
            int targetWidth = SuperSamplingLevel * (int)TargetSize.X;
            int targetHeight = SuperSamplingLevel * (int)TargetSize.Y;

            if (RenderTarget.Size.X == targetWidth && RenderTarget.Size.Y == targetHeight)
                return; // unnecessary call...

            RenderTarget.Resize(targetWidth, targetHeight);
            PostProcessingTarget.Resize(targetWidth, targetHeight);
            Camera2D.RenderTargetSize = TargetSize;
            Camera.RenderTargetSize = TargetSize;
        }

        public void Dispose()
        {
            RenderTarget.Dispose();
            GC.SuppressFinalize(this);
        }
        
    }
}