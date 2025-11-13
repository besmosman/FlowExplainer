namespace FlowExplainer;

public interface IIntegrator<TInput, TOutput>
    where TInput : IVec<TInput>, IVecDownDimension<TOutput>
    where TOutput : IVec<TOutput>, IVecUpDimension<TInput>
{
    TOutput Integrate(IVectorField<TInput, TOutput> f, TInput x, double dt);

    public static IIntegrator<TInput, TOutput> Rk4 { get; } = new RungeKutta4IntegratorGen<TInput, TOutput>();
}