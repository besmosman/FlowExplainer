using System.Diagnostics.CodeAnalysis;

namespace FlowExplainer;

/*
public interface IEditabalePeriodicVectorField<TInput, TOutput> : IPeriodicVectorField<TInput, TOutput>
{
    public void OnImGuiEdit();
}

public interface IPeriodicVectorField<TInput, TOutput> : IVectorField<TInput, TOutput> where TInput : IVec<TInput>
{
    public double Period { get; }
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

    public static IVectorField<TInput, TOutput> Constant(TOutput value) => new ConstantField<TInput, TOutput>(value, IDomain<TInput>.Infinite);
    public static IVectorField<TInput, TOutput> Constant(TOutput value, IDomain<TInput> domain) => new ConstantField<TInput, TOutput>(value, domain);

    private readonly struct ConstantField<TInput, TOutput> : IVectorField<TInput, TOutput> where TInput : IVec<TInput>
    {
        private readonly TOutput Value;
        private readonly IDomain<TInput> domain;
        public IDomain<TInput> Domain => domain;
        public IBounding<TInput> Bounding { get; } = BoundingFunctions.None<TInput>();

        public ConstantField(TOutput value, IDomain<TInput> domain)
        {
            Value = value;
            this.domain = domain;
        }

        public TOutput Evaluate(TInput x)
        {
            return Value;
        }

        public TInput Wrap(TInput x)
        {
            return x;
        }

        public bool TryEvaluate(TInput x, out TOutput v)
        {
            v = Value;
            return true;
        }

    }
}