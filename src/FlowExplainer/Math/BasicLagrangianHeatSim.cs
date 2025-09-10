using System.Buffers;
using System.Threading.Tasks;

namespace FlowExplainer;

public class BasicLagrangianHeatSim
{
    public struct Particle
    {
        public Vec2 Position;
        public float Heat;
        public float LastHeat;
        public float Tag;
        public float RadiationHeatFlux;
        public float DiffusionHeatFlux;
        public float TotalConvectionHeatFlux;
        public float TotalHeatFlux;
    }

    public Particle[] Particles = new Particle[0];
    public IIntegrator<Vec3, Vec2> Integrator = new RungeKutta4IntegratorGen<Vec3, Vec2>();
    public float cellSize = .1f;
    private Dictionary<Vec2i, List<int>> grid = new();

    private Rect<Vec2> rect;

    public float HeatDiffusionFactor = 0.0f; //heat diffusion
    public float RadiationFactor = .0f; //heat radiation strength;
    public float KernelRadius = .0f;

    public void Setup(Rect<Vec2> rect, float spacing)
    {
        this.rect = rect;
        List<Vec2> positions = new();
        float m = rect.Size.X / 100f;
        for (float x = rect.Min.X; x <= rect.Max.X; x += spacing)
        for (float y = rect.Min.Y; y <= rect.Max.Y; y += spacing)
        {
            var r = new Vec2(Random.Shared.NextSingle(), Random.Shared.NextSingle()) - new Vec2(.5f, .5f);
            positions.Add(new Vec2(x, y) + r * m);
        }

        Particles = new Particle[positions.Count];

        for (int i = 0; i < Particles.Length; i++)
        {
            ref var p = ref Particles[i];
            p.Position = positions[i];
            p.Heat = .5f;
            p.LastHeat = p.Heat;
            p.Tag = p.Position.X < rect.Center.X ? 1 : 0;
        }
    }

    public void Update(IVectorField<Vec3, Vec2> velocityField, float time, float dt)
    {
        if (Particles.Length == 0)
            return;


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

        var parallelOptions = new ParallelOptions()
        {
            /*MaxDegreeOfParallelism = 10*/
        };

        Parallel.For(0, Particles.Length, parallelOptions, (i) =>
        {
            ref var p = ref Particles[i];
            p.RadiationHeatFlux = 0f;
            p.DiffusionHeatFlux = 0f;
            p.Position = Integrator.Integrate(velocityField, new(p.Position, time), dt);


            //bounds shouldnt be needed though
            if (p.Position.X < rect.Min.X)
                p.Position.X = rect.Max.X - float.Epsilon;
            if (p.Position.X > rect.Max.X)
                p.Position.X = rect.Min.X + float.Epsilon;

            if (p.Position.Y < rect.Min.Y)
                p.Position.Y = float.Epsilon;
            if (p.Position.Y > rect.Max.Y)
                p.Position.Y = rect.Max.Y - float.Epsilon;
            //var r = new Vec2(Random.Shared.NextSingle(), Random.Shared.NextSingle()) - new Vec2(.5f, .5f);
            // p.Position += r * .001f * dt;
            float eps = .00001f;

            //bottom hot wall
            float dis = p.Position.Y - rect.Min.Y;
            var intensity = (1f / (dis * dis + eps));
            p.RadiationHeatFlux += (1 - p.Heat) * Single.Min(1, intensity * dt * RadiationFactor);

            //top cold wall
            dis = rect.Max.Y - p.Position.Y;
            intensity = (1f / (dis * dis + eps));
            p.RadiationHeatFlux += (0 - p.Heat) * Single.Min(1, intensity * dt * RadiationFactor);
        });


        if (HeatDiffusionFactor > 0 && KernelRadius > 0)
            Parallel.For(0, Particles.Length, parallelOptions, (i) =>
                //    for (int i = 0; i < Particles.Length; i++)
            {
                ref var p = ref Particles[i];
                int[] withinRange = GetWithinRange(i, KernelRadius);
                foreach (int j in withinRange)
                {
                    if (j == -1)
                        break;

                    if (i < j)
                    {
                        float distance = Vec2.Distance(Particles[j].Position, p.Position);
                        distance *= distance;
                        var flux = HeatDiffusionFactor * (KernelRadius - distance) / KernelRadius * -(Particles[j].Heat - p.Heat) * dt;
                        Particles[j].DiffusionHeatFlux += flux;
                        p.DiffusionHeatFlux -= flux;
                    }
                }

                ArrayPool<int>.Shared.Return(withinRange);
            });

        Parallel.For(0, Particles.Length, (i) =>
        {
            ref var p = ref Particles[i];
            p.LastHeat = p.Heat;
            p.Heat += p.RadiationHeatFlux;
            p.Heat += p.DiffusionHeatFlux;

            p.DiffusionHeatFlux /= dt;
            p.RadiationHeatFlux /= dt;
            p.TotalConvectionHeatFlux = -p.RadiationHeatFlux - p.DiffusionHeatFlux; //assuming steady state which is bad :(
        });
    }


    public int[] GetWithinRange(int i, float radius)
    {
        var array = ArrayPool<int>.Shared.Rent(10000);
        int index = 0;
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
                    {
                        array[index] = p;
                        index++;
                    }
                }
        }

        array[index] = -1;
        return array;
    }

    public IEnumerable<int> GetWithinRangeOld(int i, float radius)
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