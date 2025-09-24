using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;

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
            presiView.RelativeMousePosition = new Vec2(window.MousePosition.X, window.MousePosition.Y);
        }
        else
        {
            ImGUIViewRenderer.Render(presiView, FlowExplainer);
        }

        if (presiView.RelativeMousePosition.X >= 0 &&
            presiView.RelativeMousePosition.Y >= 0 &&
            presiView.RelativeMousePosition.X < presiView.Size.X &&
            presiView.RelativeMousePosition.Y < presiView.Size.Y)
        {
            var mousePos = CoordinatesConverter2D.ViewToWorld(presiView, presiView.RelativeMousePosition);
            presiView.MousePosition = mousePos;
            presiView.IsMouseButtonDownLeft = window.IsMouseButtonDown(MouseButton.Left);
            presiView.IsMouseButtonPressedLeft = window.IsMouseButtonPressed(MouseButton.Left);

            if (window.IsMouseButtonPressed(MouseButton.Right))
            {
                presiView.startCamPos = presiView.Camera2D.Position;
            }
        }
        presiView.ResizeToTargetSize();

        var pre = FlowExplainer.GetGlobalService<PresentationService>()!;
        pre.CurrentSlide.Presi = pre.Presi;

        presiView.RenderTarget.DrawTo(() =>
        {
            var clearColor = presiView.AltClearColor ?? Style.Current.BackgroundColor;
            GL.ClearColor(clearColor.R, clearColor.G, clearColor.B, clearColor.A);
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