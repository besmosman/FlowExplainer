using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class Slice3DVisualizer : WorldService
{
    public View SliceView;
    public override void Initialize()
    {
        SliceView = new View(1, 1, World);
        SliceView.TargetSize = new Vec2(1000, 500);
        SliceView.Camera2D.Position = default;
        SliceView.Camera2D.Scale = 10;
    }
    public override void Draw(View view)
    {
        if (!view.Is3DCamera)
            return;

        SliceView.Camera2D.Scale = 1000f;
        SliceView.Camera2D.Position = new Vec2(-.5, -.25);
        SliceView.ResizeToTargetSize();
        SliceView.RenderTarget.DrawTo(() =>
        {
            GL.ClearColor(.0f, .0f, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            foreach (var service in World.Services)
            {
                if (service is not Slice3DVisualizer && service.IsEnabled)
                {
                    service.Draw(SliceView);
                }
            }
        });
        double t = DataService.SimulationTime;
        GL.Enable(EnableCap.DepthTest);
        RenderTexture.Blit(SliceView.RenderTarget, SliceView.PostProcessingTarget);
        Gizmos.DrawTexturedQuadXY(view.Camera, SliceView.PostProcessingTarget, new Vec3(0, 0, t), new Vec2(1, .5f));
        //Gizmos2D.ImageCenteredInvertedY(view.Camera2D, SliceView.PostProcessingTarget, new Vec2(1.5, .5), new Vec2(1, .5f));
    }
}