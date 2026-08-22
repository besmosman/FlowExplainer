using MemoryPack;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FlowExplainer;

[MemoryPackable]
public partial struct Particle
{
    public Vec2 StartPosition;
    public Vec2 Position;
    public double T;
    public double Flux;
    public bool Fixed;
    public double Area;
    public double TotalDifference;
    public double Tsum;
    public double T_noflow;
    public bool IsSelected;
}

[MemoryPackable]
public partial struct SimulationSave
{
    public double dt;
    public List<Particle[]> States;
}

public class OperatorSplittedPullbackTestService : WorldService
{
    public class SimulationData
    {
        public double Time;
        public Particle[] Particles;
        public IVectorField<Vec3, Vec2> u;
        public PointSpatialPartitioner2D<Vec2, Vec2i, Particle> Partitioner;
        public PointSpatialPartitioner2D<Vec2, Vec2i, Particle> PartitionerStart;
        public double DiffusionKernelRadius = .04;
    }

    public SimulationData Data = new();

    public bool IsSimulationSource;
    public SimulationSave? save;
    public int saveIndex;

    public enum Position
    {
        x_0,
        x,
    }

    public enum Value
    {
        T,
        T_noflow,
        T_diff,
        T_avg,
        T_diff_avg
    }

    public Value renderValue;
    public Position renderPosition;
    public double renderRadius = .005;


    public void Save(double duration, double stepsize)
    {
        var source = IsSimulationSource;
        IsSimulationSource = true;
        Initialize();
        var dat = new SimulationSave();
        dat.dt = stepsize;
        dat.States = new();
        while (Data.Time < duration)
        {
            Step(stepsize);
            dat.States.Add((Particle[])Particles.Clone());
        }

        BinarySerializer.Save("particles.save", dat);
        IsSimulationSource = source;
    }

    public override void Initialize()
    {
        if (IsSimulationSource)
        {
            var domain = new SpeetjensVelocityField().Domain;
            Data.u = new ArbitraryField<Vec3, Vec2>(domain, x => new Vec2(double.Sin(x.Z), 0));
            Data.u = new SpeetjensVelocityField()
            {
                Epsilon = .1
            };
            var grid = new Vec2i(128, 64) * 1;
            var worldRect = Data.u.Domain.RectBoundary.Reduce<Vec2>();
            var particles = new List<Particle>();

            for (int i = 0; i < grid.X; i++)
            for (int j = 0; j < grid.Y; j++)
            {
                particles.Add(new Particle
                {
                    //StartPosition = worldRect.FromRelative(new Vec2(i,j) / grid.ToVec2()),
                    StartPosition = Utils.Halton3(worldRect, particles.Count),
                    T = 1,
                    T_noflow = 1,
                    Area = (worldRect.Size / grid.ToVec2()).Area()
                });
            }

            for (int i = 0; i < grid.X * 3; i++)
            {
                particles.Add(new Particle
                {
                    StartPosition = worldRect.FromRelative(new Vec2(i / 3.0, 0) / grid),
                    T = 2,
                    T_noflow = 2,
                    Fixed = true,
                    Area = (worldRect.Size / grid.ToVec2()).Area()
                });
                particles.Add(new Particle
                {
                    StartPosition = worldRect.FromRelative(new Vec2(i / 3.0 / grid.X, 1)),
                    T = 0.5,
                    T_noflow = 0.5,
                    Fixed = true,
                    Area = (worldRect.Size / grid.ToVec2()).Area()
                });
            }

            Data.Particles = particles.ToArray();
            foreach (ref var p in Data.Particles.AsSpan())
            {
                p.Position = p.StartPosition;
            }

            Data.Partitioner = new PointSpatialPartitioner2D<Vec2, Vec2i, Particle>(.05f);
            Data.Partitioner.Init(Data.Particles, (ps, i) => ps[i].Position);
            Data.Partitioner.UpdateEntries();

            Data.PartitionerStart = new PointSpatialPartitioner2D<Vec2, Vec2i, Particle>(.02f);
            Data.PartitionerStart.Init(Data.Particles, (ps, i) => ps[i].StartPosition);
            Data.PartitionerStart.UpdateEntries();
        }
    }

    public void Step(double dt)
    {
        //DiffusionStep(dt / 2);
        DiffusionStep(dt);
        AdvectionStep(dt);
        Data.Time += dt;
    }

    private Particle[] Particles => Data.Particles;

    public void AdvectionStep(double dt)
    {
        var rk4 = IIntegrator<Vec3, Vec2>.Rk4;
        var domainBounding = Data.u.Domain.Bounding;

        Parallel.For(0, Particles.Length, (i) =>
        {
            if (!Particles[i].Fixed)
                Particles[i].Position = domainBounding.Bound(rk4.Integrate(Data.u, Particles[i].Position.Up(Data.Time), dt)).XY;
        });
    }

    public void DiffusionStep(double dt)
    {
        Data.Partitioner.UpdateEntries();
        Data.PartitionerStart.UpdateEntries();

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
                Particles[i].TotalDifference += dt * (Particles[i].T - Particles[i].T_noflow);
                Particles[i].Tsum += dt * Particles[i].T;
            }
        });
    }

    public double LaplacianTemperature(int i)
    {
        var laplacian = 0.0;
        foreach (int j in Data.Partitioner.GetWithinRadiusPeriodicX(Particles[i].Position, Data.DiffusionKernelRadius, Data.u.Domain.RectBoundary.Size.X))
        {
            if (j == i)
                continue;

            var r = Vec2.Distance(Particles[i].Position, Particles[j].Position);
            if (r == 0)
                continue;

            var area = Particles[i].Area;
            laplacian += 2 * area * (Particles[i].T - Particles[j].T) / r * KernelDerivitive(r, Data.DiffusionKernelRadius);
        }

        return laplacian;
    }

    public double LaplacianTemperatureNoFlow(int i)
    {
        var laplacian = 0.0;
        foreach (int j in Data.PartitionerStart.GetWithinRadiusPeriodicX(Particles[i].StartPosition, Data.DiffusionKernelRadius, Data.u.Domain.RectBoundary.Size.X))
        {
            if (j == i)
                continue;

            var r = Vec2.Distance(Particles[i].StartPosition, Particles[j].StartPosition);
            if (r == 0)
                continue;

            var area = Particles[i].Area;
            laplacian += 2 * area * (Particles[i].T_noflow - Particles[j].T_noflow) / r * KernelDerivitive(r, Data.DiffusionKernelRadius);
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

    double ScaleTemperature(double t)
    {
        return (t - 0.5) / 1.5;
    }

    public override void PreDraw()
    {
        if (IsSimulationSource)
        {
            for (int i = 0; i < 1; i++)
            {
                Step(.02f);
            }

            if (GetGlobalService<WindowService>().Window.IsKeyPressed(Keys.R))
            {
                Initialize();
            }
        }

        base.PreDraw();
    }

    private List<Vec2> selectingRegionPoint = new();

    public override void Draw(View view)
    {
        var grad = Gradients.BlueGrayRed;

        if (save.HasValue)
        {
            if ((Particles?.Length ?? 0) != save.Value.States[saveIndex].Length)
                Data.Particles = new Particle[save.Value.States[saveIndex].Length];

            Array.Copy(save.Value.States[saveIndex], Data.Particles, Data.Particles.Length);
            saveIndex++;
        }

        Func<int, Vec2> getParticlePosition = renderPosition switch
        {
            Position.x_0 => i => Particles[i].StartPosition,
            Position.x => i => Particles[i].Position,
            _ => throw new ArgumentOutOfRangeException()
        };

        Func<int, double> getValue = renderValue switch
        {
            Value.T => i => ScaleTemperature(Particles[i].T),
            Value.T_avg => i => ScaleTemperature(Particles[i].Tsum / Data.Time),
            Value.T_noflow => i => ScaleTemperature(Particles[i].T_noflow),
            Value.T_diff => i => (Particles[i].T - Particles[i].T_noflow) + .5,
            Value.T_diff_avg => i => Particles[i].TotalDifference / Data.Time + .5,
            _ => throw new ArgumentOutOfRangeException()
        };

        for (int i = 0; i < Particles.Length; i++)
        {
            var cached = grad.GetCached(getValue(i));
            if (Particles[i].IsSelected)
            {
                cached = Color.Green;
            }

            Gizmos2D.Instanced.RegisterCircle(getParticlePosition(i), renderRadius, cached);
            //renderRadius, grad.GetCached((p.TotalDifference / Time)  + .5)
            // Gizmos2D.Instanced.RegisterCircle(p.StartPosition, renderRadius, grad.GetCached((p.T - p.T_noflow)+.5));
            // Gizmos2D.Instanced.RegisterCircle(p.StartPosition, renderRadius, grad.GetCached(ScaleTemperature(p.T_noflow)));
            // Gizmos2D.Instanced.RegisterCircle(p.StartPosition, renderRadius, grad.GetCached(ScaleTemperature(p.T)));
        }

        Gizmos2D.Instanced.RenderCircles(view.Camera2D);

        if (view.IsSelected && view.IsMouseButtonDownLeft)
        {
            selectingRegionPoint.Add(view.MousePosition);
        }
        else
        {
            if (selectingRegionPoint.Count > 1)
            {
                foreach (ref var p in Particles.AsSpan())
                {
                    p.IsSelected = false;
                }

                for (int i = 0; i < Particles.Length; i++)
                {
                    if (IsPointInPolygon4(selectingRegionPoint, getParticlePosition(i)))
                    {
                        Particles[i].IsSelected = true;
                    }
                }

                selectingRegionPoint.Clear();
            }
        }

        for (int i = 1; i < selectingRegionPoint.Count; i++)
        {
            var last = selectingRegionPoint[i - 1];
            var cur = selectingRegionPoint[i];
            Gizmos2D.Instanced.RegisterLine(last, cur, Color.Green, .01f);
        }

        Gizmos2D.Instanced.RenderRects(view.Camera2D);
    }

    public override void DrawImGuiSettings()
    {
        ImGuiHelpers.Slider("Render Radius", ref renderRadius, 0, .01);
        ImGuiHelpers.EnumCombo("Render Position", ref renderPosition);
        ImGuiHelpers.EnumCombo("Render Value", ref renderValue);
        base.DrawImGuiSettings();
    }

    //source: https://stackoverflow.com/questions/4243042/c-sharp-point-in-polygon
    /// <summary>
    /// Determines if the given point is inside the polygon
    /// </summary>
    /// <param name="polygon">the vertices of polygon</param>
    /// <param name="testPoint">the given point</param>
    /// <returns>true if the point is inside the polygon; otherwise, false</returns>
    public static bool IsPointInPolygon4(List<Vec2> polygon, Vec2 testPoint)
    {
        bool result = false;
        int j = polygon.Count - 1;
        for (int i = 0; i < polygon.Count; i++)
        {
            if (polygon[i].Y < testPoint.Y && polygon[j].Y >= testPoint.Y ||
                polygon[j].Y < testPoint.Y && polygon[i].Y >= testPoint.Y)
            {
                if (polygon[i].X + (testPoint.Y - polygon[i].Y) /
                    (polygon[j].Y - polygon[i].Y) *
                    (polygon[j].X - polygon[i].X) < testPoint.X)
                {
                    result = !result;
                }
            }

            j = i;
        }

        return result;
    }
}