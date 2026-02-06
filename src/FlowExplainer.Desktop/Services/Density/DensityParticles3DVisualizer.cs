using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class DensityParticles3DVisualizer : WorldService
{
    public override string? Name => "Density 3D Spheres";
    public override void Initialize()
    {
        
    }
    
    public override void Draw(View view)
    {
        if(!view.Is3DCamera)
            return;
        
        foreach (ref var p in GetRequiredWorldService<DensityParticlesData>().Particles.AsSpan())
        {
            Gizmos.Instanced.RegisterSphere(p.Phase, .005f, Color.Red.WithAlpha(1f));
        }
        GL.Enable(EnableCap.DepthTest);
        Gizmos.Instanced.DrawSpheresLit(view.Camera);
        GL.Disable(EnableCap.DepthTest);
    }
}