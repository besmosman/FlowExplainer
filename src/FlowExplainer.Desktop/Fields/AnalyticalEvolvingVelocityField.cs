using ImGuiNET;
using static System.Double;

namespace FlowExplainer;

//https://shaddenlab.berkeley.edu/uploads/LCS-tutorial/examples.html#x1-1200812
public class AnalyticalEvolvingVelocityField : IVectorField<Vec3, Vec2>
{
    public double epsilon = .1f;
    public double A = 1f;
    public double w = 0.002f;

    public double Period => (2f * Pi) / w;
    public IDomain<Vec3> Domain => new RectDomain<Vec3>(new Vec3(0, 0, 0), new Vec3(2, 1f, Period), BoundingFunctions.None<Vec3>());

    public string DisplayName { get; set; } = "Double Gyre";

    public Vec3 Wrap(Vec3 x)
    {
        var r = x;
        r.X %= 2;
        r.Y = Clamp(r.Y, 0, 1);
        return r;
    }

    public bool TryEvaluate(Vec3 x, out Vec2 value)
    {
        value = Velocity(x.X, x.Y, x.Z);
        return true;
    }

    public void OnImGuiEdit()
    {
        ImGuiHelpers.SliderFloat("A", ref A, 0, 10);
        ImGuiHelpers.SliderFloat("Epsilon", ref epsilon, 0, 2);
        ImGuiHelpers.SliderFloat("w", ref w, 0, 2);
    }

    double streamFunction(Vec3 phase)
    {
        double x = phase.X;
        double y = phase.Y;
        double t = phase.Z;
        return A * Sin(Pi * f(x, t)) * Sin(Pi * y);
    }

    double a(double t)
    {
        return epsilon * Sin(w * t);
    }

    double b(double t)
    {
        return 1f - 2 * epsilon * Sin(w * t);
    }

    public double f(double x, double t)
    {
        return a(t) * x * x + b(t) * x;
    }

    public Vec2 Evaluate(Vec3 x)
    {
        /*
        var d = .001f;
        var dx = (streamFunction(x + new Vec3(-d, 0, 0)) - streamFunction(x + new Vec3(d, 0, 0))) / (2*d);
        var dy = (streamFunction(x + new Vec3(0, -d, 0)) - streamFunction(x + new Vec3(0, d, 0))) / (2*d);
        if(x.X > 1)
        return new Vec2(-dy,dx);
        */

        return Velocity(x.X, x.Y, x.Z);
    }

    public Vec2 Velocity(double x, double y, double t)
    {
        var u = -Pi * A * Sin(Pi * f(x, t)) * Cos(Pi * y);
        var dfdx = 2 * a(t) * x + b(t);
        var v = Pi * A * Cos(Pi * f(x, t)) * Sin(Pi * y) * dfdx;
        return new Vec2(u, v);
    }
}