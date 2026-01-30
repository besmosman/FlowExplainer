namespace FlowExplainer;

public class BickleyJet : IVectorField<Vec3, Vec2>
{
    public double M = 0.001f;
    public double v = 1.5e-5f;
    public double p = 1.225f;

    public double Period => 1;
    public IDomain<Vec3> Domain => new RectDomain<Vec3>(new Vec3(.1f, -.2f, 0), new Vec3(5f, .2f, 1));


    public double sech(double x)
    {
        return 1f / double.Cosh(x);
    }

    public Vec2 Evaluate(Vec3 phase)
    {
        double x = phase.X;
        double y = phase.Y;
        var ξ = 0.2752f * double.Pow(M / (v * v * p), 1f / 3f) * y / double.Pow(x, 2f / 3f);
        var vel_x = 0.4543f * double.Pow((M * M) / (v * p * p * x), 1f / 3f) * sech(ξ) * sech(ξ);
        var vel_y = 0.5503f * double.Pow((M * v) / (p * x * x), 1f / 3f) * (2 * ξ * sech(ξ) * sech(ξ) - double.Tanh(ξ));
        return new Vec2(vel_x, vel_y);
    }
    public Vec3 Wrap(Vec3 x)
    {
        return x;
    }

    public bool TryEvaluate(Vec3 x, out Vec2 value)
    {
        value = Evaluate(x);
        return true;
    }

    public void OnImGuiEdit()
    {
        ImGuiHelpers.Slider("M", ref M, 0, .1f);
    }
}