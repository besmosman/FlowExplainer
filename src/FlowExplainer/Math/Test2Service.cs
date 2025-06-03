using System.Collections.Concurrent;

namespace FlowExplainer;

public class Test3Service : WorldService
{
    public override ToolCategory Category => ToolCategory.Simulation;

    public class Particle
    {
        public Vec2 StartPos;
        public Vec2 Position;
    }

    public Particle[] Particles;

    public override void Initialize()
    {
        Particles = new Particle[1500];
        for (int i = 0; i < Particles.Length; i++)
        {
            var pos = new Vec2(Random.Shared.NextSingle() * 2, Random.Shared.NextSingle());
            Particles[i] = new()
            {
                StartPos = pos,
                Position = pos,
            };
        }
    }

    private float t = 0;

    public override void Draw(RenderTexture rendertarget, View view)
    {
        var velocity = new AnalyticalEvolvingVelocityField();
        var integrator = new RungeKutta4Integrator();
        float dt = .1f;

        Parallel.ForEach(Particles, (p) =>
        {
            for (int i = 0; i < 32; i++)
                p.Position = integrator.Integrate(velocity.Evaluate, p.Position.Up(t + dt * i), dt / 32f);
        });


        t += dt;

        foreach (var p in Particles)
        {
            Color col = new Color(1, 0, 0, 1);
            if (p.StartPos.X > 1)
                col = new Color(0, 1, 0, 1);
            Gizmos2D.Circle(view.Camera2D, p.Position, col, .005f);
        }
    }
}

public class Test2Service : WorldService
{
    private PoincareComputer poincare;
    private AnalyticalEvolvingVelocityField velocity;

    private ConcurrentBag<(Vec2, List<Vec2>)> trajects;
    private List<Vec2> start = new();

    public override void Initialize()
    {
        velocity = new AnalyticalEvolvingVelocityField();
        var integrator = new RungeKutta4Integrator();
        poincare = new(velocity, integrator);

        trajects = new();
        //int traj = 10000;

        for (float x = 0; x <= 2f; x += .1f)
        for (float y = 0; y <= 1f; y += .1f)
        {
            //var startPos = new Vec2(i / (float)traj * 2f, 1 / 2f);
            //startPos = new Vec2(Random.Shared.NextSingle() * 2, Random.Shared.NextSingle());
            var startPos = new Vec2(x, y);
            start.Add(startPos);
            //this.points.Add(startPos);
        }

        Parallel.ForEach(start, (p) =>
        {
            //var points = poincare.ComputeOne(p, velocity.Period * 32, 60000, 1);
            var points = poincare.ComputeOne(p, velocity.Period, 1500, 100);
            trajects.Add((p, points));
            //trajects.Add((p, [p]));
        });
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        view.Camera2D.Scale = 700;
        view.Camera2D.Position = -new Vec2(1f, .5f);


        foreach (var points in trajects)
        {
            Color col = new Color(1, 0, 0, 1);
            if (points.Item1.X > 1)
                col = new Color(0, 1, 0, 1);
            foreach (var p in points.Item2)
            {
                var pos = p;
                //  if (Math.Abs(t.Z - (MathF.Sin((float)FlowExplainer.Time.TotalSeconds) + 1) / 2 * 30) < 1.9f)
                Gizmos2D.Circle(view.Camera2D, pos, col, .001f);
            }
        }
    }
}