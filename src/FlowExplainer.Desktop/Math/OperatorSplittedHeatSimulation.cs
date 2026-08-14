namespace FlowExplainer;

public class OperatorSplittedHeatSimulation : WorldService
{
    public struct Particle
    {
        public Vec2 StartPosition;
        public Vec2 Position;
        public double T;
        public double Flux;
        public bool Fixed;
        public double Area;

        public double T_noflow;
    }

    public double Time;
    public Particle[] Particles;
    public IVectorField<Vec3, Vec2> u;
    private PointSpatialPartitioner2D<Vec2, Vec2i, Particle> Partitioner;
    private PointSpatialPartitioner2D<Vec2, Vec2i, Particle> PartitionerStart;
    public double DiffusionKernelRadius = .04;

    public override void Initialize()
    {
        var domain = new SpeetjensVelocityField().Domain;
        u = new ArbitraryField<Vec3, Vec2>(domain, x => new Vec2(double.Sin(x.Z), 0));
        u = new SpeetjensVelocityField()
        {
            epsilon = .4
        };
        var grid = new Vec2i(128, 64) * 2;
        var worldRect = u.Domain.RectBoundary.Reduce<Vec2>();
        var particles = new List<Particle>();

        for (int i = 0; i < grid.X; i++)
        for (int j = 0; j < grid.Y; j++)
        {
            particles.Add(new Particle
            {
                //StartPosition = worldRect.FromRelative(new Vec2(i,j) / grid.ToVec2()),
                StartPosition = Utils.Halton3(worldRect, particles.Count),
                T = .5,
                T_noflow = .5,
                Area = (worldRect.Size / grid.ToVec2()).Area()
            });
        }

        for (int i = 0; i < grid.X * 3; i++)
        {
            particles.Add(new Particle
            {
                StartPosition = worldRect.FromRelative(new Vec2(i / 3.0, 0) / grid),
                T = 1,
                T_noflow = 1,
                Fixed = true,
                Area = (worldRect.Size / grid.ToVec2()).Area()

            });
            particles.Add(new Particle
            {
                StartPosition = worldRect.FromRelative(new Vec2(i / 3.0 / grid.X, 1)),
                T = 0,
                T_noflow = 0,
                Fixed = true,
                Area = (worldRect.Size / grid.ToVec2()).Area()
            });
        }

        Particles = particles.ToArray();
        foreach (ref var p in Particles.AsSpan())
        {
            p.Position = p.StartPosition;
        }
        Partitioner = new PointSpatialPartitioner2D<Vec2, Vec2i, Particle>(.05f);
        Partitioner.Init(Particles, (ps, i) => ps[i].Position);
        Partitioner.UpdateEntries();

        PartitionerStart = new PointSpatialPartitioner2D<Vec2, Vec2i, Particle>(.02f);
        PartitionerStart.Init(Particles, (ps, i) => ps[i].StartPosition);
        PartitionerStart.UpdateEntries();
    }

    public void Step(double dt)
    {
        //DiffusionStep(dt / 2);
        DiffusionStep(dt);
        AdvectionStep(dt);
        Time += dt;
    }

    public void AdvectionStep(double dt)
    {
        var rk4 = IIntegrator<Vec3, Vec2>.Rk4;
        var domainBounding = u.Domain.Bounding;

        Parallel.For(0, Particles.Length, (i) =>
        {
            if (!Particles[i].Fixed)
                Particles[i].Position = domainBounding.Bound(rk4.Integrate(u, Particles[i].Position.Up(Time), dt)).XY;
        });
    }

    public void DiffusionStep(double dt)
    {
        Partitioner.UpdateEntries();
        Parallel.For(0, Particles.Length, (i) =>
        {
            if (!Particles[i].Fixed)
            {
                Particles[i].Flux += (1 / 100.0) * LaplacianTemperature(i) * dt;
            }
        });

        Parallel.For(0, Particles.Length, (i) =>
        {
            if (!Particles[i].Fixed)
            {
                Particles[i].T += Particles[i].Flux;
                Particles[i].Flux = 0;
            }
        });

        Parallel.For(0, Particles.Length, (i) =>
        {
            if (!Particles[i].Fixed)
            {
                Particles[i].Flux += (1 / 100.0) * LaplacianTemperatureNoFlow(i) * dt;
            }
        });

        Parallel.For(0, Particles.Length, (i) =>
        {
            if (!Particles[i].Fixed)
            {
                Particles[i].T_noflow += Particles[i].Flux;
                Particles[i].Flux = 0;
            }
        });
    }

    public double LaplacianTemperature(int i)
    {
        var laplacian = 0.0;
        foreach (int j in Partitioner.GetWithinRadiusPeriodicX(Particles[i].Position, DiffusionKernelRadius, u.Domain.RectBoundary.Size.X))
        {
            if (j == i)
                continue;

            var r = Vec2.Distance(Particles[i].Position, Particles[j].Position);
            if (r == 0)
                continue;

            var area = Particles[i].Area;
            laplacian += 2 * area * (Particles[i].T - Particles[j].T) / r * KernelDerivitive(r, DiffusionKernelRadius);
        }

        return laplacian;
    }

    public double LaplacianTemperatureNoFlow(int i)
    {
        var laplacian = 0.0;
        foreach (int j in PartitionerStart.GetWithinRadiusPeriodicX(Particles[i].StartPosition, DiffusionKernelRadius, u.Domain.RectBoundary.Size.X))
        {
            if (j == i)
                continue;

            var r = Vec2.Distance(Particles[i].StartPosition, Particles[j].StartPosition);
            if (r == 0)
                continue;

            var area = Particles[i].Area;
            laplacian += 2 * area * (Particles[i].T_noflow - Particles[j].T_noflow) / r * KernelDerivitive(r, DiffusionKernelRadius);
        }

        return laplacian;
    }

    //Smoothed Particle Hydrodynaimcs book Chapter 7 spline kernel
    public double Kernel(double r, double h)
    {
        var pre = 10.0 / (7.0 * Math.PI * h * h);
        var q = r / h;
        return pre *
               (q switch
               {
                   >= 0 and < 1 => 1.0 - (3.0 / 2.0) * q * q + 3.0 / 4.0 * q * q * q,
                   >= 1 and <= 2 => 0.25 * Math.Pow(2 - q, 3),
                   _ => 0
               });
    }

    public double KernelDerivitive(double r, double h)
    {
        var pre = 10.0 / (7.0 * Math.PI * h * h);
        var q = r / h;

        return (pre / h) *
               q switch
               {
                   >= 0 and < 1 => -3 * q + (9.0 / 4.0) * q * q,
                   >= 1 and < 2 => -0.75 * Math.Pow(2 - q, 2),
                   _ => 0
               };
    }


    public override void Draw(View view)
    {
        for (int i = 0; i < 1; i++)
        {
            Step(.02f);
        }
        double renderRadius = .002;
        var grad = Gradients.BlueGrayRed;
        foreach (var p in Particles)
        {
           //  Gizmos2D.Instanced.RegisterCircle(p.Position, renderRadius, grad.GetCached((p.T - p.T_noflow)+.5));
            Gizmos2D.Instanced.RegisterCircle(p.Position, renderRadius, grad.GetCached(p.T));
        }

        Gizmos2D.Instanced.RenderCircles(view.Camera2D);
    }
}