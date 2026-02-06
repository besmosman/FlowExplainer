using ImGuiNET;

namespace FlowExplainer;

public class DensityParticlesData : WorldService
{
    public struct Particle
    {
        public Vec3 StartPhase;
        public Vec3 Phase;
        public double TimeAlive;
    }

    public Particle[] Particles;
    public int ParticleCount = 1000;

    public override string? Name => "Density Particles";

    public IVectorField<Vec3, Vec3> VelocityField;
    public double dt;

    public override void Initialize()
    {
        var TotalFlux = DataService.LoadedDataset.VectorFields["Total Flux"];
        var ConvectiveTemp = DataService.LoadedDataset.ScalerFields["Convective Temperature"];
        VelocityField = new ArbitraryField<Vec3, Vec3>(TotalFlux.Domain, x => TotalFlux.Evaluate(x).Up(ConvectiveTemp.Evaluate(x)));
        //VelocityField = new ArbitraryField<Vec3, Vec3>(TotalFlux.Domain, x => TotalFlux.Evaluate(x).Up(.1));
        Particles = new Particle[ParticleCount];
        var rect = VelocityField.Domain.RectBoundary;
        foreach (ref var p in Particles.AsSpan())
        {
            p.Phase = Utils.Random(rect);
        }
    }

    public override void Draw(View view)
    {

        var dt = -.01f;

        var rk4Steady = IIntegrator<Vec3, Vec3>.Rk4Steady;
        var rect = VelocityField.Domain.RectBoundary;
        var domainBounding = VelocityField.Domain.Bounding;
        Parallel.For(0, Particles.Length, i =>
        {
            ref var p = ref Particles[i];
            //p.Phase = domainBounding.Bound(rk4Steady.Integrate(VelocityField, p.Phase, dt));
            if (Random.Shared.NextSingle() > .999)
            {
                p.Phase = Utils.Random(rect);
                p.StartPhase = p.Phase;
                p.TimeAlive = 0;
            }
            p.Phase = domainBounding.Bound(rk4Steady.Integrate(VelocityField, p.Phase, dt));
            p.TimeAlive += float.Abs(dt);
            //p.Phase = domainBounding.Bound(p.Phase + VelocityField.Evaluate(p.Phase).XY.Up(0)*dt/5);
        });

        //if (!view.Is3DCamera)
        return;
        /*GL.BlendFuncSeparate(
            BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha,
            BlendingFactorSrc.One, BlendingFactorDest.One
        );*/



    }

    public override void DrawImGuiSettings()
    {
        ImGuiHelpers.SliderInt("Particle Count", ref ParticleCount, 0, 10000);
        if (ImGui.Button("Reset"))
        {
            Initialize();
        }
        base.DrawImGuiSettings();
    }
}