namespace FlowExplainer;

public class RungeKutta4IntegratorGen<TInput, TOutput> : IIntegrator<TInput, TOutput>
    where TInput : IVec<TInput>, IVecDownDimension<TOutput>
    where TOutput : IVec<TOutput>, IVecUpDimension<TInput>
{
    public TOutput Integrate(IVectorField<TInput, TOutput> f, TInput x, double dt)
    {
        TOutput p = x.Down();
        double t = x.Last;

        if (f.TryEvaluate(p.Up(t), out var k1) &&
            f.TryEvaluate((p + dt * (k1 / 2)).Up(t + dt / 2), out var k2) &&
            f.TryEvaluate((p + dt * (k2 / 2)).Up(t + dt / 2), out var k3) &&
            f.TryEvaluate((p + dt * (k3)).Up(t + dt), out var k4)
           )
        {
            return p + (dt / 6) * (k1 + 2 * k2 + 2 * k3 + k4);
        }
        
        return p + dt * f.Evaluate(p.Up(t));
        
    }
}