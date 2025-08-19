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
    TOutput Evaluate(TInput x);
    void OnImGuiEdit() { }
    public IDomain<TInput> Domain { get; }

    public static IVectorField<TInput, TOutput> Constant(TOutput value) => new ConstantField<TInput, TOutput>(value);
    
    private readonly struct ConstantField<TInput, TOutput> : IVectorField<TInput, TOutput> where TInput : IVec<TInput>
    {
        private readonly TOutput Value;
        
        public ConstantField(TOutput value)
        {
            Value = value;
        }
        public TOutput Evaluate(TInput x) => Value;
        public IDomain<TInput> Domain => IDomain<TInput>.Infinite;
    }
}
