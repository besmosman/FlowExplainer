using ImGuiNET;
using static System.Single;

namespace FlowExplainer;

public class SpeetjensVelocityField : IVectorField<Vec3, Vec2>
{
    public float epsilon = 0f;
    public float Period => 1;

    public IDomain<Vec3> Domain => new RectDomain<Vec3>(new Vec3(0, 0, 0), new Vec3(1, .5f, 1));

    public Vec2 Evaluate(Vec3 x)
    {
        return Velocity(x.X, x.Y, x.Z);
    }

    public bool TryEvaluate(Vec3 x, out Vec2 value)
    {
        value = Velocity(x.X, x.Y, x.Z);
        return true;
    }

    public void OnImGuiEdit()
    {
        ImGuiHelpers.SliderFloat("Epsilon", ref epsilon, 0, 2);
    }

    public Vec2 Velocity(float x, float y, float t)
    {
        var D = 0.5f;
        var K = 2f;
        var L = epsilon * Sin(2 * Pi * t);
        var u = Sin(K * Pi * (x - L)) * Cos(Pi * y / D);
        var v = -Cos(K * Pi * (x - L)) * Sin(Pi * y / D);
        return new Vec2(u, v);
    }
}