using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class StructureAccentuatingService : WorldService
{
    public ResizableStructArray<Particle> Particles = new(60000);
    private PointSpatialPartitioner2D<Vec2, Vec2i, Particle> partitioner;

    private RgbArrayTexture ArrayTexture;
    private Vec2[] HitCount;

    public struct Particle
    {
        public Vec2 Position;
        public double timeAlive;
        public int Direction;
    }

    public double RenderRadius = .002f;
    public double speed = .4;

    public bool Integration;
    public bool Reseed;
    public bool Transparency;
    public bool Colored;
    public bool DrawTexture;

    public override void Initialize()
    {
        var ps = Particles.AsSpan();
        var domainRectBoundary = DataService.VectorField.Domain.RectBoundary;
        for (int i = 0; i < ps.Length; i++)
        {
            ref var p = ref ps[i];
            p.Position = Utils.Random(domainRectBoundary).XY;
            //p.Position = (p.Position * 50).FloorInt().ToVec2()/50;
            p.Direction = 1;
        }
        double cellSize = .002f;
        var gridSize = (domainRectBoundary.Size.XY / cellSize).CeilInt();
        partitioner = new PointSpatialPartitioner2D<Vec2, Vec2i, Particle>(cellSize);
        partitioner.Init(Particles.Array, (particles, i) => particles[i].Position);
        ArrayTexture = new RgbArrayTexture(gridSize.X, gridSize.Y, new Color[gridSize.X * gridSize.Y])
        {
            TextureMagFilter = TextureMagFilter.Linear,
            TextureMinFilter = TextureMinFilter.Linear,
        };
        HitCount = new Vec2[ArrayTexture.Pixels.Length];
    }

    public float HitBlendFactor = 0;
    public override void Draw(View view)
    {
        var vel = World.DataService.VectorField;
        var velR = IVectorField<Vec3, Vec2>.Arbitrary(World.DataService.VectorField.Domain, p => -vel.Evaluate(p));
        var t = 3.4f;
        var dt = FlowExplainer.DeltaTime;
        var rk4 = IIntegrator<Vec3, Vec2>.Rk4;

        var ps = Particles.Array;
        var bounding = vel.Domain.Bounding;

        if (Reseed && Integration)
            Parallel.For(0, ps.Length, i =>
            {
                ref var p = ref ps[i];
                if (Random.Shared.NextDouble() > 1 - (dt * 0.2))
                {
                    p.Position = Utils.Random(DataService.VectorField.Domain.RectBoundary).XY;
                    p.timeAlive = 0;
                }
            });

        if (Integration)
            Parallel.For(0, ps.Length, i =>
            {
                ref var p = ref ps[i];
                IVectorField<Vec3, Vec2> vec = vel;
                if (p.Direction == -1)
                    vec = velR;

                p.Position = rk4.Integrate(vec, p.Position.Up(t), speed * dt).XY;
                p.Position = bounding.Bound(p.Position.Up(t)).XY;
                p.timeAlive += speed * dt;
            });



        partitioner.UpdateEntries();

        double renderRadius = RenderRadius;
        if (Transparency)
            renderRadius *= 2;

        if (!DrawTexture)
            foreach (ref var p in Particles.AsSpan())
            {
                if (p.Direction == 0)
                    continue;
                Color c = Color.White;

                if (Colored)
                    c = p.Direction == 1 ? new Color(.0, 1, .0) : new Color(1, .0, .0);
                if (Transparency)
                {
                    c = c.WithAlpha(double.Max(1, p.timeAlive / 5) / 10);
                    //RenderRadius *= 2;
                }

                Gizmos2D.Instanced.RegisterCircle(p.Position, renderRadius, c);
            }

        var domainRectBoundary = vel.Domain.RectBoundary.Reduce<Vec2>();


        var max = HitCount.MaxBy(m => m.Y).Y / 70;
        var min = -max;
        var colorGradient = new ColorGradient("r", [(0, Color.Red), (0.5, new Color(1, 1, 0, 1)), (1, Color.Green)]);
        //    colorGradient = Gradients.Grayscale;

        if (!Colored)
            colorGradient = Gradients.Grayscale;

        HitBlendFactor = .993f;
        if (DrawTexture)
        {
            ParallelGrid.For(ArrayTexture.Size, CancellationToken.None, (i, j) =>
            {
                HitCount[j * ArrayTexture.Size.X + i] *= HitBlendFactor;

                var rel = domainRectBoundary.FromRelative(new Vec2(i, j) / (ArrayTexture.Size.ToVec2() - new Vec2(1))) / domainRectBoundary.Size;
                var count = 0;
                if (partitioner.Data.TryGetValue(partitioner.GetVoxelCoords(domainRectBoundary.FromRelative(rel)), out List<int> ints))
                    foreach (var k in ints)
                    {
                        HitCount[j * ArrayTexture.Size.X + i] += new Vec2(Particles[k].Direction, double.Abs(Particles[k].Direction));
                    }
            });
            ParallelGrid.For(ArrayTexture.Size, CancellationToken.None, (i, j) =>
            {
                double count = HitCount[j * ArrayTexture.Size.X + i].Y;
                double dirAvg = HitCount[j * ArrayTexture.Size.X + i].X;
                //count = double.Clamp(count, min, max);
                var countScaled = (count - min) / (max - min);
                var colorC = ((dirAvg / count) + 1) / 2;
                if (count == 0)
                    colorC = 0;

                var f = double.Clamp(count / max, 0, 1);
                //if (count > 0)
                ArrayTexture.Pixels[j * ArrayTexture.Size.X + i] = colorGradient.Get(colorC) * double.Pow(f, 0.6f);

            });
        }
        ArrayTexture.Upload();
        Gizmos2D.ImageCenteredInvertedY(view.Camera2D, ArrayTexture, domainRectBoundary.Center, domainRectBoundary.Size);
        Gizmos2D.Circle(view.Camera2D, view.MousePosition, Color.Red, .01f);
        //GL.Disable(EnableCap.DepthTest);
        if (Transparency)
            GL.BlendFuncSeparate(
                BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha,
                BlendingFactorSrc.One, BlendingFactorDest.One
            );
        Gizmos2D.Instanced.RenderCircles(view.Camera2D);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }
}