using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Vector3 = System.Numerics.Vector3;

namespace FlowExplainer;

public class StochasticConnectionVisualization : WorldService
{
    public class GridDiagnostics : IGridDiagnostic
    {
        public bool RequireMainThread => true;
        public double interpolationFactor = 1;

        public enum Type
        {
            Attracting,
            Repelling,
            Difference,
            Sum,
        }

        public Type type;
        public double treshhold = 30;
        public void UpdateGridData(GridVisualizer gridVisualizer, CancellationToken token)
        {
            var renderGrid = gridVisualizer.RegularGrid.Grid;
            var dat = gridVisualizer.GetWorldService<DataService>()!;
            var vectorField = dat.VectorField;
            var domain = vectorField.Domain;
            var spatialBounds = domain.RectBoundary.Reduce<Vec2>();
            var stochasticConnectionVisualization = gridVisualizer.GetRequiredWorldService<StochasticConnectionVisualization>();
            int n = 0;
            double min = -1;
            double max = 1;
            switch (type)
            {

                case Type.Attracting:
                    min = 0;
                    max = 1;
                    break;
                case Type.Repelling:
                    min = 1;
                    max = 0;
                    break;
                case Type.Difference:
                    break;
                case Type.Sum:
                    min = 1;
                    max = 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            ParallelGrid.For(renderGrid.GridSize, token, (i, j) =>
            {
                var pos = spatialBounds.FromRelative(new Vec2(i + .5, j + .5) / renderGrid.GridSize.ToVec2());

               
                var density = stochasticConnectionVisualization.EstimateDensity(pos, stochasticConnectionVisualization.kernelSize, min, max);
                ref var cell = ref renderGrid.AtCoords(new Vec2i(i, j)).Value;
                var next = 0.0;

                double d = treshhold;
                if (density > d)
                {
                    density = d;
                    n++;
                }
                if (density < -d)
                {
                    density = -d;
                    n++;
                }
                
                next = density;
                cell = double.Lerp(cell, next, interpolationFactor);
                /*if (density > 0)
                    next = double.Pow(density, 1 / 1f);
                else
                    next = -double.Pow(double.Abs(density), 1 / 1f);*/

                /*if (double.Abs(density) > 10)
                    next = */

                /*if (density == 0)
                    density = 100;*/
                //  renderGrid.AtCoords(new Vec2i(i, j)).Value = density;
                /*
                if (double.Abs(density) < 3)
                    renderGrid.AtCoords(new Vec2i(i, j)).Value = 0;
                else
                {
                    renderGrid.AtCoords(new Vec2i(i, j)).Value = double.Sign(density);
                }
                */
            });

            if (n > renderGrid.GridSize.Volume() * .01)
            {
                treshhold *= 1.04f;
            }
            else
            {
                treshhold /= 1.04f;
            }
            // treshhold = 40;
            /*var min = renderGrid.Data.Min(m => m.Value);
            var max = renderGrid.Data.Max(m => m.Value);
            for (int i = 0; i < renderGrid.Data.Length; i++)
            {

                ref var p = ref renderGrid.Data[i].Value;
                if (p < min*.35)
                {
                    p = min*.35;
                }
            }*/
        }

        public void OnImGuiEdit(GridVisualizer gridVisualizer)
        {
            ImGuiHelpers.Combo("Mode", ref type);
            ImGuiHelpers.SliderFloat("Interpolation", ref interpolationFactor, 0, 1);
        }
    }

    struct Particle
    {
        public int Id;
        public Vec2 StartPosition;
        public Vec2 Position;
        public double Timealive;
        //public Matrix2 C;
        public double CurrentDensity;
    }

    public int Count = 10000;
    public double Alpha = .4f;
    private double gridSizeX;
    public double kernelSizeM = 0.9f;
    public double kernelSize => gridSizeX * kernelSizeM;
    public double ReseedChance = 4.9f;
    public double LifeTime = 4;
    private Particle[] ParticlesForward = [];
    public double RenderRadius = .008f;
    private Particle[] ParticlesBackwords = [];
    private PointSpatialPartitioner2D<Vec2, Vec2i, Particle> partitionerForward;
    private PointSpatialPartitioner2D<Vec2, Vec2i, Particle> partitionerBackword;
    public bool DrawParticles = false;
    public bool HighlightMouse;


    public override void Initialize()
    {
        Init();
    }
    private void Init()
    {

        var dat = GetRequiredWorldService<DataService>();
        var rect = dat.VectorField.Domain.RectBoundary;
        double split = 90;
        gridSizeX = ((rect.Size.X + rect.Size.Y) / 2) / split;
        var gridSize = (rect.Size.XY / gridSizeX).CeilInt();

        ParticlesForward = new Particle[Count];
        ParticlesBackwords = new Particle[Count];
        SetupGridParticles(ParticlesForward, gridSize, rect);
        SetupGridParticles(ParticlesBackwords, gridSize, rect);

        partitionerForward = new(gridSizeX);
        partitionerBackword = new(gridSizeX);
        partitionerForward.Init(ParticlesForward, static (particles, i) => particles[i].Position);
        partitionerBackword.Init(ParticlesBackwords, static (particles, i) => particles[i].Position);
        partitionerForward.UpdateEntries();
        partitionerBackword.UpdateEntries();

        //var ps = Particles.AsSpan();
        /*for (int c = 0; c < ps.Length; c++)
        {
            ref var p = ref ps[c];
            foreach (var i in partitioner2D.GetWithinRadius(p.Phase.XY, kernelSize))
            {
                if (i != c)
                    p.Neighbors.Add(i);
            }
        }*/

        var vel = GetRequiredWorldService<DataService>().VectorField;
        var rk = IIntegrator<Vec3, Vec2>.Rk4;
        /*var steps = 200;
        for (int i = 0; i < steps; i++)
            foreach (ref var p in Particles.AsSpan())
            {
                double dt = 1.0 / steps;
                p.Phase = vel.Domain.Bounding.Bound(rk.Integrate(vel, p.Phase, dt));
                p.Timealive += dt;
            }*/


        //UpdateMatrix();
    }
    private void SetupGridParticles(Particle[] Particles, Vec2i gridSize, Rect<Vec3> rect)
    {
        int i = 0;
        foreach (ref var p in Particles.AsSpan())
        {
            p.Timealive = Utils.Random(-LifeTime, LifeTime);
            p.Position = Utils.Random(rect).XY;
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
        Parallel.For(0, ParticlesForward.Length, i =>
        {
            ref var p = ref ParticlesForward[i];
            p.Position = bounding.Bound(rk.Integrate(vel, p.Position.Up(dat.SimulationTime), dt)).XY;
            p.Timealive += double.Abs(dt);
        });

        Parallel.For(0, ParticlesBackwords.Length, i =>
        {
            ref var p = ref ParticlesBackwords[i];
            p.Position = bounding.Bound(rk.Integrate(vel, p.Position.Up(dat.SimulationTime), -dt)).XY;
            p.Timealive += double.Abs(dt);
        });

        RespawnOld(ParticlesForward);
        RespawnOld(ParticlesBackwords);


        // UpdateMatrix();
    }
    /*private void Respawn(Particle[] particles)
    {
        var dat = GetRequiredWorldService<DataService>();
        var domainRectBoundary = dat.VectorField.Domain.RectBoundary;
        Parallel.ForEach(Partitioner.Create(0, particles.Length), (range) =>
        {
            for (int i = range.Item1; i < range.Item2; i++)
            {
                ref var p = ref particles[i];
                var relative = domainRectBoundary.ToRelative(p.Position.Up(0));
                if (Random.Shared.NextSingle() < ReseedChance * dat.MultipliedDeltaTime || relative.X < -0.1 || relative.Y < -0.1 || relative.X > 1.1 || relative.Y > 1.1)
                {
                    Particles[i].Position = Utils.Random(domainRectBoundary).XY;
                    Particles[i].Timealive = 0;
                    /*var max = .24f;
                    var min = -.24f;
                    if (Random.Shared.NextSingle()< dat.ScalerField.Evaluate(Particles[i].Position.Up(.4f)))
                    {
                        break;
                    }#1#
                }

            }
        });
    }    */

    private void Respawn(Particle[] particles)
    {
        var dat = GetRequiredWorldService<DataService>();
        var rect = dat.VectorField.Domain.RectBoundary;

        //for (int i = 0; i < particles.Length; i++)
        Parallel.For(0, particles.Length, i =>
        {
            ref var p = ref particles[i];
            // ref var p2 = ref Particles[i + Particles.Length / 2];

            //var relevantPartitioner = particles == ParticlesBackwords ? partitionerBackword : partitionerForward;
            var relative = rect.ToRelative(p.Position.Up(0));
            bool outofbounds = relative.X < -0.1 || relative.Y < -0.1 || relative.X > 1.1 || relative.Y > 1.1;
            if (Random.Shared.NextSingle() < ReseedChance * dat.MultipliedDeltaTime || outofbounds)
            {
                var bestMatch = Utils.Random(rect).XY;
                p.Position = bestMatch;
                p.Timealive = 0;
                p.StartPosition = p.Position;
                //p2.Position = p.Position;
                //p2.Timealive = p.Timealive;
                //p2.StartPosition = p.Position;
            }
        });
    }
    private void RespawnOld(Particle[] particles)
    {
        var dat = GetRequiredWorldService<DataService>();
        var rect = dat.VectorField.Domain.RectBoundary;

        //for (int i = 0; i < particles.Length; i++)
        Parallel.For(0, particles.Length, i =>
        {
            ref var p = ref particles[i];
            // ref var p2 = ref Particles[i + Particles.Length / 2];

            var relevantPartitioner = particles == ParticlesBackwords ? partitionerBackword : partitionerForward;
            var relative = rect.ToRelative(p.Position.Up(0));
            bool outofbounds = relative.X < -0.1 || relative.Y < -0.1 || relative.X > 1.1 || relative.Y > 1.1;
            if (Random.Shared.NextSingle() < ReseedChance * dat.MultipliedDeltaTime /*|| p.Timealive > LifeTime*/ || outofbounds)
            {
                var k = 0;
                var bestScore = int.MaxValue;
                var bestMatch = Vec2.Zero;
                for (int j = 0; j < k; j++)
                {
                    var candidate = Utils.Random(rect).XY;
                    var score = relevantPartitioner.GetWithinRadius(candidate, kernelSize / 2).Count();
                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestMatch = candidate;
                    }
                }
                bestMatch = Utils.Random(rect).XY;
                p.Position = bestMatch;

                p.Timealive = Utils.Random(-LifeTime / 2, LifeTime / 2);
                p.StartPosition = p.Position;
                //p2.Position = p.Position;
                //p2.Timealive = p.Timealive;
                //p2.StartPosition = p.Position;
            }
        });
    }

    /*private void UpdateMatrix()
    {

        var ps = Particles;
//for (int i = 0; i < ps.Length; i++)
        Parallel.For(0, ps.Length, i =>
        {
            /*
            ref var p = ref ps[i];
            Matrix2 M = new Matrix2();
            Matrix2 B = new Matrix2();
            foreach (int ni in p.Neighbors)
            {
                ref var n = ref Particles[ni];
                Vec2 dX = n.StartPosition - p.StartPosition;
                Vec2 dx = n.Phase.XY - p.Phase.XY;

                var w = 1;
                M = M.AddOuterProduct(dX, dX * w);
                B = B.AddOuterProduct(dx, dX * w);
            }
            var F = B * M.Inverse();
            var FT = F.Transpose();
            p.C = FT * F;
            #1#

            ps[i].CurrentDensity = EstimateDensity(ps[i].Position);
        });
    }*/

    private IEnumerable<(Particle[], int)> GetWithinRadius(Vec2 pos, double radius)
    {
        foreach (var j in partitionerForward.GetWithinRadius(pos, radius))
            yield return (ParticlesForward, j);

        foreach (var j in partitionerBackword.GetWithinRadius(pos, radius))
            yield return (ParticlesBackwords, j);
    }

    public double SmoothingKernel(double radius, double dst)
    {
        var volume = double.Pi * double.Pow(radius, 8) / 4;
        var value = double.Max(0, radius * radius - dst * dst);
        return value * value * value / volume;
    }

    public double EstimateDensity(Vec2 pos, double h, double backwordsMass = -1.0, double forwardMass = 1.0)
    {
        var density = 0.0;
        var volume = 0.0;
        int w = 0;


        foreach (var pair in GetWithinRadius(pos, h))
        {
            var Particles = pair.Item1;
            var j = pair.Item2;

            var dst = Vec2.Distance(Particles[j].Position, pos);
            var lifetimeFactor = double.Max(0, Particles[j].Timealive / LifeTime);
            lifetimeFactor = 1;
            //lifetimeFactor *= lifetimeFactor;
            var mass = Particles == ParticlesBackwords ? backwordsMass : forwardMass;
            mass /= Particles.Length / lifetimeFactor;
            var influence = SmoothingKernel(h, dst);
            density += (mass) * influence;
            volume += influence;
        }
        return density;

        /*
        foreach (var j in partitioner2D.GetWithinRadius(pos, kernelSize))
        {
            var position = Particles[j].Position;
            var dis2 = Vec2.DistanceSquared(pos, position);
            double minSize = float.Epsilon;
            if (dis2 > minSize) //ignore overlapping.
            {
                var distanceFactor = Vec2.Distance(position, pos) / kernelSize;
                var lifetimeFactor = Particles[j].Timealive / LifeTime;
                //double weight = Kernel_Wendland2D(distanceFactor, kernelSize);

                double sigma = 0.5f;
                var weight = (double)Math.Exp(-(distanceFactor * distanceFactor) / (2 * sigma * sigma));

                var mass = j < Particles.Length / 2 ? -1.0 : 1.0;
                //mass /= Particles.Length;
                density += mass * weight;
                w++;
            }
        }
        return density;*/
    }

    double Kernel_Wendland2D(double q, double h)
    {
        if (q >= 1.0) return 0.0;

        double sigma = 7.0 / (4.0 * Math.PI * h * h);
        double term = (1.0 - q);
        return sigma * term * term * term * (4.0 * q + 1.0);
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        var dat = GetRequiredWorldService<DataService>();
        SimStep(dat.MultipliedDeltaTime);
        var colorGradient = dat.ColorGradient;

        partitionerForward.UpdateEntries();
        partitionerBackword.UpdateEntries();
        if (!DrawParticles)
            return;
        GL.BlendFuncSeparate(
            BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha,
            BlendingFactorSrc.One, BlendingFactorDest.One
        );

        //Logger.LogDebug(EstimateDensity(view.MousePosition).ToString());

        ;
       ;
        foreach (ref var p in ParticlesForward.AsSpan())
        {
            Gizmos2D.Instanced.RegisterCircle(p.Position, RenderRadius, new Color(.2, 1f, .2f, 1).WithAlpha(GetAlpha(p.Timealive)));
        }

        foreach (ref var p in ParticlesBackwords.AsSpan())
            Gizmos2D.Instanced.RegisterCircle(p.Position, RenderRadius,  new Color(1, .2f, .2f, 1).WithAlpha(GetAlpha(p.Timealive)));

        Gizmos2D.Instanced.RenderCircles(view.Camera2D);


        if (HighlightMouse)
        {
            var pos = view.MousePosition;
            foreach (var pair in GetWithinRadius(pos, kernelSize * 3))
            {
                var Particles = pair.Item1;
                var j = pair.Item2;

                var dst = Vec2.Distance(Particles[j].Position, pos);
                var lifetimeFactor = double.Max(0, Particles[j].Timealive / LifeTime);
                lifetimeFactor = 1;
                //lifetimeFactor *= lifetimeFactor;
                var mass = Particles == ParticlesBackwords ? -1.0 : 1.0;
                mass /= Particles.Length / lifetimeFactor;
                var influence = SmoothingKernel(kernelSize, dst);
                //density += (mass) * influence;
                //volume += influence;
                var colc = new Color(1, 0, 0, 1);
                if (mass > 0)
                    colc = new Color(0, 1, 0, 1);
                Gizmos2D.Line(view.Camera2D, view.MousePosition, Particles[j].Position, colc, .0002f);
                Gizmos2D.Circle(view.Camera2D, Particles[j].Position, colc, .0014f);
            }
            double estimateDensity = EstimateDensity(pos, kernelSize * 3);
            //estimateDensity = -.2;
            //Logger.LogDebug(estimateDensity.ToString());
            var col = Utils.Lerp(new Color(1, 0, 0, 1), new Color(0, 1, 0, 1), ((double.Clamp(estimateDensity * estimateDensity * estimateDensity, -1, 1) * 5) + 1) / 2f).WithAlpha(.3f);
            col = (estimateDensity > 0 ? new Color(0, 1, 0, 1) : new Color(1, 0, 0, 1)).WithAlpha(.3);
            Gizmos2D.Circle(view.Camera2D, view.MousePosition, col, kernelSize * 3);
            Gizmos2D.Circle(view.Camera2D, pos, Color.White, .002f);
        }


        /* for (int i = 0; i < .Length; i++)
         {
             var p = Particles[i];
             /*foreach (int i in p.Neighbors)
             {
                 var p2 = Particles[i].Phase;
                 var dis = Vec2.Distance(p.Phase.XY, p2.XY);
                 //  if (dis < 0.4f)
                 //      Gizmos2D.Instanced.RegisterLine(p.Phase.XY, p2.XY, new Color(1, 0, 1, 1), .001f);
             }#1#
             var metric = 0.0;
             //if (p.Neighbors.Count >= 4)
             {
                 //metric = p.Neighbors.Select(s => Vec2.Distance(Particles[s].Phase.XY, p.Phase.XY) / Vec2.Distance(Particles[s].StartPosition, p.StartPosition)).Average();
                 //metric = CalculateFTLEFromTensor2D(p.C, p.Timealive);
                 //metric = p.Neighbors.Select(s => Particles[s].CurrentDensity).Average();
                 metric = p.CurrentDensity;
                 if (!double.IsNaN(metric))
                 {
                     var lifetimeFactor = double.Max(0, p.Timealive / LifeTime);
                     lifetimeFactor *= lifetimeFactor;
                     /*if(EstimateDensity(p.Position) > 1)
                     {
                         var traj = IFlowOperator<Vec2, Vec3>.Default.ComputeTrajectory(dat.SimulationTime, dat.SimulationTime - LifeTime/4, p.Position, dat.VectorField);

                         for (int j = 0; j < traj.Entries.Length-1; j++)
                         {
                         Gizmos2D.Instanced.RegisterLine(traj.Entries[j].XY, traj.Entries[j+1].XY, Color.White, .001f);

                         }
                     }#1#
                     Gizmos2D.Instanced.RegisterCircle(p.Position, .002f, colorGradient.Get(i < Particles.Length / 2 ? 0 : 1).WithAlpha(1));
                 }
             }
             /*foreach (var i in partitioner2D.GetWithinRadius(p.Position, .02f))
             {
                 var p2 = Particles[i].Position;
                 Gizmos2D.Instanced.RegisterLine(p.Position, p2, new Color(1, 0, 1, 1), .003f);
             }#1#
         }*/
        Gizmos2D.Instanced.RenderRects(view.Camera2D);
        Gizmos2D.Instanced.RenderCircles(view.Camera2D);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }
    private double GetAlpha(double pTimealive)
    {
        var a = Alpha;
        a *= double.Min(1, double.Max(0, pTimealive) / 8);
        return a;
    }

    //source gpt
    private double CalculateFTLEFromTensor2D(Matrix2 C, double integrationTime)
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

    public override void DrawImGuiSettings()
    {
        ImGui.Checkbox("Draw particles", ref DrawParticles);
        ImGuiHelpers.SliderInt("Count", ref Count, 1, 100000);
        ImGuiHelpers.SliderFloat("Reseed Rate", ref ReseedChance, 0, 3f);
        ImGuiHelpers.SliderFloat("Kernel Radius", ref kernelSizeM, 0, 3f);
        ImGuiHelpers.SliderFloat("Render Radius", ref RenderRadius, 0, .1f);
        ImGuiHelpers.SliderFloat("T", ref LifeTime, 0, 4f);
        if (ImGui.Button("Reset"))
        {
            Init();
        }
        base.DrawImGuiSettings();
    }

}