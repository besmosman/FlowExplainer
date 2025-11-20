using ImGuiNET;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class Stochastic3DVisualization : WorldService
{

    public override void Initialize()
    {

    }

    public override void Draw(RenderTexture rendertarget, View view)
    {

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
    public double RespawnChance = .01f;
    public bool additiveBlending = true;
    public bool FixedT = true;
    public bool ColorByGradient = false;
    public bool TimeIntegration = true;
    

    public override string? Name => "Stochastic Structures";
    public override string? CategoryN => "Structure";
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
        var advectionR = new ArbitraryField<Vec3, Vec2>(vectorfield.Domain, p => -advection.Evaluate(p));
        var Pe = 1000000;

        double sqrt = double.Sqrt((2 * dt) / Pe);
        var domainRectBoundary = vectorfield.Domain.RectBoundary;
        var rk4 = IIntegrator<Vec3, Vec2>.Rk4;
        Parallel.For(0, Particles.Length, (i) =>
        {
            ref var p = ref Particles[i];
            p.Timealive += dat.FlowExplainer.DeltaTime;
            if (Random.Shared.NextSingle() < RespawnChance)
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

            var t = dat.SimulationTime;
            if (!FixedT)
                t = Particles[i].T;

            var r = reverse;
            
            if (TimeIntegration)
            {
                if (r)
                    p.Position = rk4.Integrate(advectionR, p.Position.Up(t), dt);
                else
                    p.Position = rk4.Integrate(advection, p.Position.Up(t), dt);
            }
            else
            {
                if (r)
                    p.Position = rk4.Integrate(advectionR, p.Position.Up(t), dt);
                else
                    p.Position = rk4.Integrate(advection, p.Position.Up(t), dt);
            }
            //p.Position += Vec2.Normalize(advectionR.Evaluate(p.Position.Up(t))) * dt;
            //p.Position += sqrt * RandomWienerVector();
            p.Position = advection.Domain.Bounding.Bound(p.Position.Up(t)).XY;
        });
    }

    private bool lastReverse = false;

    public override void Draw(RenderTexture rendertarget, View view)
    {
        if (lastReverse != reverse)
        {
            lastReverse = reverse;
            Reset();
        }
        var dat = GetRequiredWorldService<DataService>();
        if (TimeIntegration)
        {
            Step(dat.MultipliedDeltaTime);
        }
        else
        {
            Step(dt);
        }


        if (!view.Is2DCamera)
            return;

        if (additiveBlending)
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);

        if (!FixedT)
            GL.BlendFuncSeparate(
                BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha,
                BlendingFactorSrc.One, BlendingFactorDest.One
            );

        double maxLast = dat.VectorField.Domain.RectBoundary.Max.Last;
        var grad = (AltGradient ?? dat.ColorGradient);
        foreach (var p in Particles)
        {
            var c = p.T / maxLast;
            var color = Color.White;

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
        return;
    }

    public override void DrawImGuiDataSettings()
    {
        OptonalVectorFieldSelector(ref AltVectorfield);
        AltGradientSelector(ref AltGradient);
        base.DrawImGuiDataSettings();
    }

    public override void DrawImGuiSettings()
    {
        if (ImGui.Button("Reset"))
        {
            Reset();
        }

        ImGuiHelpers.SliderInt("Particle Count", ref Count, 1, 100000);
        ImGuiHelpers.SliderFloat("dt", ref dt, 0, .1f);
        ImGuiHelpers.SliderFloat("Respawn Rate", ref RespawnChance, 0, .1f);
        ImGuiHelpers.SliderFloat("Render Radius", ref RenderRadius, 0, .1f);
        ImGuiHelpers.SliderFloat("Alpha", ref alpha, 0, .5f);

        ImGui.Checkbox("Color by gradient", ref ColorByGradient);
        ImGui.Checkbox("Reverse", ref reverse);
        ImGui.Checkbox("Locked t", ref FixedT);
        base.DrawImGuiSettings();
    }

    private void Reset()
    {
        var dat = GetRequiredWorldService<DataService>();
        Particles = new Particle[Count];
        for (int i = 0; i < Count; i++)
        {
            Particles[i].Position = Utils.Random(dat.VectorField.Domain.RectBoundary).XY;
            Particles[i].T = (Random.Shared.NextDouble()) * dat.VectorField.Domain.RectBoundary.Max.Last;
            Particles[i].Timealive = 0;
        }
    }
    public string GetTitle()
    {
        string type = reverse ? "Repelling" : "Attracting";
        return $"{type} regions ({(AltVectorfield ?? GetRequiredWorldService<DataService>().VectorField).DisplayName})";
    }
}