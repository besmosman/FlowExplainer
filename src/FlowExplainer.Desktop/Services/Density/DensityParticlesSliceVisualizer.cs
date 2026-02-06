using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class DensityParticlesSliceVisualizer : WorldService
{
    public override void Initialize()
    {

    }
    public double RenderRadius = .01f;

    public override void Draw(View view)
    {
        if (!view.Is2DCamera)
            return;
        GL.BlendFuncSeparate(
            BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha,
            BlendingFactorSrc.One, BlendingFactorDest.One
        );

        var pdata = GetRequiredWorldService<DensityParticlesData>();
        foreach (ref var p in pdata.Particles.AsSpan())
        {
            var time = p.Phase.Last;
            var distanceToTargetTime = double.Abs(time - 3.7f);
            var alpha = GetWeight(distanceToTargetTime) * double.Min(double.Abs(p.Phase.Last - p.StartPhase.Last) * 3, 1) * .2f;
            Gizmos2D.Instanced.RegisterCircle(p.Phase.XY, RenderRadius, Color.Red.WithAlpha(alpha));
        }
        Gizmos2D.Instanced.RenderCircles(view.Camera2D);
    }

    public double GetWeight(double dis)
    {
        double sigma = 0.04f;
        return (double)Math.Exp(-(dis * dis) / (2 * sigma * sigma));
    }
}