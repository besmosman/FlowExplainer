using static System.Single;

namespace FlowExplainer;

//https://shaddenlab.berkeley.edu/uploads/LCS-tutorial/examples.html#x1-1200812
public class AnalyticalEvolvingVelocityField : IPeriodicVectorField<Vec3, Vec2>
{
    public float elipson = 1f;
    public float A = 1f;
    public float w = 1f;

    public float Period => (2f * Pi) / w;

    float streamFunction(float x, float y, float t)
    {
        return A * Sin(Pi * f(x, t)) * Sin(Pi * y);
    }

    float a(float t)
    {
        return elipson * Sin(w * t);
    }

    float b(float t)
    {
        return 1f - 2 * elipson * Sin(w * t);
    }

    public float f(float x, float t)
    {
        return a(t) * x * x + b(t) * x;
    }

    public Vec2 Evaluate(Vec3 x)
    {
        return Velocity(x.X, x.Y, x.Z);
    }

    public Vec2 Velocity(float x, float y, float t)
    {
        var u = -Pi * A * Sin(Pi * f(x, t)) * Cos(Pi * y);
        var dfdx = 2 * a(t) * x + b(t);
        var v = Pi * A * Cos(Pi * f(x, t)) * Sin(Pi * y) * dfdx;
        return -new Vec2(u, v);
    }
}