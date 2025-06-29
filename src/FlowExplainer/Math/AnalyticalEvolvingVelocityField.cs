using ImGuiNET;
using static System.Single;

namespace FlowExplainer;

//https://shaddenlab.berkeley.edu/uploads/LCS-tutorial/examples.html#x1-1200812
public class AnalyticalEvolvingVelocityField : IEditabalePeriodicVectorField<Vec3, Vec2>
{
    public float elipson = 0f;
    public float A = 1f;
    public float w = 1f;

    public float Period => (2f * Pi) / w;
    public Rect Domain => new Rect(new Vec2(0, 0), new Vec2(2, 1));

    public void OnImGuiEdit()
    {
        ImGuiHelpers.SliderFloat("A", ref A, 0, 10);
        ImGuiHelpers.SliderFloat("Elipson", ref elipson, 0, 2);
        ImGuiHelpers.SliderFloat("w", ref w, 0, 2);
    }

    float streamFunction(Vec3 phase)
    {
        float x = phase.X;
        float y = phase.Y;
        float t = phase.Z;
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
        /*
        var d = .001f;
        var dx = (streamFunction(x + new Vec3(-d, 0, 0)) - streamFunction(x + new Vec3(d, 0, 0))) / (2*d);
        var dy = (streamFunction(x + new Vec3(0, -d, 0)) - streamFunction(x + new Vec3(0, d, 0))) / (2*d);
        if(x.X > 1)
        return new Vec2(-dy,dx);
        */
        
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