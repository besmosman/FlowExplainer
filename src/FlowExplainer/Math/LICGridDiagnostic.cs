using System.Numerics;
using FlowExplainer;

namespace FlowExplainer;

public static class ParallelGrid
{
    public static void Run(Vec2i grid, Action<int, int> action)
    {
        Parallel.For(0, grid.Y, y =>
        {
            for (int x = 0; x < grid.X; x++)
                action(x, y);
        });
    }
}

public static class LIC
{
    public static void Compute(
        IVectorField<Vec2, float> noiseF,
        IVectorField<Vec3, Vec2> convolution,
        RegularGridVectorField<Vec2, Vec2i, float> output,
        float t,
        float T)
    {
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
            float dt = cellSize * .5f;
            var steps = float.Abs(T) / dt;
            for (int k = 0; k < steps; k++)
            {
                if (k != 0)
                {
                    var diff = (integrator.Integrate(convolution, cur.Up(t), dt) - cur);
                    if (diff.Length() > 0)
                    {
                        cur = cur + (diff / diff.Length()) * cellSize * .1f;
                    }
                    else
                    {
                        break;
                    }
                }

                float weight = Kernel(k / steps * .1f);
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
        value = ((noise.GetSimplex(x.X * 5000, x.Y * 5000)) + 1) * 1;
        return true;
    }
}

public class LICGridDiagnostic : IGridDiagnostic
{
    public float T = 1;

    private RegularGridVectorField<Vec2, Vec2i, float> licField = new(Vec2i.One, default, default);
    private IVectorField<Vec2, float> NoiseField = new NoiseField();

    public void UpdateGridData(GridVisualizer gridVisualizer)
    {
        var renderGrid = gridVisualizer.RegularGrid;

        var dat = gridVisualizer.GetRequiredWorldService<DataService>()!;
        var domain = dat.VelocityField.Domain;

        var t = dat.SimulationTime;
        var tau = dat.SimulationTime + T;

        if (licField.GridSize != gridVisualizer.RegularGrid.GridSize)
            licField.Resize(gridVisualizer.RegularGrid.GridSize, gridVisualizer.RegularGrid.RectDomain);


        LIC.Compute(NoiseField, dat.VelocityField, licField, t, T);
        for (int i = 0; i < licField.Grid.Data.Length; i++)
            renderGrid.Grid.Data[i].Value = licField.Grid.Data[i];

        return;

        Vec2 gridToWorld(Vec2 v)
            => domain.Boundary.Reduce<Vec2>().Relative(new Vec2(v.X, v.Y) / renderGrid.GridSize.ToVec2());

        var cellSize = domain.Boundary.Size.X / renderGrid.GridSize.X;
        var domainBoundary = domain.Boundary.Reduce<Vec2>();
        var integrator = IIntegrator<Vec3, Vec2>.Rk4;
        Parallel.For(0, renderGrid.GridSize.Y, y =>
        {
            for (int x = 0; x < renderGrid.GridSize.X; x++)
            {
                renderGrid.AtCoords(new Vec2i(x, y)).Value = 0;
                var pos = domainBoundary.Relative(new Vec2(x, y) / renderGrid.GridSize.ToVec2());
                var noiseSum = 0f;
                var weightSum = 0f;

                var cur = domainBoundary.Relative(new Vec2(x + .5f, y + .5f) / renderGrid.GridSize.ToVec2());
                float dt = cellSize * .5f;
                var steps = T / dt;

                for (int k = 0; k < steps; k++)
                {
                    cur = integrator.Integrate(dat.VelocityField, cur.Up(t), dt);
                    float weight = Kernel(k / steps * .1f);
                    var noise = NoiseField.Evaluate(cur / 1);
                    noiseSum += noise * weight;
                    weightSum += weight;
                }

                var lic = noiseSum / weightSum;
                renderGrid.AtCoords(new Vec2i(x, y)).Value = lic;
            }
            //renderGrid.AtCoords(new Vec2i(i, j)).Value = NoiseField.AtCoords(new Vec2i(i,j));
        });

        /*for (int x = 0; x < renderGrid.GridSize.X; x++)
        for (int y = 0; y < renderGrid.GridSize.Y; y++)
        {
//            renderGrid.Grid.AtCoords(new Vec2i(x, y)).Value =  NoiseField.AtCoords(new Vec2i(x,y));
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
        ImGuiHelpers.SliderFloat("T", ref T, -period * 1, period * 1);
    }
}