using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class PresentationViewController : IViewController
{
    public void UpdateAndDraw(View presiView)
    {
        var FlowExplainer = presiView.World.FlowExplainer;
        var window = FlowExplainer.GetGlobalService<WindowService>()!.Window;
        var baseSize = FlowExplainer.GetGlobalService<PresentationService>()!.CanvasSize;
        presiView.Camera2D.Position = -baseSize / 2;

        var size = new Vec2(window.ClientSize.X, window.ClientSize.Y);
        if (presiView.IsFullScreen)
        {
            presiView.TargetSize = size;
        }
        else
        {
            ImGUIViewRenderer.Render(presiView, FlowExplainer);
        }

        presiView.ResizeToTargetSize();

        var pre = FlowExplainer.GetGlobalService<PresentationService>()!;
        pre.CurrentSlide.Presi = pre.Presi;

        presiView.RenderTarget.DrawTo(() =>
        {
            GL.ClearColor(presiView.ClearColor.R, presiView.ClearColor.G, presiView.ClearColor.B, presiView.ClearColor.A);
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
            pre.CurrentSlide.Draw();
        });

        RenderTexture.Blit(presiView.RenderTarget, presiView.PostProcessingTarget);


        if (presiView.IsFullScreen)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, (int)size.X, (int)size.Y);
            //   Gizmos2D.RectCenter(new ScreenCamera(size.RoundInt()), size / 2, size, FlowExplainer.GetGlobalService<WindowService>()!.ClearColor);
            GL.Disable(EnableCap.Blend);
            Gizmos2D.ImageCentered(new ScreenCamera(size.RoundInt()), presiView.PostProcessingTarget, size / 2, size);
            GL.Enable(EnableCap.Blend);
        }

        //presiView.Camera2D.Scale = 1;
        if (presiView.IsFullScreen || presiView.IsSelected)
        {
            if (window.MouseState.ScrollDelta.Y != 0)
            {
                presiView.Camera2D.Scale *= (1f + window.MouseState.ScrollDelta.Y * .01f);
            }
        }
    }
}