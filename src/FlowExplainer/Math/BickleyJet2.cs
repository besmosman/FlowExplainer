namespace FlowExplainer;

//https://coherentstructures.github.io/CoherentStructures.jl/stable/generated/bickley/
public class BickleyJet2 : IVectorField<Vec3, Vec2>
{
    public float Period => 1;
    public IDomain<Vec3> Domain => new RectDomain<Vec3>(new Vec3(0, -3, 0), new Vec3(20, 3f, 1));

    float r0 = 4371e-3f;
    float U0 = 62.66e-3f;
    float ε1 = 0.0075f;
    float ε2 = 0.15f;
    float ε3 = 0.3f;
    float L0 = 1770e-3f;

    public float streamFunction(Vec3 phase)
    {
        float k1 = 2f / r0;
        float k2 = 4f / r0;
        float k3 = 6f / r0;

        float c2 = 0.205f * U0;
        float c3 = 0.461f * U0;
        float c1 = c3 + (Single.Sqrt(5) - 1f) * (c2 - c3);
        var x = phase.X;
        var y = phase.Y;
        var t = phase.Z;

        var psi0 = -U0 * L0 * Single.Tanh(y / L0);

        var Σ1 = ε1 * Single.Cos(k1 * (x - c1 * t));
        var Σ2 = ε2 * Single.Cos(k2 * (x - c2 * t));
        var Σ3 = ε3 * Single.Cos(k3 * (x - c3 * t));
        var re_sum_term = Σ1 + Σ2 + Σ3;
        var psi1 = U0 * L0 * Single.Pow(sech(y / L0), 2) * re_sum_term;
        var psi = psi0 + psi1;
        return psi;
    }

    public float sech(float x)
    {
        return 1f / Single.Cosh(x);
    }

    //source online differentiate tool
    public Vec2 Evaluate(Vec3 phase)
    {
        float r0 = 4371e-3f;
        float U0 = 62.66e-3f;
        float ε1 = 0.0075f;
        float ε2 = 0.15f;
        float ε3 = 0.3f;
        float L0 = 1770e-3f;

        float k1 = 2f / r0;
        float k2 = 4f / r0;
        float k3 = 6f / r0;

        float c2 = 0.205f * U0;
        float c3 = 0.461f * U0;
        float c1 = c3 + (Single.Sqrt(5) - 1f) * (c2 - c3);

        var x = phase.X;
        var y = phase.Y;
        var t = phase.Z;

        // Common terms
        float sech_y_L0 = sech(y / L0);
        float tanh_y_L0 = Single.Tanh(y / L0);

        var Σ1 = ε1 * Single.Cos(k1 * (x - c1 * t));
        var Σ2 = ε2 * Single.Cos(k2 * (x - c2 * t));
        var Σ3 = ε3 * Single.Cos(k3 * (x - c3 * t));
        var re_sum_term = Σ1 + Σ2 + Σ3;

        // u = ∂ψ/∂y
        float dpsi0_dy = -U0 * sech_y_L0 * sech_y_L0;
        float dpsi1_dy = -2f * U0 * sech_y_L0 * sech_y_L0 * tanh_y_L0 * re_sum_term / L0;
        float u = dpsi0_dy + dpsi1_dy;

        // v = -∂ψ/∂x  
        var dΣ1_dx = -ε1 * k1 * Single.Sin(k1 * (x - c1 * t));
        var dΣ2_dx = -ε2 * k2 * Single.Sin(k2 * (x - c2 * t));
        var dΣ3_dx = -ε3 * k3 * Single.Sin(k3 * (x - c3 * t));
        var dre_sum_dx = dΣ1_dx + dΣ2_dx + dΣ3_dx;

        float v = U0 * L0 * sech_y_L0 * sech_y_L0 * dre_sum_dx;

        // return FiniteDifferences(phase);
        return new Vec2(u, v);
    }

    private Vec2 FiniteDifferences(Vec3 x)
    {
        var d = .01f;
        var dx = (streamFunction(x + new Vec3(-d, 0, 0)) - streamFunction(x + new Vec3(d, 0, 0))) / (2 * d);
        var dy = (streamFunction(x + new Vec3(0, -d, 0)) - streamFunction(x + new Vec3(0, d, 0))) / (2 * d);
        return new Vec2(-dy, dx);
    }


    public void OnImGuiEdit()
    {
    }
}