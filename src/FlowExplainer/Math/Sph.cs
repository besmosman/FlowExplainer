using System.Buffers;
using System.Threading.Tasks;

namespace FlowExplainer;

public class Sph
{
    public struct Particle
    {
        public Vec2 Position;
        public float Heat;
        public float tag;
        public float RadiationHeatFlux;
        public float DiffusionHeatFlux;
    }

    public Particle[] Particles = new Particle[0];
    public IIntegrator<Vec3, Vec2> Integrator = new RungeKutta4Integrator();
    public float cellSize = .1f;
    private Dictionary<Vec2i, List<int>> grid = new();

    private Rect domain;

    public float HeatDiffusionFactor = 0.5f; //heat diffusion
    public float RadiationFactor = .05f; //heat radiation strength;
    public float KernelRadius = .14f;
    
    public void Setup(Rect domain, float spacing)
    {
        this.domain = domain;
        List<Vec2> positions = new();
        float m = domain.Size.X / 100f;
        for (float x = domain.Min.X; x <= domain.Max.X; x += spacing)
        for (float y = domain.Min.Y; y <= domain.Max.Y; y += spacing)
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
            p.tag = Vec2.Distance(p.Position, new Vec2(.5f, .3f)) < 1f ? 1 : 0;
        }
    }

    public void Update(IVectorField<Vec3, Vec2> velocityField, float time, float dt)
    {
        if(Particles.Length ==0)
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
        
        Parallel.For(0, Particles.Length, (i) =>
        {
            ref var p = ref Particles[i];
            p.RadiationHeatFlux = 0f;
            p.DiffusionHeatFlux = 0f;
            p.Position = Integrator.Integrate(velocityField.Evaluate, new(p.Position, time), dt);
            //var r = new Vec2(Random.Shared.NextSingle(), Random.Shared.NextSingle()) - new Vec2(.5f, .5f);
            // p.Position += r * .001f * dt;
            float eps = .002f;

            //bottom wall
            float dis = p.Position.Y;
            var intensity = (1f / (dis * dis + eps));
            p.RadiationHeatFlux += (1 - p.Heat) * Single.Min(1, intensity * dt * RadiationFactor);

            //top wall
            dis = 1f - p.Position.Y ;
            intensity = (1f / (dis * dis + eps));
            p.RadiationHeatFlux += (0 - p.Heat) * Single.Min(1, intensity * dt * RadiationFactor);


            //bounds shouldnt be needed though
            if (p.Position.X < domain.Min.X)
                p.Position.X = domain.Min.X + float.Epsilon;
            if (p.Position.X > domain.Max.X)
                p.Position.X = domain.Max.X - float.Epsilon;

            if (p.Position.Y < domain.Min.Y)
                p.Position.Y = float.Epsilon;
            if (p.Position.Y > domain.Max.Y)
                p.Position.Y = domain.Max.Y - float.Epsilon;
        });
        
         Parallel.For(0, Particles.Length, (i) =>
        {
            ref var p = ref Particles[i];
            p.Heat += p.RadiationHeatFlux;
        });

        Parallel.For(0, Particles.Length, (i) =>
        //    for (int i = 0; i < Particles.Length; i++)
        {
            ref var p = ref Particles[i];
            int[] withinRange = GetWithinRange(i, KernelRadius);
            foreach (int j in withinRange)
            {
                if(j == -1)
                    break;
                
                float distance = Vec2.Distance(Particles[j].Position, p.Position);
                var flux = dt * HeatDiffusionFactor  * (KernelRadius*KernelRadius - distance*distance) / KernelRadius * -(Particles[j].Heat - p.Heat);
                Particles[j].DiffusionHeatFlux += flux;
                p.DiffusionHeatFlux -= flux;
            }

            ArrayPool<int>.Shared.Return(withinRange);
        });

        Parallel.For(0, Particles.Length, (i) =>
        {
            ref var p = ref Particles[i];
            p.Heat += p.DiffusionHeatFlux;
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