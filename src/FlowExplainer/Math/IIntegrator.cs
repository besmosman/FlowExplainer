namespace FlowExplainer;

public interface IIntegrator<TInput, TOutput>
{
    TOutput Integrate(Func<TInput, TOutput> f, TInput x, float dt);
}