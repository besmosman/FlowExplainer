namespace FlowExplainer;

public class StochasticConnectionVisualization : WorldService
{

    struct Particle
    {
        public int Id;
        public Vec2 StartPosition;
        public Vec2 Position;
        public double Timealive;
        public List<int> Neighbors;
    }

    public int Count = 10000;
    private Particle[] Particles = [];
    public double Speed = 0;

    private PointSpatialPartitioner2D<Particle> partitioner2D;
    public override void Initialize()
    {
        var dat = GetRequiredWorldService<DataService>();
        Particles = new Particle[Count];
        for (int i = 0; i < Count; i++)
        {
            Particles[i].Position = Utils.Random(dat.VectorField.Domain.RectBoundary).XY;
            Particles[i].StartPosition = Particles[i].Position;
            Particles[i].Timealive = 0;
            Particles[i].Id = i;
            Particles[i].Neighbors = new();
        }
        partitioner2D = new PointSpatialPartitioner2D<Particle>(dat.VectorField.Domain.RectBoundary.Size.Y / 10f);
        partitioner2D.Init(Particles, static (particles, i) => particles[i].Position);
        partitioner2D.UpdateEntries();

        var ps = Particles.AsSpan();
        for (int c = 0; c < ps.Length; c++)
        {
            ref var p = ref ps[c];
            foreach (var i in partitioner2D.GetWithinRadius(p.Position, .01f))
            {
                if (i != c)
                    p.Neighbors.Add(i);
            }
        }


    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        var vel = GetRequiredWorldService<DataService>().VectorField;
        var rk = IIntegrator<Vec3, Vec2>.Rk4;
        foreach (ref var p in Particles.AsSpan())
        {
            p.Position = vel.Domain.Bounding.Bound(rk.Integrate(vel, p.Position.Up(0.5), Speed).Up(.5f)).XY;
        }

        partitioner2D.UpdateEntries();
        foreach (var p in Particles)
        {
            foreach (int i in p.Neighbors)
            {
                var p2 = Particles[i].Position;
                var dis = Vec2.Distance(p.Position, p2);
                if (dis < .4f)
                    Gizmos2D.Instanced.RegisterLine(p.Position, p2, new Color(1, 0, 1, 1), .0003f);
            }
            var metric = 0.0;
            if (p.Neighbors.Count > 0)
                metric = p.Neighbors.Select(s => Vec2.Distance(Particles[s].Position, p.Position) / Vec2.Distance(Particles[s].StartPosition, p.StartPosition)).Average();
            Gizmos2D.Instanced.RegisterCircle(p.Position, .002f, new Color(metric * .1f, 0, 0, 1));
            /*foreach (var i in partitioner2D.GetWithinRadius(p.Position, .02f))
            {
                var p2 = Particles[i].Position;
                Gizmos2D.Instanced.RegisterLine(p.Position, p2, new Color(1, 0, 1, 1), .003f);
            }*/
        }
        Gizmos2D.Instanced.RenderRects(view.Camera2D);
        Gizmos2D.Instanced.RenderCircles(view.Camera2D);
    }

    public override void DrawImGuiSettings()
    {
        ImGuiHelpers.SliderFloat("Speed", ref Speed, 0, .1);
        base.DrawImGuiSettings();
    }
}