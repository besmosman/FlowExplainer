using ImGuiNET;

namespace FlowExplainer;

public class StochasticConnectionVisualizationTest : WorldService
{
    public struct NeigborInfo
    {
        public int Index;
        public int NeighborVersionWhenConnected;
        public Vec2 StartSeperation;
    }

    public struct Particle
    {
        public bool Border;
        public int Left;
        public int Right;
        public int Down;
        public int Up;


        public Vec2 Position;
        public int Version;
        public Vec2 StartPosition;
        public Matrix2 C;
        public double FTLE;
        public double TimeAlive;
        public List<NeigborInfo> Neighbors;
    }

    public Vec2i GridSize = new Vec2i(48, 24) * 2;
    public Particle[] Particles = [];

    private PointSpatialPartitioner2D<Vec2, Vec2i, Particle> partitioner;

    private void UpdateData(Particle[] Particles)
    {
        var ps = Particles;
        Parallel.For(0, ps.Length, i =>
        {
            ref var p = ref ps[i];
            Matrix2 M = new Matrix2();
            Matrix2 B = new Matrix2();
            foreach (var neigh in p.Neighbors)
            {
                ref var n = ref Particles[neigh.Index];
                if (neigh.Index == i || neigh.NeighborVersionWhenConnected != ps[neigh.Index].Version)
                    continue;

                Vec2 dX = neigh.StartSeperation;
                Vec2 dx = n.Position - p.Position;

                var w = 1;
                M = M.AddOuterProduct(dX, dX * w);
                B = B.AddOuterProduct(dx, dX * w);
            }

            /*//regularization
            M.M11 += 1e-6f;
            M.M22 += 1e-6f;*/

            var F = B * M.Inverse();
            var FT = F.Transpose();
            p.C = FT * F;

            
            
            
            p.FTLE = CalculateFTLEFromTensor2D(p.C, p.TimeAlive);
        });
    }

    //source gpt
    private static double CalculateFTLEFromTensor2D(Matrix2 C, double integrationTime)
    {
        var delta = C;

        var m = delta.Trace * .5;
        var p = delta.Determinant;
        var n = m * m - p;

        if (n < 1e-05)
            n = 0;

        var right = double.Sqrt(n);
        var max_eigen = double.Max(m + right, m - right);
        var ftle = (1f / double.Abs(integrationTime)) * double.Log(double.Sqrt(max_eigen));
        return ftle;
    }

    public override void Initialize()
    {
        var dat = GetWorldService<DataService>()!;
        var vectorField = dat.VectorField;
        var domain = vectorField.Domain;
        var spatialBounds = domain.RectBoundary.Reduce<Vec2>();
        Dictionary<Vec2i, int> byGridSpot = new();
        Particles = new Particle[GridSize.Volume()];
        {
            int i = 0;
            for (int x = 0; x < GridSize.X; x++)
            for (int y = 0; y < GridSize.Y; y++)
            {
                var pos = spatialBounds.FromRelative(new Vec2(x, y) / GridSize);
                Particles[i] = new Particle
                {
                    Position = pos,
                    StartPosition = pos,
                };
                byGridSpot.Add(new Vec2i(x, y), i);
                i++;
            }
        }

        partitioner = new PointSpatialPartitioner2D<Vec2, Vec2i, Particle>(.02f);
        partitioner.Init(Particles, (particles, i1) => particles[i1].Position);
        partitioner.UpdateEntries();

        /*var ps = Particles.AsSpan();
        foreach (var g in byGridSpot)
        {
            ref var p = ref Particles[g.Value];
            byGridSpot.TryGetValue(g.Key + new Vec2i(-1, 0), out p.Left);
            byGridSpot.TryGetValue(g.Key + new Vec2i(1, 0), out p.Right);
            byGridSpot.TryGetValue(g.Key + new Vec2i(0, -1), out p.Down);
            byGridSpot.TryGetValue(g.Key + new Vec2i(0, 1), out p.Up);
        }*/

         AddNearbyNeighbors(spatialBounds);
    }
    private void AddNearbyNeighbors(Rect<Vec2> spatialBounds)
    {

        var ps = Particles.AsSpan();
        for (int i = 0; i < ps.Length; i++)
        {
            ref var p = ref ps[i];
            p.Neighbors = new List<NeigborInfo>();
            foreach (var n in partitioner.GetWithinRadius(p.Position, (spatialBounds.Size.X / GridSize.X) * 1.5))
            {
                if (i != n)
                    p.Neighbors.Add(new NeigborInfo
                    {
                        Index = n,
                        NeighborVersionWhenConnected = 0,
                        StartSeperation = Particles[n].Position - p.Position,
                    });
            }
        }
    }
    private void SimStep(double dt)
    {
        if (dt == 0)
            return;
        var vel = GetRequiredWorldService<DataService>().VectorField;
        var rk = IIntegrator<Vec3, Vec2>.Rk4;
        var dat = GetRequiredWorldService<DataService>();

        var bounding = vel.Domain.Bounding;
        Parallel.For(0, Particles.Length, i =>
        {
            ref var p = ref Particles[i];
            p.Position = bounding.Bound(rk.Integrate(vel, p.Position.Up(dat.SimulationTime), dt)).XY;
            p.TimeAlive += double.Abs(dt);
        });
        partitioner.UpdateEntries();
        UpdateData(Particles);
        // UpdateMatrix();
    }

    public override void DrawImGuiSettings()
    {
        if (ImGui.Button("Woow"))
        {
            Initialize();
        }
        base.DrawImGuiSettings();
    }
    public override void DrawImGuiDataSettings()
    {

        base.DrawImGuiDataSettings();
    }

    public override void Draw(View view)
    {
        SimStep(GetRequiredWorldService<DataService>().MultipliedDeltaTime);

        foreach (ref var p in Particles.AsSpan())
        {
            foreach (var neigh in p.Neighbors)
            {
                ref var n = ref Particles[neigh.Index];
                if (n.Version != neigh.NeighborVersionWhenConnected)
                    continue;
                var p0 = p.Position;
                var p1 = n.Position;
                //  Gizmos2D.Instanced.RegisterLine(p0, p1, Color.White, .001);
            }
        }

        float RenderRadius = .002f;
        var dat = GetWorldService<DataService>()!;
        var datColorGradient = dat.ColorGradient;

        var datVectorField = dat.VectorField;
        var spatialBounds = datVectorField.Domain.RectBoundary.Reduce<Vec2>();
        var d = spatialBounds.Size / GridSize;
        foreach (ref var p in Particles.AsSpan())
        {
            //var ff = FTLEComputer.Compute(p.Position, 0, 1, datVectorField, d);
            // Gizmos2D.Instanced.RegisterCircle(p.Position, RenderRadius, datColorGradient.GetCached(p.FTLE));
            Gizmos2D.Instanced.RegisterCircle(p.StartPosition, RenderRadius, datColorGradient.GetCached(p.FTLE / 5));
            if (p.FTLE < 0)
            {
                int c = 5;
            }
        }

        Gizmos2D.Instanced.RenderCircles(view.Camera2D);
        Gizmos2D.Instanced.RenderRects(view.Camera2D);
    }
}