using ImGuiNET;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class StochasticPoincare : WorldService
{
    public struct Particle
    {
        public Vec2 Position;
    }

    public Particle[] Particles;
    public int Count = 200000;

    public float RenderRadius = .004f;

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


    static float RandomNormal()
    {
        float u1 = 1f - Random.Shared.NextSingle();
        float u2 = 1f - Random.Shared.NextSingle();
        return MathF.Sqrt(-2f * MathF.Log(u1)) * MathF.Cos(2f * MathF.PI * u2);
    }

    public Vec2 RandomWienerVector()
    {
        return new Vec2(RandomNormal(), RandomNormal());
    }

    public void Step(float dt)
    {
        var dat = GetRequiredWorldService<DataService>();
        var advection = dat.VectorField;
        var Pe = 100;
        var t = dat.SimulationTime;

        float sqrt = float.Sqrt((2 * dt) / Pe);
        var domainRectBoundary = dat.VectorField.Domain.RectBoundary;
        Parallel.For(0, Particles.Length, (i) =>
        {
            ref var p = ref Particles[i];
            if (Random.Shared.NextSingle() > .9f)
            {
                while (true)
                {
                    Particles[i].Position = Utils.Random(domainRectBoundary).XY;
                    var max = .8f;
                    var min = -.8f;
                    if (Random.Shared.NextSingle()*(max-min) + min < dat.ScalerFields[dat.currentSelectedScaler].Evaluate(Particles[i].Position.Up(t)))
                    {
                        break;
                    }
                }
            }

            p.Position += advection.Evaluate(p.Position.Up(t)) * dt;
            //p.Position += sqrt * RandomWienerVector();
            p.Position = advection.Domain.Bounding.Bound(p.Position.Up(t)).XY;
        });

    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        var dat = GetRequiredWorldService<DataService>();
        Step(dat.MultipliedDeltaTime);

        if (!view.Is2DCamera)
            return;
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);

        foreach (var p in Particles)
        {
            var color = new Color(1, 1, 1, 1f);
            Gizmos2D.Instanced.RegisterCircle(p.Position, RenderRadius, new Color(1, 1, 1, .02f));
        }

        Gizmos2D.Instanced.RenderCircles(view.Camera2D);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

    }

    public override void DrawImGuiEdit()
    {
        if (ImGui.Button("Reset"))
        {
            var dat = GetRequiredWorldService<DataService>();
            Particles = new Particle[Count];
            for (int i = 0; i < Count; i++)
            {
                Particles[i].Position = Utils.Random(dat.VectorField.Domain.RectBoundary).XY;
            }
        }
        base.DrawImGuiEdit();
    }
}