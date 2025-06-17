using OpenTK.Graphics.OpenGL4;

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
        public Vec3 CameraOffset;
        public float CameraZoom = .5f;
        public View? CameraSync;
        
        public Vec2 RelativeMousePosition;
        public ICamera ScreenCamera => new ScreenCamera(RenderTarget.Size.ToNumerics());

        public bool IsOpen = true;
        public bool Is3DCamera = false;
        public bool Is2DCamera => !Is3DCamera;
        public Camera Camera = new();
        public Camera2D Camera2D = new();
        public World World;
        public readonly RenderTexture RenderTarget;
        public readonly RenderTexture PostProcessingTarget;
        public bool IsSelected;

        /// <summary>
        /// As in the target size, not the the current rendertarget size.
        /// </summary>
        public Vec2 TargetSize;
        public Vec2i Size => RenderTarget.Size;
        public int Width => RenderTarget.Size.X;
        public int Height => RenderTarget.Size.Y;

        public View(int w, int h, World world)
        {
            Id = viewsCreated++;
            Name = $"view {Id}";
            GL.GetInteger(GetPName.DrawFramebufferBinding, out int previouslyBound);
            RenderTarget = new RenderTexture(w, h, true, true);
            PostProcessingTarget = new RenderTexture(w, h, true, false);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, previouslyBound);
            World = world;
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