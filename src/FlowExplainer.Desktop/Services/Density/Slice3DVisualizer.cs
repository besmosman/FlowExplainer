using System.Numerics;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class Slice3DVisualizer : WorldService
{
    public View SliceView;

    public override string? Name => "Slice";
    public override string? Description => "View 2D visualizations in 3D as a quad in spacetime space";
    public override string? CategoryName => "General";
    public override bool Category3D => true;

    public override void Initialize()
    {
        SliceView = new View(1, 1, World);
        SliceView.Camera2D.Position = default;
        SliceView.Camera2D.Scale = 10;
    }

    public override void Draw(View view)
    {
        if (!view.Is3DCamera)
            return;
        var spaceTime = GetRequiredWorldService<DensityPathStructuresSpaceTime>();
        var size = spaceTime.GetRelevant2DPos(DataService.VectorField.Domain.RectBoundary.Size);

        SliceView.Camera2D.Scale = 2000f;
        SliceView.Camera2D.Position = -size/2;
        SliceView.TargetSize = size*2000;
        SliceView.ResizeToTargetSize();

        SliceView.RenderTarget.DrawTo(() =>
        {
            //GL.ClearColor(.0f, .0f, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Disable(EnableCap.DepthTest);
            foreach (var service in World.Services)
            {
                if (service is not Slice3DVisualizer && service.IsEnabled)
                {
                    service.Draw(SliceView);
                }
            }
        });
        double t = DataService.SimulationTime;
        RenderTexture.Blit(SliceView.RenderTarget, SliceView.PostProcessingTarget);
        var p = new Vec3();
        p[spaceTime.SlicingAxis] = spaceTime.SliceValue;
        GL.Enable(EnableCap.DepthTest);

        Gizmos.texturedMat.Use();
        var pos = p;
        Gizmos.texturedMat.SetUniform("view", view.Camera.GetViewMatrix());
        Gizmos.texturedMat.SetUniform("projection", view.Camera.GetProjectionMatrix());
        var rot = Matrix4x4.Identity;
        if (spaceTime.SlicingAxis == 1)
        {
            rot = Matrix4x4.CreateRotationX(-float.Pi/2) * Matrix4x4.CreateRotationY(-float.Pi/2);
            //size = new Vec2(size.Y, size.X);
        }
        
        if (spaceTime.SlicingAxis == 0)
        {
            rot = Matrix4x4.CreateRotationY(-float.Pi/2) ;
            //size = new Vec2(size.Y, size.X);
        }
        Gizmos.texturedMat.SetUniform("model", 
                                                Matrix4x4.CreateScale((float)size.X, (float)size.Y, 1) * rot  * 
                                               Matrix4x4.CreateTranslation(pos.ToNumerics()));
        Gizmos.texturedMat.SetUniform("tint", Color.White);
        Gizmos.texturedMat.SetUniform("mainTex", SliceView.PostProcessingTarget);
        Gizmos.Quad.Draw();

        //Gizmos.DrawTexturedQuadXY(view.Camera, SliceView.PostProcessingTarget, p, DataService.VectorField.Domain.RectBoundary.Size.XY);
    }

    public override void DrawImGuiSettings()
    {
        if (ImGui.Button("screen"))
        {
            SliceView.PostProcessingTarget.SaveToFile("test.png", SliceView.PostProcessingTarget.Size, 1);
        }
        base.DrawImGuiSettings();
    }
}