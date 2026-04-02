using ImGuiNET;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class DensityPathVisualizer : WorldService
{

    public override void Initialize()
    {

    }
    public override void Draw(View view)
    {
        var ConvectiveTemp = DataService.LoadedDataset.ScalerFields["Convective Temperature"];
        var vec = DataService.VectorField;
        var FluxField = new ArbitraryField<Vec3, Vec3>(vec.Domain, x => vec.Evaluate(x).Up(ConvectiveTemp.Evaluate(x)));

        var traj = IFlowOperatorSteady<Vec3>.Default.ComputeTrajectory(new Vec3(0.1, 0.25, DataService.SimulationTime), -15, FluxField);

        foreach (var (start, end) in traj.EnumerateSegments())
        {
            Gizmos.DrawLine(view, start, end, .004f, Color.Green);
        }
    }
}

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

        if (ExtendBounds)
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