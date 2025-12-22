using OpenTK.Mathematics;

namespace FlowExplainer;

public static class FTLEComputer
{
    public static double Compute(Vec2 x, double t_start, double t_end, IVectorField<Vec3, Vec2> vectorfield, Vec2 d)
    {
        var flowOperator = IFlowOperator<Vec2, Vec3>.Default;
        
        var end_left =  flowOperator.ComputeEnd(t_start, t_end, x + new Vec2(d.X, 0), vectorfield);
        var end_right = flowOperator.ComputeEnd(t_start, t_end, x + new Vec2(-d.X, 0), vectorfield);
        var end_up = flowOperator.ComputeEnd(t_start, t_end, x + new Vec2(0, d.Y), vectorfield);
        var end_down = flowOperator.ComputeEnd(t_start, t_end, x + new Vec2(0, -d.Y), vectorfield);


        Matrix2d gradient = new Matrix2d(
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

        var right = double.Sqrt(n);
        var max_eigen = double.Max(m + right, m - right);
        var ftle = (1f / double.Abs(t_end - t_start)) * double.Log(double.Sqrt(max_eigen));
        return ftle;
    }
}