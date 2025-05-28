namespace FlowExplainer;

public interface IVectorField<TInput, TOutput>
{
    TOutput Evaluate(TInput x);
}