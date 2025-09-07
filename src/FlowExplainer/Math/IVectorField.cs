using System.Diagnostics.CodeAnalysis;

namespace FlowExplainer;

/*
public interface IEditabalePeriodicVectorField<TInput, TOutput> : IPeriodicVectorField<TInput, TOutput>
{
    public void OnImGuiEdit();
}

public interface IPeriodicVectorField<TInput, TOutput> : IVectorField<TInput, TOutput> where TInput : IVec<TInput>
{
    public float Period { get; }
}
*/

public interface IVectorField<TInput, TOutput> where TInput : IVec<TInput>
{
    
    /// <summary>
    /// Get value at point, exception if outside of bounds.
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    TOutput Evaluate(TInput x);
    
    bool TryEvaluate(TInput x, [MaybeNullWhen(false)] out TOutput value);

    void OnImGuiEdit()
    {
    }

    public IDomain<TInput> Domain { get; }

    public static IVectorField<TInput, TOutput> Constant(TOutput value) => new ConstantField<TInput, TOutput>(value);

    private readonly struct ConstantField<TInput, TOutput> : IVectorField<TInput, TOutput> where TInput : IVec<TInput>
    {
        private readonly TOutput Value;

        public ConstantField(TOutput value)
        {
            Value = value;
        }

        public TOutput Evaluate(TInput x)
        {
            return Value;
        }

        public bool TryEvaluate(TInput x, out TOutput v)
        {
            v = Value;
            return true;
        }

        public IDomain<TInput> Domain => IDomain<TInput>.Infinite;
    }
}