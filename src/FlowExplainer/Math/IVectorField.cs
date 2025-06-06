namespace FlowExplainer;

public interface IPeriodicVectorField<TInput, TOutput> : IVectorField<TInput, TOutput>
{
    public float Period { get; }
}

public interface IVectorField<TInput, TOutput>
{
    TOutput Evaluate(TInput x);
}