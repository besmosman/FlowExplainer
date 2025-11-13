using MemoryPack;
using static System.Double;

namespace FlowExplainer;

//https://coherentstructures.github.io/CoherentStructures.jl/stable/generated/bickley/
public class BickleyJet2 : IVectorField<Vec3, Vec2>
{
    public double Period => 1;
    public IDomain<Vec3> Domain => new RectDomain<Vec3>(new Vec3(0, -3, 0), new Vec3(20, 3f, 100000000));
    public IBounding<Vec3> Bounding { get; } = BoundingFunctions.None<Vec3>();

    double r0 = 4371e-3f;
    double U0 = 62.66e-3f;
    double ε1 = 0.075f;
    double ε2 = 0.4f;
    double ε3 = 0.3f;
    double L0 = 1770e-3f;

    public double streamFunction(Vec3 phase)
    {
        double k1 = 2f / r0;
        double k2 = 4f / r0;
        double k3 = 6f / r0;

        double c2 = 0.205f * U0;
        double c3 = 0.461f * U0;
        double c1 = c3 + ((Sqrt(5) - 1f)/2f) * (k2/k1) * (c2- c3);
        var x = phase.X;
        var y = phase.Y;
        var t = phase.Z;

        var psi0 = -U0 * L0 * Tanh(y / L0);

        var Σ1 = ε1 * Cos(k1 * (x - c1 * t));
        var Σ2 = ε2 * Cos(k2 * (x - c2 * t));
        var Σ3 = ε3 * Cos(k3 * (x - c3 * t));
        var re_sum_term = Σ1 + Σ2 + Σ3;
        var psi1 = U0 * L0 * Pow(sech(y / L0), 2) * re_sum_term;
        var psi = psi0 + psi1;
        return psi;
    }
    
    public Vec2 Velocity(Vec3 phase)
    {
        double k1 = 2f / r0;
        double k2 = 4f / r0;
        double k3 = 6f / r0;

        double c2 = 0.205f * U0;
        double c3 = 0.461f * U0;
        double c1 = c3 + ((Sqrt(5) - 1f)/2f) * (k2/k1) * (c2- c3);
    
        var x = phase.X;
        var y = phase.Y;
        var t = phase.Z;

        // Common terms
        var tanh_term = Tanh(y / L0);
        var sech_term = sech(y / L0);
        var sech_squared = Pow(sech_term, 2);
    
        var Σ1 = ε1 * Cos(k1 * (x - c1 * t));
        var Σ2 = ε2 * Cos(k2 * (x - c2 * t));
        var Σ3 = ε3 * Cos(k3 * (x - c3 * t));
        var re_sum_term = Σ1 + Σ2 + Σ3;

        // u = ∂ψ/∂y
        var dpsi0_dy = -U0 * Pow(sech_term, 2);
    
        var dpsi1_dy = U0 * L0 * (-2f * sech_squared * tanh_term / L0) * re_sum_term;
    
        var u = dpsi0_dy + dpsi1_dy;

        // v = -∂ψ/∂x
        var dΣ1_dx = -ε1 * k1 * Sin(k1 * (x - c1 * t));
        var dΣ2_dx = -ε2 * k2 * Sin(k2 * (x - c2 * t));
        var dΣ3_dx = -ε3 * k3 * Sin(k3 * (x - c3 * t));
        var dre_sum_dx = dΣ1_dx + dΣ2_dx + dΣ3_dx;
    
        var dpsi1_dx = U0 * L0 * sech_squared * dre_sum_dx;
    
        var v = -dpsi1_dx; // Note: ∂ψ0/∂x = 0 since ψ0 only depends on y

        return new Vec2(u, v);
    }



    public double sech(double x)
    {
        return 1f / Cosh(x);
    }

    //source online differentiate tool
    public Vec2 Evaluate(Vec3 phase)
    {
        double r0 = 4371e-3f;
        double U0 = 62.66e-3f;
        double ε1 = 0.075f;
        double ε2 = 0.4f;
        double ε3 = 0.3f;
        double L0 = 1770e-3f;

        double k1 = 2f / r0;
        double k2 = 4f / r0;
        double k3 = 6f / r0;

        double c2 = 0.205f * U0;
        double c3 = 0.461f * U0;
        double c1 = c3 + ((Sqrt(5) - 1f)/2f) * (k2/k1) * (c2- c3);

        var x = phase.X;
        var y = phase.Y;
        var t = phase.Z;

        // Common terms
        double sech_y_L0 = sech(y / L0);
        double tanh_y_L0 = Tanh(y / L0);

        var Σ1 = ε1 * Cos(k1 * (x - c1 * t));
        var Σ2 = ε2 * Cos(k2 * (x - c2 * t));
        var Σ3 = ε3 * Cos(k3 * (x - c3 * t));
        var re_sum_term = Σ1 + Σ2 + Σ3;

        // u = ∂ψ/∂y
        double dpsi0_dy = -U0 * sech_y_L0 * sech_y_L0;
        double dpsi1_dy = -2f * U0 * sech_y_L0 * sech_y_L0 * tanh_y_L0 * re_sum_term / L0;
        double u = dpsi0_dy + dpsi1_dy;

        // v = -∂ψ/∂x  
        var dΣ1_dx = -ε1 * k1 * Sin(k1 * (x - c1 * t));
        var dΣ2_dx = -ε2 * k2 * Sin(k2 * (x - c2 * t));
        var dΣ3_dx = -ε3 * k3 * Sin(k3 * (x - c3 * t));
        var dre_sum_dx = dΣ1_dx + dΣ2_dx + dΣ3_dx;

        double v = U0 * L0 * sech_y_L0 * sech_y_L0 * dre_sum_dx;

        // return FiniteDifferences(phase);
        return new Vec2(u, v);
    }
    public Vec3 Wrap(Vec3 x)
    {
        return x;
    }

    public bool TryEvaluate(Vec3 x, out Vec2 value)
    {
        //value = FiniteDifferences(x);
        value = -Velocity(x);
        return true;
    }

    private Vec2 FiniteDifferences(Vec3 x)
    {
        var d = .0001f;
        var dx = (streamFunction(x + new Vec3(d, 0, 0)) - streamFunction(x + new Vec3(-d, 0, 0))) / (2 * d);
        var dy = (streamFunction(x + new Vec3(0, d, 0)) - streamFunction(x + new Vec3(0, -d, 0))) / (2 * d);
        return new Vec2(-dy, dx);
    }


    public void OnImGuiEdit()
    {
    }
}