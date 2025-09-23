using OpenTK.Mathematics;

namespace FlowExplainer;

public static class FTLEComputer
{
    public static float Compute(Vec2 x, float t_start, float t_end, IVectorField<Vec3, Vec2> vectorfield, Vec2 d)
    {
        var flowOperator = IFlowOperator<Vec2, Vec3>.Default;

        var trajLeft = flowOperator.Compute(t_start, t_end, x + new Vec2(d.X, 0), vectorfield);
        var trajRight = flowOperator.Compute(t_start, t_end, x + new Vec2(-d.X, 0), vectorfield);
        var trajUp = flowOperator.Compute(t_start, t_end, x + new Vec2(0, d.Y), vectorfield);
        var trajDown = flowOperator.Compute(t_start, t_end, x + new Vec2(0, -d.Y), vectorfield);

        var end_left = trajLeft.Entries[^1];
        var end_right = trajRight.Entries[^1];
        var end_up = trajUp.Entries[^1];
        var end_down = trajDown.Entries[^1];


        Matrix2 gradient = new Matrix2(
            (end_left.X - end_right.X) / d.X,
            (end_down.X - end_up.X) / d.Y,
            (end_left.Y - end_right.Y) / d.X,
            (end_down.Y - end_up.Y) / d.Y
        );

        var delta = gradient * gradient.Transposed();

        var m = delta.Trace * .5f;
        var p = delta.Determinant;
        var n = m * m - p;

        if (n < 1e-05)
            n = 0;

        var right = float.Sqrt(n);
        var max_eigen = float.Max(m + right, m - right);
        var ftle = (1f / float.Abs(t_end - t_start)) * float.Log(float.Sqrt(max_eigen));
        return ftle;
    }
}