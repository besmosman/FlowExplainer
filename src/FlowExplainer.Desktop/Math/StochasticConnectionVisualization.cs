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
            ParallelGrid.For(renderGrid.GridSize, token, (i, j) =>
            {
                var pos = spatialBounds.FromRelative(new Vec2(i + .5, j + .5) / renderGrid.GridSize.ToVec2());
//var density = stochasticConnectionVisualization.InterpolateDensity(pos, stochasticConnectionVisualization.kernelSize);
                var density = stochasticConnectionVisualization.InterpolatePropertyCurrent(pos, stochasticConnectionVisualization.ConnectionDistance,
                    (p, i1) => p[i1].Value);


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
                if (!double.IsRealNumber(next) || !double.IsFinite(next))
                    next = 100;
                cell = double.Lerp(cell, next, interpolationFactor);
                //cell = next;
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


    public struct NeigborInfo
    {
        public int Index;
        public int Version;
        public Vec2 StartSeperation;
    }

    public struct Particle
    {
        public int Id;
        public int Version;
        public double Mass;
        public Vec2 StartPosition;
        public Vec2 Position;
        public double Timealive;
        public Matrix2 C;
        public List<NeigborInfo> Neighbors;
        public double Value;
    }

    public int Count = 1000;
    public double Alpha = .4f;
    private double gridSizeX;
    public double kernelSizeM = 0.499;
    public double kernelSize => gridSizeX * kernelSizeM;
    public double ReseedChance = 0.4;
    public double LifeTime = 4;
    private Particle[] ParticlesForward = [];
    public double RenderRadius = .004;
    private PointSpatialPartitioner2D<Vec2, Vec2i, Particle> partitionerForward;
    private PointSpatialPartitioner2D<Vec2, Vec2i, Particle> partitionerStartPosition;
    public bool DrawParticles = true;
    public bool HighlightMouse;

    public IMode Mode = new DensityMode();
    public double ConnectionDistance => kernelSize * 4;

    public class FTLEMode : IMode
    {
        public bool ComputeC => true;
        public bool ComputeConnections => true;
        public double GetScaler(StochasticConnectionVisualization v, ref Particle p)
        {
            var ftle = CalculateFTLEFromTensor2D(p.C, p.Timealive);
            if (!double.IsFinite(ftle))
            {
                ftle = 0;
            }
            return ftle;
        }
    }

    public class DensityMode : IMode
    {
        public bool ComputeC => false;
        public bool ComputeConnections => false;
        public double GetScaler(StochasticConnectionVisualization v, ref Particle p)
        {
            return v.InterpolateDensityCurrent(p.Position, v.kernelSize);
        }
    }

    public interface IMode
    {
        public bool ComputeC { get; }
        public bool ComputeConnections { get; }
        public double GetScaler(StochasticConnectionVisualization v, ref Particle p);
    }

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
        SetupGridParticles(ParticlesForward, rect);
        partitionerForward = new(gridSizeX);
        partitionerStartPosition = new(gridSizeX);
        partitionerForward.DistanceSqrtFunc = (a, b) => dat.VectorField.Domain.Bounding.ShortestSpatialDistanceSqrt(a.Up(0), b.Up(0));
        partitionerStartPosition.DistanceSqrtFunc = (a, b) => dat.VectorField.Domain.Bounding.ShortestSpatialDistanceSqrt(a.Up(0), b.Up(0));
        partitionerForward.Init(ParticlesForward, static (particles, i) => particles[i].Position);
        partitionerStartPosition.Init(ParticlesForward, static (particles, i) => particles[i].StartPosition);
        partitionerForward.UpdateEntries();
        partitionerStartPosition.UpdateEntries();
        if (Mode.ComputeConnections)
        {
            for (int i = 0; i < ParticlesForward.Length; i++)
            {
                UpdateNeighbors(ParticlesForward, i);
                //partitionerForward.AddWithinRadius(p.Position, p.Neighbors);
            }
        }
        UpdateData(ParticlesForward);
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
    private void UpdateNeighbors(Particle[] ps, int i)
    {
        ref var p = ref ps[i];
        p.Neighbors.Clear();
        foreach (var n in partitionerForward.GetWithinRadius(p.Position, ConnectionDistance))
        {
            if (i == n)
                continue;

            p.Neighbors.Add(new NeigborInfo
            {
                Index = n,
                Version = ParticlesForward[n].Version,
                StartSeperation = ParticlesForward[n].Position - p.Position,
            });
        }
    }

    private void SetupGridParticles(Particle[] Particles, Rect<Vec3> rect)
    {
        int i = 0;
        foreach (ref var p in Particles.AsSpan())
        {
            p.Timealive = 0;
            p.Position = Utils.Random(rect).XY;
            p.StartPosition = p.Position;
            p.Neighbors = new(6);
            p.Mass = (rect.Size.X * rect.Size.Y) / Particles.Length;
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
        partitionerForward.UpdateEntries();
        RespawnOld(ParticlesForward);
        UpdateData(ParticlesForward);

        // UpdateMatrix();
    }

    private void RespawnOld(Particle[] particles)
    {
        var dat = GetRequiredWorldService<DataService>();
        var rect = dat.VectorField.Domain.RectBoundary;
        Parallel.For(0, particles.Length, i =>
        {
            ref var p = ref particles[i];
            var relevantPartitioner = partitionerForward;
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
                p.Timealive = 0;
                p.Version++;
                UpdateNeighbors(particles, i);
                p.StartPosition = p.Position;
            }
        });
    }

    private void UpdateData(Particle[] Particles)
    {
        var ps = Particles;
        if (Mode.ComputeC)
            Parallel.For(0, ps.Length, i =>
            {
                ref var p = ref ps[i];
                Matrix2 M = new Matrix2();
                Matrix2 B = new Matrix2();
                foreach (var neigh in p.Neighbors)
                {
                    ref var n = ref Particles[neigh.Index];
                    if (neigh.Index == i || neigh.Version != ps[neigh.Index].Version)
                        continue;

                    Vec2 dX = neigh.StartSeperation;
                    Vec2 dx = n.Position - p.Position;

                    var w = 1;
                    M = M.AddOuterProduct(dX, dX * w);
                    B = B.AddOuterProduct(dx, dX * w);
                }

                //regularization
                M.M11 += 1e-6f;
                M.M22 += 1e-6f;

                var F = B * M.Inverse();
                var FT = F.Transpose();
                p.C = FT * F;
                //p.Value = CalculateFTLEFromTensor2D(p.C, p.Timealive);
            });
        Parallel.For(0, ps.Length, i => { ps[i].Value = Mode.GetScaler(this, ref ps[i]); });
    }
    public double SmoothingKernel(double radius, double dst)
    {
        var volume = double.Pi * double.Pow(radius, 8) / 4;
        var value = double.Max(0, radius * radius - dst * dst);
        return value * value * value / volume;
    }

    public double InterpolatePropertyCurrent(Vec2 pos, double h, Func<Particle[], int, double> selector)
    {
        double weightSum = 0.0;
        double valueSum = 0.0;

        foreach (var j in partitionerForward.GetWithinRadius(pos, h))
        {
            ref var p = ref ParticlesForward[j];
            double r = double.Sqrt(partitionerForward.DistanceSqrtFunc(p.Position, pos));
            double w = SmoothingKernel(h, r);
            double value = selector(ParticlesForward, j);


            weightSum += p.Mass * w;
            valueSum += p.Mass * value * w;
        }

        if (weightSum < 1e-12)
            return 0.0;

        return valueSum / weightSum;
    }


    public double InterpolatePropertyStart(Vec2 pos, double h, Func<Particle[], int, double> selector)
    {
        double weightSum = 0.0;
        double valueSum = 0.0;

        foreach (var j in partitionerStartPosition.GetWithinRadius(pos, h))
        {
            ref var p = ref ParticlesForward[j];
            double r = double.Sqrt(partitionerStartPosition.DistanceSqrtFunc(p.StartPosition, pos));
            double w = SmoothingKernel(h, r);
            double value = selector(ParticlesForward, j);
            if (value != 0)
            {
                weightSum += p.Mass * w;
                valueSum += p.Mass * value * w;
            }
        }

        if (weightSum < 1e-12)
            return 0.0;

        return valueSum / weightSum;
    }

    public double InterpolateDensityCurrent(Vec2 pos, double h)
    {
        double weightSum = 0.0;

        foreach (var j in partitionerForward.GetWithinRadius(pos, h))
        {
            ref var p = ref ParticlesForward[j];
            double r = double.Sqrt(partitionerForward.DistanceSqrtFunc(p.Position, pos));
            double w = SmoothingKernel(h, r);
            weightSum += p.Mass * w;
        }

        if (weightSum < 1e-12)
            return 0.0;

        return weightSum;
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
        partitionerForward.UpdateEntries();
        partitionerStartPosition.UpdateEntries();
        var colorGradient = dat.ColorGradient;

        if (!DrawParticles)
            return;
        GL.BlendFuncSeparate(
            BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha,
            BlendingFactorSrc.One, BlendingFactorDest.One
        );

        //Logger.LogDebug(EstimateDensity(view.MousePosition).ToString());

        /*foreach (ref var p in ParticlesForward.AsSpan())
        {
            foreach (var neigh in p.Neighbors)
            {
                ref var n = ref ParticlesForward[neigh.Index];
                if (n.Version != neigh.Version)
                    continue;

                var p0 = p.Position;
                var p1 = n.Position;
                if (Vec2.DistanceSquared(p0, p1) != dat.VectorField.Domain.Bounding.ShortestSpatialDistanceSqrt(p0.Up(0), p1.Up(0)))
                {
                    //continue;
                }
                //   Gizmos2D.Instanced.RegisterLine(p0, p1, Color.White, .001);

            }
        }*/
        foreach (ref var p in ParticlesForward.AsSpan())
        {
            Gizmos2D.Instanced.RegisterCircle(p.StartPosition, RenderRadius, colorGradient.GetCached(double.Clamp(p.Value * 1f, 0, 1)));
        }

        Gizmos2D.Instanced.RenderCircles(view.Camera2D);


        /*
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
                var mass = 1.0;
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
        */


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
            /*for (int i = 0; i < 100; i++)
            {
                SimStep(1 / 100f);

            }*/
        }
        base.DrawImGuiSettings();
    }

}