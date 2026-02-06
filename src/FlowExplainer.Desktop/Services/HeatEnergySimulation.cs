using System.Globalization;
using Microsoft.VisualBasic;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class HeatEnergySimulation : WorldService
{
    public class TemperatureDiagnostic : IGridDiagnostic
    {
        public void UpdateGridData(GridVisualizer gridVisualizer, CancellationToken token)
        {
            var rk4 = IIntegrator<Vec3, Vec2>.Rk4;
            int steps = 64;
            var dat = gridVisualizer.GetRequiredWorldService<DataService>()!;
            var vectorfield = dat.VectorField;
            var renderGrid = gridVisualizer.RegularGrid;
            var domain = vectorfield.Domain.RectBoundary.Reduce<Vec2>();
            ParallelGrid.For(renderGrid.GridSize, token, (i, j) =>
            {
                var worldpos = domain.FromRelative(new Vec2(i, j) / renderGrid.GridSize);
                var heatsim = gridVisualizer.GetRequiredWorldService<HeatEnergySimulation>();
                renderGrid.AtCoords(new Vec2i(i, j)).Value = heatsim.EstimateTemperatureAt(worldpos);
            });
        }

        public void OnImGuiEdit(GridVisualizer gridVisualizer)
        {

        }
    }

    public struct EnergyParticle
    {
        public double Influence;
        public Vec2 Position;
        public double TimeAlive;
        public int Index;
    }

    public EnergyParticle[] Particles;
    public int Count = 195000;
    public Stack<int> DeadParticles = new Stack<int>();
    public float TempratureTopWall = 0;
    public float TempratureBotWall = 2;
    public float HeatEnergyPerParticle = 1f / 64;
    public float InfluenceRadius = .02f;

    private float GridSpacing = .03f;
    private Rect<Vec2> WorldRect;

    public PointSpatialPartitioner2D<Vec2, Vec2i, EnergyParticle> partitioner;

    public struct Cell
    {
        public double Temperature;
    }

    public Func<Vec2, Vec2, double> shortestSpatialFunc;
    public override void Initialize()
    {

        Particles = new EnergyParticle[Count];
        for (int i = Particles.Length - 1; i >= 0; i--)
        {
            DeadParticles.Push(i);
            Particles[i].Index = i;
        }
        var dat = GetRequiredWorldService<DataService>();

        var rect = dat.VectorField.Domain.RectBoundary;
        WorldRect = rect.Reduce<Vec2>();
        shortestSpatialFunc = (a, b) => dat.VectorField.Domain.Bounding.ShortestSpatialDistanceSqrt(a.Up(0), b.Up(0));
        AddTemperatureGrid();
        partitioner = new PointSpatialPartitioner2D<Vec2, Vec2i, EnergyParticle>(GridSpacing);
        partitioner.Init(Particles, (particles, i) => particles[i].Position);
        for (int i = 0; i < (WorldRect.Size.X*WorldRect.Size.Y * 2000) / HeatEnergyPerParticle; i++)
        {
            var sample = new Vec2(Random.Shared.NextSingle(), Random.Shared.NextSingle() / 2);
            //Spawn(sample);
        }
        partitioner.UpdateEntries();

    }

    public void AddTemperatureGrid()
    {
        World.AddVisualisationService<GridVisualizer>().SetGridDiagnostic(new TemperatureDiagnostic());
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

    public double Gaussian(double disSqrt)
    {
        var sigma = InfluenceRadius / 3.3f;
        return Math.Exp(-(disSqrt) / (2f * sigma * sigma));
    }

    private Cell def;

    public double EstimateTemperatureAt(Vec2 pos)
    {
        double temp = 0.0;
        //border links/rechts fixen. Moet in partioner gebeuren helaas.
        foreach (var i in partitioner.GetWithinRadiusPeriodicX(pos, InfluenceRadius, 1))
        {
            var disSqrt = shortestSpatialFunc(pos, Particles[i].Position);
            temp += Gaussian(disSqrt) * HeatEnergyPerParticle * Particles[i].Influence;
            //temp += Particles[i].Influence * HeatEnergyPerParticle;
        }
        return temp;
    }

    public override void Draw(View view)
    {
        var rk4 = IIntegrator<Vec3, Vec2>.Rk4;
        var dat = GetRequiredWorldService<DataService>();
        var transportfield = dat.VectorField;
        var dt = dat.MultipliedDeltaTime;
        double t = dat.SimulationTime;
        if (dt != 0)
        {

            def = default;
            Parallel.For(0, Particles.Length, (i) =>
            {
                ref var p = ref Particles[i];
                if (p.Influence == 0)
                    return;

                p.Position = rk4.Integrate(transportfield, p.Position.Up(t), dt).XY;
                var alpha = 0.5f / 100; //??? idk
                var diff = Math.Sqrt(2 * alpha * dt) * RandomWienerVector();
                p.Position += diff;
                p.TimeAlive += dt;

                /*if (p.Position.Y > .5f || p.PositionNoFlow.Y > .5f)
                {
                    Die(ref p);
                }*/

                if (p.Position.Y < 0)
                {
                    p.Position.Y = -p.Position.Y;
                }

                if (p.Position.X < .0f)
                    p.Position.X += 1f;

                if (p.Position.X > 1f)
                    p.Position.X -= 1f;

                /*
                if (p.PositionNoFlow.X < .0f)
                    p.PositionNoFlow.X += 1f;

                if (p.PositionNoFlow.X > 1f)
                    p.PositionNoFlow.X -= 1f;
                    */

            });

            partitioner.UpdateEntries();

            for (int i = 0; i < 1000; i++)
            {
                var samplePosBot = new Vec2(Random.Shared.NextSingle(),0 );
                var samplePosTop = new Vec2(Random.Shared.NextSingle(), .5f);

                foreach (var p in partitioner.GetWithinRadius(samplePosTop, partitioner.CellSize))
                {
                    Die(ref Particles[p]);
                }

                var delta = TempratureBotWall - EstimateTemperatureAt(samplePosBot);
                for (int k = 0; k < (int)(delta / HeatEnergyPerParticle) / 1; k++)
                {
                    Spawn(new Vec2((Random.Shared.NextSingle()-.5f)/30 + samplePosBot.X, (Random.Shared.NextSingle()/10f)));
                }
                if (i % 10 == 0)
                {
                    partitioner.UpdateEntries();
                }
            }
            def = default;
        }

        foreach (ref var p in Particles.AsSpan())
        {
            if (p.Influence == 0)
                continue;
            Gizmos2D.Instanced.RegisterCircle(p.Position, .002f, Color.White.WithAlpha(.1) * p.Influence);
        }

        GL.BlendFuncSeparate(
            BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha,
            BlendingFactorSrc.One, BlendingFactorDest.One
        );
        Gizmos2D.Instanced.RenderCircles(view.Camera2D);

        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        Gizmos2D.AdvText(view.Camera2D, view.MousePosition, .03f, Color.White, EstimateTemperatureAt(view.MousePosition).ToString());

    }
    private void Die(ref EnergyParticle p)
    {
        p.Influence = 0;
        //p.TimeAlive = 0;
        //DeadParticles.Push(p.Index);
    }

    private ref EnergyParticle Spawn(Vec2 pos)
    {
        var i = DeadParticles.Pop();
        ref var p = ref Particles[i];
        p.Influence = 1;
        p.Position = pos;
        return ref p;
    }

}