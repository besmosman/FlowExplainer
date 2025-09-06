namespace FlowExplainer;

public class Test3Service : WorldService
{
    public override ToolCategory Category => ToolCategory.Flow;

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
        var integrator = IIntegrator<Vec3, Vec2>.Rk4;
        float dt = .1f;

        Parallel.ForEach(Particles, (p) =>
        {
            for (int i = 0; i < 32; i++)
                p.Position = integrator.Integrate(velocity, p.Position.Up(t + dt * i), dt / 32f);
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