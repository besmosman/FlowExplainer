namespace FlowExplainer;

public class LagrangianTransportFormalismService : WorldService
{
    public override string? Name => "LTF";

    public void RecomputeNonDimensional(IVectorField<Vec3, Vec2> u,
        IVectorField<Vec3, double> T,
        IVectorField<Vec3, double> T_reference,
        IVectorField<Vec3, double> Pe)
    {
        var T_conv = new ArbitraryField<Vec3, double>(T.Domain, x => T.Evaluate(x) - T_reference.Evaluate(x));
        var T_conv_grad = T_conv.FiniteDifferenceGradientIgnoreLastField(T.Domain.RectBoundary.Size.XY / 100.0);
        var q_prime = new ArbitraryField<Vec3, Vec2>(T.Domain, x => -(1.0 / Pe.Evaluate(x)) * T_conv_grad.Evaluate(x));
        var q_c = new ArbitraryField<Vec3, Vec2>(T.Domain, x => (u.Evaluate(x)) * T_conv.Evaluate(x));
        var q_total = new ArbitraryField<Vec3, Vec2>(T.Domain, x => q_prime.Evaluate(x) + q_c.Evaluate(x));

        Artifacts.RegisterOrUpdate(new Artifact<IVectorField<Vec3, Vec2>>
            (q_total, "LTF/flux/total", "Flux (net convective)", "LTF"));
        Artifacts.RegisterOrUpdate(new Artifact<IVectorField<Vec3, Vec2>>
            (q_c, "LTF/flux/conductive", "Flux (convection)", "LTF"));
        Artifacts.RegisterOrUpdate(new Artifact<IVectorField<Vec3, Vec2>>(q_prime,
            "LTF/flux/conductive", "Flux (conductive)", "LTF"));
        Artifacts.RegisterOrUpdate(new Artifact<IVectorField<Vec3, double>>(T_conv,
            "LTF/temperature/convective", "Temperature (relative)", "LTF"));
    }


    public void Recompute(IVectorField<Vec3, Vec2> Velocity,
        IVectorField<Vec3, double> Temperature,
        IVectorField<Vec3, double> ReferenceTemperature,
        IVectorField<Vec3, double> ThermalDiffusivity) // J/(m3·K)
    {
        var T_conv = new ArbitraryField<Vec3, double>(Temperature.Domain,
            x => Temperature.Evaluate(x) - ReferenceTemperature.Evaluate(x));
        var gridSize = ((RegularGridVectorField<Vec3, Vec3i, double>)Temperature).GridSize;
        var h = Temperature.Domain.RectBoundary.Size.X / gridSize.ToVec3().XY;
        h /= 2;

        var T_conv_grad = T_conv.FiniteDifferenceGradientIgnoreLastField(h);

        var q_prime = new ArbitraryField<Vec3, Vec2>(Temperature.Domain,
            x => -ThermalDiffusivity.Evaluate(x) * T_conv_grad.Evaluate(x));

        var q_c = new ArbitraryField<Vec3, Vec2>(Temperature.Domain,
            x => Velocity.Evaluate(x) * T_conv.Evaluate(x));

        var q_total = new ArbitraryField<Vec3, Vec2>(Temperature.Domain,
            x => q_prime.Evaluate(x) + q_c.Evaluate(x));

        Artifacts.RegisterOrUpdate(new Artifact<IVectorField<Vec3, Vec2>>
            (q_total, "LTF/flux/total", "Flux (net convective)", "LTF"));
        Artifacts.RegisterOrUpdate(new Artifact<IVectorField<Vec3, Vec2>>
            (q_c, "LTF/flux/convection", "Flux (convection)", "LTF"));
        Artifacts.RegisterOrUpdate(new Artifact<IVectorField<Vec3, Vec2>>(q_prime,
            "LTF/flux/conductive", "Flux (conductive)", "LTF"));
        Artifacts.RegisterOrUpdate(new Artifact<IVectorField<Vec3, double>>(T_conv,
            "LTF/temperature/convective", "Temperature (relative)", "LTF"));
    }

    public override void Initialize()
    {
        
    }

    public override void Draw(View view)
    {
    }
}