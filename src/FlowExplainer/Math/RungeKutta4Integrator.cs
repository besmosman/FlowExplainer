namespace FlowExplainer;

public interface IIntegrator<TInput, TOutput>
    where TInput : IVec<TInput>, IVecDownDimension<TOutput>
    where TOutput : IVec<TOutput>, IVecUpDimension<TInput>
{
    TOutput Integrate(IVectorField<TInput, TOutput> f, TInput x, float dt);

    public static IIntegrator<TInput, TOutput> Rk4 { get; } = new RungeKutta4IntegratorGen<TInput, TOutput>();
}

public class RungeKutta4IntegratorGen<TInput, TOutput> : IIntegrator<TInput, TOutput>
    where TInput : IVec<TInput>, IVecDownDimension<TOutput>
    where TOutput : IVec<TOutput>, IVecUpDimension<TInput>
{

    public TOutput Integrate(IVectorField<TInput, TOutput> f, TInput x, float dt)
    {
        TOutput p = x.Down();
        float t = x.Last;

        var k1 = f.Evaluate(p.Up(t));
        var k2 = f.Evaluate((p + dt * (k1 / 2)).Up(t + dt / 2));
        var k3 = f.Evaluate((p + dt * (k2 / 2)).Up(t + dt / 2));
        var k4 = f.Evaluate((p + dt * (k3)).Up(t + dt));
        return p + (dt / 6) * (k1 + 2 * k2 + 2 * k3 + k4);
    }
}