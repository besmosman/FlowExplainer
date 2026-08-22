namespace FlowExplainer;

public class PipeFlow : IVectorField<Vec3, Vec2>
{
    //matlab script gives:
    double d_s_out = 60.3e-3; // outer diameter steel pipe
    double delta_s = 2.9e-3; // thickness steel pipe
    double d_c_out = 125e-3; // outer diameter casing
    double delta_c = 3e-3; // thickness casing
    double U = 1e-2;

    public PipeFlow(IDomain<Vec3> domain)
    {
        Domain = domain;
    }

    double rw => d_s_out / 2 - delta_s;

    public IDomain<Vec3> Domain { get; set; }

    public Vec2 Evaluate(Vec3 x)
    {
        var r = x.Y;

        if (r < 0 || r > rw)
            return new Vec2(0, 0);

        double vZ = 2 * U * (1 - (r * r) / (rw * rw));

        return new Vec2(vZ, 0);
    }

    public bool TryEvaluate(Vec3 x, out Vec2 value)
    {
        value = Evaluate(x);
        return true;
    }
}