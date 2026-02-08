namespace FlowExplainer;

public static class FD
{

    extension<Vec>(IVectorField<Vec, double> scalerfield) where Vec : IVec<Vec, double>
    {
        public Vec FiniteDifferenceGradient(Vec x, double h)
        {
            var d = Vec.Zero;
            for (int i = 0; i < x.ElementCount; i++)
            {
                var leftCoords = x;
                var rightCoords = x;
                leftCoords[i] -= h;
                rightCoords[i] += h;
                d[i] = (scalerfield.Evaluate(rightCoords) - scalerfield.Evaluate(leftCoords)) / (2 * h);
            }
            return d;
        }

        public VecR FiniteDifferenceGradientIgnoreLast<VecR>(Vec x, double h) where VecR : IVec<VecR, double>, IVecUpDimension<Vec>
        {
            var d = VecR.Zero;
            for (int i = 0; i < x.ElementCount - 1; i++)
            {
                var leftCoords = x;
                var rightCoords = x;
                leftCoords[i] -= h;
                rightCoords[i] += h;
                d[i] = (scalerfield.Evaluate(rightCoords) - scalerfield.Evaluate(leftCoords)) / (2 * h);
            }
            return d;
        }
        
    }

    public struct Neighbors(Vec2 left, Vec2 right, Vec2 up, Vec2 down, Vec2 delta)
    {
        public double dFx_dx => FD.Derivative(left.X, right.X, delta.X);
        public double dFy_dx => FD.Derivative(left.Y, right.Y, delta.X);
        public double dFx_dy => FD.Derivative(down.X, up.X, delta.Y);
        public double dFy_dy => FD.Derivative(down.Y, up.Y, delta.Y);
    }

    public static Neighbors CentralDifference(Vec2 center, Vec2 delta, Func<Vec2, Vec2> eval)
    {
        var left = eval(center - new Vec2(delta.X, 0));
        var right = eval(center + new Vec2(delta.X, 0));
        var up = eval(center + new Vec2(0, delta.Y));
        var down = eval(center - new Vec2(0, delta.Y));
        return new Neighbors(left, right, up, down, delta);
    }

    public static Neighbors CentralDifference(Vec2 left, Vec2 right, Vec2 up, Vec2 down, Vec2 delta)
    {
        return new Neighbors(left, right, up, down, delta);
    }

    public static double Derivative(double left, double right, double d)
    {
        return (right - left) / (2 * d);
    }

    public static Vec2 Derivative(Vec2 left, Vec2 right, Vec2 d)
    {
        return new Vec2(
            Derivative(left.X, right.X, d.X),
            Derivative(left.Y, right.Y, d.Y));
    }

    public static double Divergence(Vec2 left, Vec2 right, Vec2 up, Vec2 down, Vec2 d)
    {
        return FD.Derivative(left.X, right.X, d.X) + FD.Derivative(up.Y, down.Y, d.Y);
    }
}