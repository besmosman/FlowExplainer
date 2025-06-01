using System.Threading.Tasks;

namespace FlowExplainer;

public class Sph
{
    public Particle[] Particles = new Particle[0];
    public IIntegrator<Vec3, Vec2> Integrator = new RungeKutta4Integrator();
    public float cellSize = .1f;
    private Dictionary<Vec2i, List<int>> grid = new();

    public void Setup()
    {
        var ps = Particles.AsSpan();
        for (int i = 0; i < ps.Length; i++)
        {
            ref var p = ref ps[i];
            p.Position = new Vec2(Random.Shared.NextSingle() * 1, Random.Shared.NextSingle() * .5f);
            //p.Position = new Vec2(i/(float)ps.Length, .25f);
            p.Heat = .5f;
            
            p.tag = Vec2.Distance(p.Position, new Vec2(.5f, .3f)) < .2f ? 1 : 0;
        }
    }

    public void Update(IVectorField<Vec3, Vec2> velocityField, float time, float dt)
    {
        float k = .01f; //heat diffusion
        float thermalEffect = .009f; //heat radiation strength;
        float rad = .13f;

        
        var ps = Particles.AsSpan();
        foreach (var l in grid.Values)
        {
            l.Clear();
        }

        for (int i = 0; i < ps.Length; i++)
        {
            var p = ps[i];
            var coords = GetVoxelCoords(p.Position);

            if (!grid.ContainsKey(coords))
                grid.Add(coords, new());

            grid[coords].Add(i);
        }

        Parallel.For(0, Particles.Length, (i) =>
        {
            ref var p = ref Particles[i];
            p.RadiationHeatFlux = 0f;
            p.DiffusionHeatFlux = 0f;
            p.Position = Integrator.Integrate(velocityField.Evaluate, new(p.Position, time), dt);

            float eps = .002f;

            //bottom wall
            float dis = p.Position.Y + .001f;
            var intensity = (1f / (dis * dis + eps));
            p.RadiationHeatFlux += (1 - p.Heat) * Single.Min(1, intensity * dt * thermalEffect);

            //top wall
            dis = .5f - p.Position.Y + +.001f;
            intensity = (1f / (dis * dis + eps));
            p.RadiationHeatFlux += (0 - p.Heat) * Single.Min(1, intensity * dt * thermalEffect);
            
            
            //bounds shouldnt be needed though
            if (p.Position.X < 0)
                p.Position.X = 0;
            if (p.Position.X > 1)
                p.Position.X = 1;

            if (p.Position.Y < 0)
                p.Position.Y = 0;
            if (p.Position.Y > .5f)
                p.Position.Y = .5f;
        });

        /*
        Parallel.For(0, Particles.Length, (i) =>
            //for (int i = 0; i < Particles.Length; i++)
        {
            ref var p = ref Particles[i];
            foreach (int j in GetWithinRange(i, rad))
            {
                // if (Particles[j].Heat > p.Heat)
                {
                    float distance = Vec2.Distance(Particles[j].Position, p.Position);
                    var flux = k * dt * (rad - distance) / rad * -(Particles[j].Heat - p.Heat);
                    Particles[j].DiffusionHeatFlux += flux;
                    p.DiffusionHeatFlux -= flux;
                }
            }
        });
        */

        Parallel.For(0, Particles.Length, (i) =>
        {
            ref var p = ref Particles[i];
            p.Heat += p.DiffusionHeatFlux;
            p.Heat += p.RadiationHeatFlux;
        });
    }

    public IEnumerable<int> GetWithinRange(int i, float radius)
    {
        var pos = Particles[i].Position;
        var min = GetVoxelCoords(pos - new Vec2(radius, radius));
        var max = GetVoxelCoords(pos + new Vec2(radius, radius));
        float r2 = radius * radius;

        for (int x = min.X; x <= max.X; x++)
        for (int y = min.Y; y <= max.Y; y++)
        {
            grid.TryGetValue(new Vec2i(x, y), out var l);

            if (l != null)
                foreach (var p in l)
                {
                    if (p != i && Vec2.DistanceSquared(pos, Particles[p].Position) < r2)
                        yield return p;
                }
        }
    }

    private Vec2i GetVoxelCoords(Vec2 pPosition)
    {
        var p = pPosition / cellSize;
        return new Vec2i((int)p.X, (int)p.Y);
    }
}