using OpenTK.Mathematics;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace FlowExplainer;

public class Sph
{
    public Particle[] Particles = new Particle[40000];
    public IIntegrator<Vector3, Vector2> Integrator = new RungeKutta4Integrator();
    public float cellSize = .05f;
    private Dictionary<Vector2i, List<int>> grid = new();

    public void Setup()
    {
        var ps = Particles.AsSpan();
        for (int i = 0; i < ps.Length; i++)
        {
            ref var p = ref ps[i];
            p.Position = new Vector2(Random.Shared.NextSingle() * 1, Random.Shared.NextSingle() * .5f);
            //p.Position = new Vector2(i/(float)ps.Length, .25f);
            p.Heat = .5f;
            p.tag = Vector2.Distance(p.Position, new Vector2(.5f,.3f)) < .2f ? 1 : 0;
        }
    }

    public void Update(IVectorField<Vector3, Vector2> velocityField, float time, float dt)
    {
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
            p.HeatFlux = 0f;
            p.Position = Integrator.Integrate(velocityField.Evaluate, new(p.Position, time), dt);
            float margin = .1f;
            float speed = dt * 8;
            if (p.Position.Y < margin)
            {
                float c = (margin - p.Position.Y) / margin;
                p.Heat = Single.Lerp(p.Heat, 1, Single.Min(1, speed * c * c));
            }

            if (p.Position.Y > .5f - margin)
            {
                float c = (margin - (.5f - p.Position.Y)) / margin;
                p.Heat = Single.Lerp(p.Heat, 0, Single.Min(1, speed * c * c));
            }

           // p.Position += new Vector2(Random.Shared.NextSingle() - .5f, Random.Shared.NextSingle() - .5f) * 0.00001f;

            /*if (p.Position.X < 0)
                p.Position.X = 0;
            if (p.Position.X > 1)
                p.Position.X = 1;

            if (p.Position.Y < 0)
                p.Position.Y = 0;
            if (p.Position.Y > .5f)
                p.Position.Y = .5f;*/
        });

        /*
        Parallel.For(0, Particles.Length, (i) =>
        {
            ref var p = ref Particles[i];

            var totalFlux = 0f;
            foreach (int j in GetWithinRange(i, .06f))
            {
                if (p.Heat > Particles[j].Heat)
                {
                    totalFlux += (p.Heat - Particles[j].Heat);
                }
            }

            foreach (int j in GetWithinRange(i, .06f))
            {
                if (p.Heat > Particles[j].Heat)
                {
                    var flux = (p.Heat - Particles[j].Heat)/totalFlux;
                    flux *= .001f;
                    Particles[j].HeatFlux += flux;
                    p.HeatFlux -= flux;
                }
            }
        });*/

        Parallel.For(0, Particles.Length, (i) =>
        {
            ref var p = ref Particles[i];
            p.Heat += p.HeatFlux;
        });
    }

    public IEnumerable<int> GetWithinRange(int i, float radius)
    {
        var pos = Particles[i].Position;
        var min = GetVoxelCoords(pos - new Vector2(radius, radius));
        var max = GetVoxelCoords(pos + new Vector2(radius, radius));

        for (int x = min.X; x <= max.X; x++)
        for (int y = min.Y; y <= max.Y; y++)
        {
            grid.TryGetValue(new Vector2i(x, y), out var l);

            if (l != null)
                foreach (var p in l)
                    if (p != i && Vector2.DistanceSquared(pos, Particles[p].Position) < radius * radius)
                        yield return p;
        }
    }

    private Vector2i GetVoxelCoords(Vector2 pPosition)
    {
        var p = pPosition / cellSize;
        return new Vector2i((int)p.X, (int)p.Y);
    }
}