using System.Collections.Concurrent;
using System.Numerics;
using FlowExplainer;

namespace FlowExplainer;

public static class ParallelGrid
{
    public static void Run(Vec2i grid, Action<int, int> action)
    {
        Parallel.ForEach(Partitioner.Create(0, grid.X * grid.Y), range =>
        {
            for (int i = range.Item1; i < range.Item2; i++)
            {
                var x = i % grid.X;
                var y = i / grid.X;
                action(x, y);
            }
        });

    }
}

public static class LIC
{
    public static void Compute(
        IVectorField<Vec2, float> noiseF,
        IVectorField<Vec3, Vec2> convolution,
        RegularGridVectorField<Vec2, Vec2i, float> output,
        float t, float arcLength)
    {
        var reverse = new ArbitraryField<Vec3, Vec2>(convolution.Domain, (p) => convolution.Evaluate(-p));
        var integrator = IIntegrator<Vec3, Vec2>.Rk4;
        var domain = output.Domain;
        var cellSize = domain.Boundary.Size.X / output.GridSize.X;
        var domainBoundary = domain.Boundary;
        ParallelGrid.Run(output.GridSize, (x, y) =>
        {
            ref var atCoords = ref output.AtCoords(new Vec2i(x, y));
            atCoords = 0;
            //var pos = domainBoundary.Relative(new Vec2(x, y) / output.GridSize.ToVec2());
            var noiseSum = 0f;
            var weightSum = 0f;
            var cur = domainBoundary.Relative(new Vec2(x + .5f, y + .5f) / output.GridSize.ToVec2());
            float stepSizePerCell = cellSize * .5f;
            var steps = float.Ceiling(arcLength / stepSizePerCell)+1;
            for (int k = 0; k < steps; k++)
            {
                if (k != 0)
                {
                    if (!convolution.TryEvaluate(cur.Up(t), out var dir) || dir.LengthSquared() == 0)
                        break;
                    
                    cur += (dir / dir.Length()) * stepSizePerCell;
                }

                float weight = Kernel(k / steps);
                if (noiseF.TryEvaluate(cur, out var noise))
                {
                    noiseSum += noise * weight;
                    weightSum += weight;
                }
            }
            
            for (int k = 0; k < steps; k++)
            {
                    if (!convolution.TryEvaluate(cur.Up(t), out var dir) || dir.LengthSquared() == 0)
                        break;
                    
                    cur += -(dir / dir.Length()) * stepSizePerCell;

                float weight = Kernel(k / steps);
                if (noiseF.TryEvaluate(cur, out var noise))
                {
                    noiseSum += noise * weight;
                    weightSum += weight;
                }
            }

      
            var lic = noiseSum / weightSum;
            atCoords = lic;
        });
    }

    static float Kernel(float dis)
    {
        float sigma = 0.3f;
        return (float)Math.Exp(-(dis * dis) / (2 * sigma * sigma));
    }
}

public class NoiseField : IVectorField<Vec2, float>
{
    public IDomain<Vec2> Domain => IDomain<Vec2>.Infinite;

    FastNoise noise = new FastNoise();

    public float Evaluate(Vec2 x)
    {
        TryEvaluate(x, out var v);
        return v;
    }

    public bool TryEvaluate(Vec2 x, out float value)
    {
        value = ((noise.GetNoise(x.X * 10000, x.Y * 10000)) + 1) * .5f;
        return true;
    }
}

public class LICGridDiagnostic : IGridDiagnostic
{
    public float arcLength = 1;
    public bool UseCustomColoring => true;

    private RegularGridVectorField<Vec2, Vec2i, float> licField = new(Vec2i.One, default, default);
    private IVectorField<Vec2, float> NoiseField = new NoiseField();

    public void UpdateGridData(GridVisualizer gridVisualizer)
    {
        var renderGrid = gridVisualizer.RegularGrid;

        var dat = gridVisualizer.GetRequiredWorldService<DataService>()!;
        var domain = dat.VelocityField.Domain;



        var t = dat.SimulationTime;
        var tau = dat.SimulationTime + arcLength;

        if (licField.GridSize != gridVisualizer.RegularGrid.GridSize)
            licField.Resize(gridVisualizer.RegularGrid.GridSize, gridVisualizer.RegularGrid.RectDomain);


        LIC.Compute(NoiseField, dat.VelocityField, licField, t, arcLength);
        var licMin = licField.Grid.Data.Min();
        var licMax = licField.Grid.Data.Max();
        ParallelGrid.Run(renderGrid.GridSize, (i, j) =>
        {
            var lic = licField.Grid[new Vec2i(i, j)];
            ref var cell = ref renderGrid.Grid.AtCoords(new Vec2i(i, j));
            var pos = renderGrid.ToWorldPos(new Vec2(i + .5f, j + .5f));
            var temp = dat.TempratureField.Evaluate(pos.Up(t));
            var v = gridVisualizer.ScaleScaler(temp);
            cell.Value = temp;
            var licMulti = (lic - licMin) / (licMax - licMin);
            licMulti += .5f;
            cell.Color = dat.ColorGradient.GetCached(v) * new Color(licMulti, licMulti, licMulti, 1);
        });
        /*for (int i = 0; i < licField.Grid.Data.Length; i++)
        {
            var lic = licField.Grid.Data[i];
            renderGrid.Grid.Data[i].Value = dat.TempratureField.Evaluate();
            var v = gridVisualizer.ScaleScaler(renderGrid.Grid.Data[i].Value);
            var heat = dat.ColorGradient.Get(v);
            renderGrid.Grid.Data[i].Color = new Color(float.Abs(renderGrid.Grid.Data[i].Value / 1), 0, 0);
        }*/
    }

    float Kernel(float dis)
    {
        float sigma = 0.3f;
        return (float)Math.Exp(-(dis * dis) / (2 * sigma * sigma));
    }

    public void OnImGuiEdit(GridVisualizer vis)
    {
        var dat = vis.GetRequiredWorldService<DataService>()!;
        float period = dat.VelocityField.Domain.Boundary.Size.Last;
        ImGuiHelpers.SliderFloat("arc length", ref arcLength, 0, dat.VelocityField.Domain.Boundary.Size.X/5);
    }
}