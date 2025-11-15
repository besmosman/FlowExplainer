using ImGuiNET;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class StochasticPoincare : WorldService
{
    public IVectorField<Vec3, Vec2> VectorField;
    
    public struct Particle
    {
        public Vec2 Position;
    }

    public Particle[] Particles;
    public int Count = 100000;

    public double dt = 0.001;
    public double RenderRadius = .008f;

    public double alpha = .1f;
    public bool reverse;
    public override ToolCategory Category => ToolCategory.Flow;

    public override void Initialize()
    {
        var dat = GetRequiredWorldService<DataService>();

        Particles = new Particle[Count];
        for (int i = 0; i < Count; i++)
        {
            Particles[i].Position = Utils.Random(dat.VectorField.Domain.RectBoundary).XY;
        }
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
        var dat = GetRequiredWorldService<DataService>();
        var advection = dat.VectorField;
        var advectionR = new ArbitraryField<Vec3, Vec2>(dat.VectorField.Domain, p => -advection.Evaluate(p));
        var Pe = 100;
        var t = dat.SimulationTime;

        double sqrt = double.Sqrt((2 * dt) / Pe);
        var domainRectBoundary = dat.VectorField.Domain.RectBoundary;
        var rk4 = IIntegrator<Vec3, Vec2>.Rk4;
        Parallel.For(0, Particles.Length, (i) =>
        {
            ref var p = ref Particles[i];
            if (Random.Shared.NextSingle() > .99f)
            {
                while (true)
                {
                    Particles[i].Position = Utils.Random(domainRectBoundary).XY;
                    var max = .8f;
                    var min = -.8f;
                    if (Random.Shared.NextSingle() * (max - min) + min < dat.ScalerField.Evaluate(Particles[i].Position.Up(t)))
                    {
                        break;
                    }
                }
            }

            if (reverse)
                p.Position = rk4.Integrate(advectionR, p.Position.Up(t), dt);
            else
                p.Position = rk4.Integrate(advection, p.Position.Up(t), dt);
            //p.Position += Vec2.Normalize(advectionR.Evaluate(p.Position.Up(t))) * dt;
            //p.Position += sqrt * RandomWienerVector();
            p.Position = advection.Domain.Bounding.Bound(p.Position.Up(t)).XY;
        });
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        var dat = GetRequiredWorldService<DataService>();
        Step(dt);

        if (!view.Is2DCamera)
            return;
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);

        foreach (var p in Particles)
        {
            var color = new Color(1, 1, 1, 1f);
            Gizmos2D.Instanced.RegisterCircle(p.Position, RenderRadius / 10, new Color(1, 1, 1, alpha));
        }

        Gizmos2D.Instanced.RenderCircles(view.Camera2D);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    public override void DrawImGuiEdit()
    {
        
        ImGui.BeginGroup();
        
        ImGui.EndGroup();
        
        if (ImGui.Button("Reset"))
        {
            var dat = GetRequiredWorldService<DataService>();
            Particles = new Particle[Count];
            for (int i = 0; i < Count; i++)
            {
                Particles[i].Position = Utils.Random(dat.VectorField.Domain.RectBoundary).XY;
            }
        }

        ImGuiHelpers.SliderFloat("dt", ref dt, 0, .1f);
        ImGuiHelpers.SliderFloat("Render Radius", ref RenderRadius, 0, .1f);
        ImGuiHelpers.SliderFloat("Alpha", ref alpha, 0, .1f);
        ImGui.Checkbox("Reverse", ref reverse);
        base.DrawImGuiEdit();
    }
}