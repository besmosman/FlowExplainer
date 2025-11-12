namespace FlowExplainer;

public class BickleyJet : IVectorField<Vec3, Vec2>
{
    public float M = 0.001f;
    public float v = 1.5e-5f;
    public float p = 1.225f;

    public float Period => 1;
    public IDomain<Vec3> Domain => new RectDomain<Vec3>(new Vec3(.1f, -.2f, 0), new Vec3(5f, .2f, 1));


    public float sech(float x)
    {
        return 1f / Single.Cosh(x);
    }

    public Vec2 Evaluate(Vec3 phase)
    {
        float x = phase.X;
        float y = phase.Y;
        var ξ = 0.2752f * Single.Pow(M / (v * v * p), 1f / 3f) * y / Single.Pow(x, 2f / 3f);
        var vel_x = 0.4543f * Single.Pow((M * M) / (v * p * p * x), 1f / 3f) * sech(ξ) * sech(ξ);
        var vel_y = 0.5503f * Single.Pow((M * v) / (p * x * x), 1f / 3f) * (2 * ξ * sech(ξ) * sech(ξ) - Single.Tanh(ξ));
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
        ImGuiHelpers.SliderFloat("M", ref M, 0, .1f);
    }
}