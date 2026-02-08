using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class SpatialPartitioner<T, Vec, Veci> where Veci : IVec<Veci, int>
    where Vec : IVec<Vec, double>, IVecIntegerEquivalent<Veci>
{
    public RegularGrid<Veci, List<int>?> Partioner;

    public void Register(T[] entries, Func<T[], int, Vec> getPos)
    {
        for (int i = 0; i < entries.Length; i++)
        {
            var pos = getPos(entries, i);
            var cell = pos.FloorInt();
            ref var p = ref Partioner[cell];
            if (p == null)
                p = [];
            p.Add(i);
        }
    }
}

public class StochasticVisualization : WorldService, IAxisTitle
{
    public struct Particle
    {
        public double Timealive;
        public double T;
        public Vec2 Position;
    }

    public Particle[] Particles;
    public int Count = 10000;

    public IVectorField<Vec3, Vec2>? AltVectorfield;
    public ColorGradient? AltGradient;
    public double dt = 0.01;
    public double RenderRadius = .008f;

    public double alpha = .1f;
    public bool reverse;
    public bool fadeIn = true;
    public double ReseedChance = .01f;
    public bool ColorByGradient = false;
    public Color Color = Color.White;

    //public bool Extraction;
    //public PointSpatialPartitioner2D<Particle> Partitioner2D = new PointSpatialPartitioner2D<Particle>(.1f);

    public enum Mode
    {
        Instantaneous,
        InstantaneousMerged,
        TimeIntegration,
    }

    public Mode mode;

    public override string? Name => "Stochastic Structures";
    public override string? CategoryName => "Structure";
    public override string? Description => "Visualize attracting/repelling regions using particle advection transparent rendering";

    public override void Initialize()
    {
        var dat = GetRequiredWorldService<DataService>();

        Particles = new Particle[Count];
        Reset();
    }


    static double RandomNormal()
    {
        double u1 = 1f - Random.Shared.NextSingle();
        double u2 = 1f - Random.Shared.NextSingle();
        return Math.Sqrt(-2f * Math.Log(u1)) * Math.Cos(2f * Math.PI * u2);
    }

    public Vec2 RandomWienerVector()
    {
        return new Vec2(RandomNormal(), RandomNormal());
    }

    public void Step(double dt)
    {
        if (Count != Particles.Length)
            Reset();

        var dat = GetRequiredWorldService<DataService>();
        var vectorfield = AltVectorfield ?? dat.VectorField;
        var advection = vectorfield;
        if (reverse)
            advection = new ArbitraryField<Vec3, Vec2>(vectorfield.Domain, p => -vectorfield.Evaluate(p));
        var Pe = 1009;

        double sqrt = double.Sqrt((2 * dt) / Pe);
        var domainRectBoundary = vectorfield.Domain.RectBoundary;
        var rk4 = IIntegrator<Vec3, Vec2>.Rk4;
        var domainBounding = advection.Domain.Bounding;
        Parallel.ForEach(Partitioner.Create(0, Particles.Length), (range) =>
        {
            for (int i = range.Item1; i < range.Item2; i++)
            {
                ref var p = ref Particles[i];
                p.Timealive += double.Abs(dt);
                var t = dat.SimulationTime;
                if (mode == Mode.InstantaneousMerged)
                    t = Particles[i].T;


                var relative = domainRectBoundary.ToRelative(p.Position.Up(t));
                if (Random.Shared.NextSingle() < ReseedChance * dt || relative.X < -0.1 || relative.Y < -0.1 || relative.X > 1.1 || relative.Y > 1.1)
                {
                    Particles[i].Position = Utils.Random(domainRectBoundary).XY;
                    Particles[i].Timealive = 0;
                    /*var max = .24f;
                    var min = -.24f;
                    if (Random.Shared.NextSingle()< dat.ScalerField.Evaluate(Particles[i].Position.Up(.4f)))
                    {
                        break;
                    }*/
                }


                var r = reverse;


                p.Position = rk4.Integrate(advection, domainBounding.Bound(p.Position.Up(t)), dt).XY;
                //p.Position += Vec2.Normalize(advectionR.Evaluate(p.Position.Up(t))) * dt;
                //p.Position += sqrt * RandomWienerVector();
            }
        });
    }

    private bool lastReverse = false;

    public override void Draw(View view)
    {if (!view.Is2DCamera)
            return;
        if (lastReverse != reverse)
        {
            lastReverse = reverse;
            Reset();
        }
        var dat = GetRequiredWorldService<DataService>();

        switch (mode)
        {
            case Mode.Instantaneous:
                Step(dt);
                break;
            case Mode.InstantaneousMerged:
                Step(dt);
                break;
            case Mode.TimeIntegration:
                Step(dat.MultipliedDeltaTime);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        

        //if (additiveBlending)
        GL.BlendFuncSeparate(
            BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha,
            BlendingFactorSrc.One, BlendingFactorDest.One
        );

        double maxLast = dat.VectorField.Domain.RectBoundary.Max.Last;
        var grad = (AltGradient ?? dat.ColorGradient);
        foreach (var p in Particles)
        {
            var c = p.T / maxLast;
            var color = Color;

            if (ColorByGradient)
            {
                color = grad.GetCached(c);
            }
            color.A = (float)alpha;
            if (fadeIn)
                color.A *= MathF.Min(1, (float)p.Timealive / 8);
            Gizmos2D.Instanced.RegisterCircle(p.Position, RenderRadius, color);
        }

        Gizmos2D.Instanced.RenderCircles(view.Camera2D);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    public override void DrawImGuiDataSettings()
    {
        ImGuiHelpers.OptonalVectorFieldSelector(World, ref AltVectorfield);
        ImGuiHelpers.OptionalGradientSelector(ref AltGradient);
        base.DrawImGuiDataSettings();
    }

    public override void DrawImGuiSettings()
    {
        if (ImGui.Button("Reset"))
        {
            Reset();
        }

        ImGuiHelpers.SliderInt("Particle Count", ref Count, 1, 100000);
        ImGuiHelpers.Slider("Reseed Rate", ref ReseedChance, 0, .1f);
        ImGuiHelpers.Slider("Render Radius", ref RenderRadius, 0, .1f);
        ImGui.NewLine();
        ImGui.Checkbox("Color by gradient", ref ColorByGradient);
        if (ColorByGradient)
            ImGui.BeginDisabled();
        ImGuiHelpers.ColorPicker("Particle Color", ref Color);
        if (ColorByGradient)
            ImGui.EndDisabled();
        ImGuiHelpers.Slider("Max alpha", ref alpha, 0, .5f);

        if (ImGui.BeginCombo("Integration Mode", Enum.GetName(mode)))
        {
            foreach (var m in Enum.GetValues<Mode>())
            {
                if (ImGui.Selectable(Enum.GetName(m)))
                    mode = m;
            }
            ImGui.EndCombo();
        }
        bool dtUsed = mode == Mode.Instantaneous || mode == Mode.InstantaneousMerged;
        if (!dtUsed)
            ImGui.BeginDisabled();
        ImGuiHelpers.Slider("dt", ref dt, 0, .1f);
        if (!dtUsed)
            ImGui.EndDisabled();
        ImGui.Checkbox("Reverse", ref reverse);
        base.DrawImGuiSettings();
    }

    private void Reset()
    {
        var dat = GetRequiredWorldService<DataService>();
        var vectorFieldDomain = (AltVectorfield ?? dat.VectorField).Domain;
        Particles = new Particle[Count];
        for (int i = 0; i < Count; i++)
        {
            Particles[i].Position = Utils.Random(vectorFieldDomain.RectBoundary).XY;
            Particles[i].T = (Random.Shared.NextDouble()) * vectorFieldDomain.RectBoundary.Max.Last;
            Particles[i].Timealive = 0;
        }
    }
    public string GetTitle()
    {
        string type = reverse ? "Repelling" : "Attracting";
        return $"{type} regions ({(AltVectorfield ?? GetRequiredWorldService<DataService>().VectorField).DisplayName})";
    }
}