namespace FlowExplainer;

public class RungeKutta4IntegratorBaseGen<TInput> : IIntegrator<TInput, TInput> where TInput : IVec<TInput, double>
{
    public TInput Integrate(IVectorField<TInput, TInput> f, TInput p, double dt)
    {
        if (f.TryEvaluate(p, out var k1) &&
            f.TryEvaluate((p + dt * (k1 / 2.0)), out var k2) &&
            f.TryEvaluate((p + dt * (k2 / 2.0)), out var k3) &&
            f.TryEvaluate((p + dt * (k3)), out var k4)
           )
        {
            return p + (dt / 6.0) * (k1 + 2.0 * k2 + 2.0 * k3 + k4);
        }
        return p + dt * f.Evaluate(p);
    }
}

public class RungeKutta4IntegratorGen<TPhase, TSpace> : IIntegrator<TPhase, TSpace>
    where TPhase : IVec<TPhase, double>, IVecDownDimension<TSpace>
    where TSpace : IVec<TSpace, double>, IVecUpDimension<TPhase>
{

    public static readonly RungeKutta4IntegratorGen<TPhase, TSpace> Instance = new();

    public TPhase Integrate(IVectorField<TPhase, TSpace> f, TPhase x, double dt)
    {
        TSpace p = x.Down();
        double t = x.Last;

        if (f.TryEvaluate(p.Up(t), out var k1) &&
            f.TryEvaluate((p + dt * (k1 / 2.0)).Up(t + dt / 2.0), out var k2) &&
            f.TryEvaluate((p + dt * (k2 / 2.0)).Up(t + dt / 2.0), out var k3) &&
            f.TryEvaluate((p + dt * (k3)).Up(t + dt), out var k4)
           )
        {
            return (p + (dt / 6.0) * (k1 + 2 * k2 + 2 * k3 + k4)).Up(t + dt);
        }
        return (p + dt * f.Evaluate(p.Up(t))).Up(t + dt);
    }
}