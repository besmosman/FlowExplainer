using ImGuiNET;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class DensityParticles3DVisualizer : WorldService
{
    public override string? Name => "Density 3D Spheres";
    public double Radius = .004;
    public bool ExtendBounds;

    public override void Initialize()
    {

    }

    public override void Draw(View view)
    {
        if (!view.Is3DCamera)
            return;

        foreach (ref var p in GetRequiredWorldService<DensityParticlesData>().Particles.AsSpan())
        {
            Gizmos.Instanced.RegisterSphere(p.Phase, Radius, Color.White.WithAlpha(1f));
        }
        
        if(ExtendBounds)
        foreach (ref var p in GetRequiredWorldService<DensityParticlesData>().Particles.AsSpan())
            if (p.Phase.X > 0.5)
                Gizmos.Instanced.RegisterSphere(p.Phase + new Vec3(-1, 0, 0), Radius, Color.White.WithAlpha(1f));
            else
                Gizmos.Instanced.RegisterSphere(p.Phase + new Vec3(1, 0, 0), Radius, Color.White.WithAlpha(1f));
        GL.Enable(EnableCap.DepthTest);
        Gizmos.Instanced.DrawSpheresLit(view.Camera);
        GL.Disable(EnableCap.DepthTest);
    }

    public override void DrawImGuiSettings()
    {
        ImGuiHelpers.Slider("Radius", ref Radius, 0, .01);
        ImGui.Checkbox("Extend", ref ExtendBounds);
        base.DrawImGuiSettings();
    }
}