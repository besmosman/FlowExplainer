using System.Collections.Concurrent;
using ImGuiNET;

namespace FlowExplainer;

public class DensityParticlesData : WorldService
{
    public struct Particle
    {
        public double Weight;
        public double LastTimeAlive;
        public double TimeAlive;
        public Vec3 Phase;
        public Vec3 LastPhase;
        public Vec3 LastLastPhase;
        public double HeatingCoolingAccumelation;
    }

    public ResizableStructArray<Particle> Particles;
    public double ReseedRate = 0.2;
    public override string? Name => "Density Particles";
    public Rect<Vec1> SeedInterval = new Rect<Vec1>(0, 3);
    //public IVectorField<Vec3, Vec3> VelocityField;
    public IVectorField<Vec3, double> SourceField;
    public double dFicticious;
    public bool Reversed = false;

    private int seedCounter;
    public override void Initialize()
    {
        var ConvectiveTemp = DataService.LoadedDataset.ScalerFields["Convective Temperature"];
        SourceField = DataService.LoadedDataset.ScalerFields["Physical Source"];
        if (Particles == null)
            Particles = new(1);
        Array.Clear(Particles.Array);
        var rect = ConvectiveTemp.Domain.RectBoundary;
        rect = new Rect<Vec3>(rect.Min.XY.Up(SeedInterval.Min), rect.Max.XY.Up(SeedInterval.Max));

        foreach (ref var p in Particles.AsSpan())
        {
            Reseed(ref p, SourceField, rect);

        }
    }

    public override void PreDraw()
    {
      
        var ConvectiveTemp = DataService.LoadedDataset.ScalerFields["Convective Temperature"];
        var vec = DataService.VectorField;
        var FluxField = new ArbitraryField<Vec3, Vec3>(new RectDomain<Vec3>(vec.Domain.RectBoundary),
            x => { return vec.Evaluate(x).Up(double.Abs(ConvectiveTemp.Evaluate(x))); });
        var boundsZ = ConvectiveTemp.Domain.RectBoundary.Size.Z > 1 ? BoundaryType.Fixed : BoundaryType.Periodic;
        var bounds = BoundingFunctions.Build([BoundaryType.Periodic, BoundaryType.Fixed, boundsZ], ConvectiveTemp.Domain.RectBoundary);

        var rk4 = IIntegrator<Vec3, Vec3>.Rk4Steady;
        var seed = ConvectiveTemp.Domain.RectBoundary;
        var domainBounding = bounds;

        var targetDt = dFicticious;
        //var eps = 0.000000001;
        var sliceT = DataService.SimulationTime;
        seed = new Rect<Vec3>(seed.Min.XY.Up(SeedInterval.Min), seed.Max.XY.Up(SeedInterval.Max));
        var dtFicticious = dFicticious * (Reversed ? -1 : 1);

        if (Particles.Length > 0)
            Parallel.ForEach(Partitioner.Create(0, Particles.Length), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    ref var p = ref Particles[i];
                    //var dtFicticious = targetDt / double.Max(double.Abs(ConvectiveTemp.Evaluate(p.Phase)), eps);
                    p.LastLastPhase = p.LastPhase;
                    p.LastPhase = p.Phase;
                    p.Phase = rk4.Integrate(FluxField, p.Phase, dtFicticious);
                    p.HeatingCoolingAccumelation += (p.Phase.Z - p.LastPhase.Z);
                    p.Phase = domainBounding.Bound(p.Phase);
                    p.LastTimeAlive = p.TimeAlive;
                    p.TimeAlive += double.Abs(dtFicticious);
                }

                for (int i = range.Item1; i < range.Item2; i++)
                {
                    ref var p = ref Particles[i];

                    if (Random.Shared.NextSingle() < ReseedRate * double.Abs(dtFicticious) /*||
                        (!Reversed && p.Phase.Z > sliceT) ||
                        (Reversed && p.Phase.Z < sliceT)*/)
                        Reseed(ref p, SourceField, seed);
                }
            });
        base.PreDraw();
    }

    public override void Draw(View view)
    {
        view.CameraOffset = new Vec3(-0.5, -0.25, DataService.SimulationTime);
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

    private void Reseed(ref Particle p, IVectorField<Vec3, double> sourceField, Rect<Vec3> rect)
    {
        var spacetime = Utils.Halton3(rect, ++seedCounter);
        p.Phase = spacetime;
        p.LastPhase = p.Phase;
        p.LastLastPhase = p.LastPhase;
        p.Weight = sourceField.Evaluate(spacetime);
        p.TimeAlive = 0;
        p.LastTimeAlive = 0;
    }

    public override void DrawImGuiSettings()
    {
        int t = Particles.Length;
        ImGuiHelpers.Slider("Particle Count", ref t, 0, 10000);
        Particles.ResizeIfNeeded(t);
        ImGuiHelpers.Slider("Δξ", ref dFicticious, 0, .1);
        if (ImGui.Checkbox("Backwords Integration", ref Reversed))
        {
            Initialize();
        }
        ImGuiHelpers.Slider("Reseed Rate", ref ReseedRate, 0, 1);
        ImGuiHelpers.Slider("t0", ref SeedInterval.Min.X, 0, DataService.ScalerField.Domain.RectBoundary.Max.Z);
        ImGuiHelpers.Slider("t1", ref SeedInterval.Max.X, SeedInterval.Min.X, DataService.ScalerField.Domain.RectBoundary.Max.Z);
        if (ImGui.Button("Reset"))
        {
            Initialize();
        }
        base.DrawImGuiSettings();
    }
}