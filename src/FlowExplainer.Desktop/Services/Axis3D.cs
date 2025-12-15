using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class Axis3D : WorldService
{
    public override void Initialize()
    {

    }
    public override void Draw(RenderTexture rendertarget, View view)
    {
        if (!view.Is2DCamera)
        {
            var dat = GetRequiredWorldService<DataService>();
            var domain = dat.VectorField.Domain.RectBoundary;

            var th = 0.02f;
            GL.Enable(EnableCap.DepthTest);
            Gizmos.DrawLine(view, domain.Min, new Vec3(domain.Max.X, domain.Min.Y, domain.Min.Z), th, new Color(1, 0, 0, 1));
            Gizmos.DrawLine(view, domain.Min, new Vec3(domain.Min.X, domain.Max.Y, domain.Min.Z), th, new Color(0, 1, 0, 1));
            Gizmos.DrawLine(view, domain.Min, new Vec3(domain.Min.X, domain.Min.Y, domain.Max.Z), th, new Color(0, 0, 1, 1));
            GL.Disable(EnableCap.DepthTest);
        }

    }
}