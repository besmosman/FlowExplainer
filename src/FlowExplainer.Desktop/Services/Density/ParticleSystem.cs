using System.Collections.Concurrent;
using ImGuiNET;

namespace FlowExplainer;

public class ParticleSystem : WorldService
{
    public struct Particle
    {
        public double LastTimeAlive;
        public double TimeAlive;
        public Vec3 Phase;
        public Vec3 LastPhase;
        public Vec3 LastLastPhase;
    }

    public enum IntegrationMode
    {
        Forward,
        Backwords,
        Mixed,
    }

    public override string? Name => "Particles System";
    public ResizableStructArray<Particle> Particles = new(8096);
    public double ReseedRate = 0.2;
    public Rect<Vec3> SeedRect;
    public double dFictitious = .05;
    public IntegrationMode integrationMode;
    private int seedCounter;
    public IVectorField<Vec3, Vec3> TransportField;



    public override void Initialize()
    {
        var ConvectiveTemp = DataService.LoadedDataset.ScalerFields["Convective Temperature"];
        Array.Clear(Particles.Array);
        SeedRect = ConvectiveTemp.Domain.RectBoundary;
        foreach (ref var p in Particles.AsSpan())
        {
            Reseed(ref p);
        }
        var TotalFlux = DataService.LoadedDataset.VectorFields["Total Flux"];
        TransportField = new ArbitraryField<Vec3, Vec3>(new RectDomain<Vec3>(TotalFlux.Domain.RectBoundary),
            x => TotalFlux.Evaluate(x).Up(double.Abs(ConvectiveTemp.Evaluate(x))));


    }

    public override void PreDraw()
    {
        var ConvectiveTemp = DataService.LoadedDataset.ScalerFields["Convective Temperature"];
        var TotalFlux = DataService.LoadedDataset.VectorFields["Total Flux"];

        TransportField = new ArbitraryField<Vec3, Vec3>(new RectDomain<Vec3>(TotalFlux.Domain.RectBoundary),
            x => TotalFlux.Evaluate(x).Up(ConvectiveTemp.Evaluate(x)));

        /*
        TransportField = new ArbitraryField<Vec3, Vec3>(new RectDomain<Vec3>(TotalFlux.Domain.RectBoundary),
            x =>
            {
                double d = ConvectiveTemp.Evaluate(x);
                if (double.Abs(d) < 0.001)
                    d =double.Sign(d) * 0.001;
                return (TotalFlux.Evaluate(x) / d).Up(1);
            });*/
        var bounds = TransportField.Domain.Bounding;

        //For the forced periodic definition
        //var boundsZ = ConvectiveTemp.Domain.RectBoundary.Size.Z > 1 ? BoundaryType.Fixed : BoundaryType.Periodic;
        //bounds = BoundingFunctions.Build([BoundaryType.Periodic, BoundaryType.Fixed, boundsZ], ConvectiveTemp.Domain.RectBoundary);

        var rk4 = IIntegrator<Vec3, Vec3>.Rk4Steady;

        Func<int, double> integrationDirection = integrationMode switch
        {
            IntegrationMode.Forward => i => 1,
            IntegrationMode.Backwords => i => -1,
            IntegrationMode.Mixed => i => i % 2 == 0 ? 1 : -1,
            _ => throw new ArgumentOutOfRangeException(),
        };

        if(Particles.Length > 0)
        Parallel.ForEach(Partitioner.Create(0, Particles.Length), range =>
        {
            for (int i = range.Item1; i < range.Item2; i++)
            {
                ref var p = ref Particles[i];
                p.LastLastPhase = p.LastPhase;
                p.LastPhase = p.Phase;
                var dir = integrationDirection(i);
                p.Phase = rk4.Integrate(TransportField, p.Phase, dFictitious * dir);
                p.Phase = bounds.Bound(p.Phase);
                p.LastTimeAlive = p.TimeAlive;
                p.TimeAlive += double.Abs(dFictitious);
            }

            for (int i = range.Item1; i < range.Item2; i++)
            {
                ref var p = ref Particles[i];
                if (Random.Shared.NextSingle() < ReseedRate * double.Abs(dFictitious))
                    Reseed(ref p);
            }
        });
        base.PreDraw();
    }

    public override void Draw(View view)
    {
        view.CameraOffset = new Vec3(-1.0 / 2, -.5 / 2, DataService.SimulationTime);
    }

    private IVectorField<Vec3, T> PerodicExtend<T>(IVectorField<Vec3, T> vec)
    {
        var rect = vec.Domain.RectBoundary;
        rect.Min.X -= .5f;
        rect.Max.X += .5f;
        return new ArbitraryField<Vec3, T>(new RectDomain<Vec3>(rect), x =>
        {
            if (x.X < 0)
                x.X = 1 - x.X;
            if (x.X > 1)
                x.X -= 1;
            return vec.Evaluate(x);
        });
    }

    private void Reseed(ref Particle p)
    {
        var spacetime = Utils.Halton3(SeedRect, ++seedCounter);
        p.Phase = spacetime;
        p.LastPhase = p.Phase;
        p.LastLastPhase = p.LastPhase;
        p.TimeAlive = 0;
        p.LastTimeAlive = 0;
    }

    public int SlicingAxis = 2;
    public string SlicingAxisName()
    {
        switch (SlicingAxis)
        {
            case 0: return "X";
            case 1: return "Y";
            case 2: return "Z";
        }
        throw new NotImplementedException();
    }
    public override void DrawImGuiSettings()
    {
        int t = Particles.Length;

        ImGuiHelpers.Slider("Particle Count", ref t, 0, 10000);
        Particles.ResizeIfNeeded(t);
        ImGuiHelpers.Slider("Δξ", ref dFictitious, 0, .1);
        if (ImGuiHelpers.EnumCombo("Integration Mode", ref integrationMode))
        {
            //  Initialize();
        }

        var min = SeedRect.Min[SlicingAxis];
        var max = SeedRect.Max[SlicingAxis];

        if (TransportField != null)
        {
            var domainRect = TransportField.Domain.RectBoundary;
            ImGuiHelpers.Slider($"Seed {SlicingAxisName()} Min", ref min, domainRect.Min[SlicingAxis], domainRect.Max[SlicingAxis]);
            ImGuiHelpers.Slider($"Seed {SlicingAxisName()} Max", ref max, domainRect.Min[SlicingAxis], domainRect.Max[SlicingAxis]);
            SeedRect.Min = domainRect.Min;
            SeedRect.Max = domainRect.Max;
            SeedRect.Min[SlicingAxis] = min;
            SeedRect.Max[SlicingAxis] = max;
        }

        ImGuiHelpers.Slider("Reseed Rate", ref ReseedRate, 0, 1);
        ImGuiHelpers.Slider("t0", ref SeedRect.Min.X, 0, DataService.ScalerField.Domain.RectBoundary.Max.Z);
        ImGuiHelpers.Slider("t1", ref SeedRect.Max.X, SeedRect.Min.X, DataService.ScalerField.Domain.RectBoundary.Max.Z);
        if (ImGui.Button("Reset"))
        {
            Initialize();
        }
        base.DrawImGuiSettings();
    }
}