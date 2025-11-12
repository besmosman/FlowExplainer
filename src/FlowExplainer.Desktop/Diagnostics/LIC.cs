namespace FlowExplainer;

public static class LIC
{
    public static void ComputeASteady(
        IVectorField<Vec2, float> noiseF,
        IVectorField<Vec3, Vec2> convolution,
        RegularGridVectorField<Vec2, Vec2i, float> output,
        float t_start, float time, float arcLength)
    {
        var domain = output.Domain;
        var cellSize = domain.RectBoundary.Size.X / output.GridSize.X;
        var domainBoundary = domain.RectBoundary;
        ParallelGrid.For(output.GridSize, (x, y) =>
        {
            ref var atCoords = ref output.AtCoords(new Vec2i(x, y));
            atCoords = 0;
            //var pos = domainBoundary.Relative(new Vec2(x, y) / output.GridSize.ToVec2());
            var noiseSum = 0f;
            var weightSum = 0f;
            var cur = domainBoundary.Relative(new Vec2(x + .5f, y + .5f) / output.GridSize.ToVec2());
            float stepSizePerCell = cellSize * .5f;
            var steps = float.Ceiling(arcLength / stepSizePerCell) + 1;
            bool failed = false;
            for (int k = 0; k < steps; k++)
            {
                var t = t_start + time * (k / steps);
                if (k != 0)
                {
                    if (!convolution.TryEvaluate(domainBoundary.Clamp(cur).Up(t), out var dir) || dir.LengthSquared() == 0)
                        break;

                    cur += (dir / dir.Length()) * stepSizePerCell;
                }

                float distance = k * stepSizePerCell;
                float weight = Kernel(distance / arcLength);
                if (noiseF.TryEvaluate(cur, out var noise))
                {
                    noiseSum += noise * weight;
                    weightSum += weight;
                }
            }

            /*
            cur = domainBoundary.Relative(new Vec2(x + .5f, y + .5f) / output.GridSize.ToVec2());
            for (int k = 1; k < steps; k++)
            {
                var t = t_start - time * (k / steps);

                if (!convolution.TryEvaluate(domainBoundary.Clamp(cur).Up(t), out var dir) || dir.LengthSquared() == 0)
                    break;

                cur += -(dir / dir.Length()) * stepSizePerCell;

                float distance = k * stepSizePerCell;
                float weight = Kernel(distance / arcLength);
                if (noiseF.TryEvaluate(cur, out var noise))
                {
                    noiseSum += noise * weight;
                    weightSum += weight;
                }
            }*/

            var lic = noiseSum / weightSum;
            atCoords = lic;
        });
    }

    public static void ComputeSteady(
        IVectorField<Vec2, float> noiseF,
        IVectorField<Vec3, Vec2> convolution,
        RegularGridVectorField<Vec2, Vec2i, float> output,
        float t, float arcLength)
    {
        var domain = output.Domain;
        var cellSize = domain.RectBoundary.Size.X / output.GridSize.X;
        var domainBoundary = domain.RectBoundary;
        ParallelGrid.For(output.GridSize, (x, y) =>
        {
            ref var atCoords = ref output.AtCoords(new Vec2i(x, y));
            atCoords = 0;
            //var pos = domainBoundary.Relative(new Vec2(x, y) / output.GridSize.ToVec2());
            var noiseSum = 0f;
            var weightSum = 0f;
            var cur = domainBoundary.Relative(new Vec2(x + .5f, y + .5f) / output.GridSize.ToVec2());
            float stepSizePerCell = cellSize * .5f;
            var steps = float.Ceiling(arcLength / stepSizePerCell) + 1;
            bool failed = false;
            for (int k = 0; k < steps; k++)
            {
                if (k != 0)
                {
                    cur.X = cur.X % domainBoundary.Max.X;
                    if (!convolution.TryEvaluate(domainBoundary.Clamp(cur).Up(t), out var dir) || dir.LengthSquared() == 0)
                        break;

                    cur += (dir / dir.Length()) * stepSizePerCell;
                }

                float distance = k * stepSizePerCell;
                float weight = Kernel(distance / arcLength);
                if (noiseF.TryEvaluate(cur, out var noise))
                {
                    noiseSum += noise * weight;
                    weightSum += weight;
                }
            }

            cur = domainBoundary.Relative(new Vec2(x + .5f, y + .5f) / output.GridSize.ToVec2());
            for (int k = 1; k < steps; k++)
            {
                cur.X = cur.X % domainBoundary.Max.X;
                if (!convolution.TryEvaluate(domainBoundary.Clamp(cur).Up(t), out var dir) || dir.LengthSquared() == 0)
                    break;

                cur += -(dir / dir.Length()) * stepSizePerCell;

                float distance = k * stepSizePerCell;
                float weight = Kernel(distance / arcLength);
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