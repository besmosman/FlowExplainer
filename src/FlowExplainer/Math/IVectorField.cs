namespace FlowExplainer;

public interface IEditabalePeriodicVectorField<TInput, TOutput> : IPeriodicVectorField<TInput, TOutput>
{
    public void OnImGuiEdit();
}

public interface IPeriodicVectorField<TInput, TOutput> : IVectorField<TInput, TOutput>
{
    public float Period { get; }
    public Rect Domain { get; }
}

public interface IVectorField<TInput, TOutput>
{
    TOutput Evaluate(TInput x);
}