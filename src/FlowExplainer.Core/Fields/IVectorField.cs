using System.Diagnostics.CodeAnalysis;
using System.Numerics;

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

public struct SelectableVectorField<TInput, TOutput> : ISelectableVectorField<TInput, TOutput> where TInput : IVec<TInput, double>
{
    public SelectableVectorField(string displayName, IVectorField<TInput, TOutput> vectorField)
    {
        DisplayName = displayName;
        VectorField = vectorField;
    }

    public string DisplayName { get; init; }
    public IVectorField<TInput, TOutput> VectorField { get; init; }
}

public interface ISelectableVectorField<TInput, TOutput> where TInput : IVec<TInput, double>
{
    public string DisplayName { get; }
    public IVectorField<TInput, TOutput> VectorField { get; }
}

public interface IArtifact
{
    public string DisplayName { get; }
    public string Description { get; }
    public Type ValueType { get; }
    public object ValueObj { get; set; }
}

public class Artifact<T> : IArtifact
{
    public T Value { get; set; }
    public int Version { get; set; }
    public object ValueObj { get => Value; set => Value = (T)value; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public Type ValueType => typeof(T);

    public Artifact(T value, string displayName, string description)
    {
        Value = value;
        DisplayName = displayName;
        Description = description;
    }
}

public class VectorfieldSlice<TUp, TDown, TOutput> : IVectorField<TDown, TOutput>
    where TDown : IVec<TDown, double>, IVecUpDimension<TUp>
    where TUp : IVec<TUp, double>, IVecDownDimension<TDown>
{
    public readonly IVectorField<TUp, TOutput> VectorField;
    public IDomain<TDown> Domain { get; }
    public Func<double> Time;

    public VectorfieldSlice(IVectorField<TUp, TOutput> vectorField, Func<double> time)
    {
        VectorField = vectorField;
        Time = time;
        Domain = VectorField.Domain.ReducedSlice<TUp, TDown>(time);
    }


    public TOutput Evaluate(TDown x)
    {
        return VectorField.Evaluate(x.Up(Time()));
    }

    public bool TryEvaluate(TDown x, [MaybeNullWhen(false)] out TOutput value)
    {
        return VectorField.TryEvaluate(x.Up(Time()), out value);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(VectorField.GetHashCode(), Time().GetHashCode());
    }
}

public interface IVectorField<TInput, TOutput> where TInput : IVec<TInput, double>
{
    /// <summary>
    /// Get value at point, exception if outside of bounds.
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    TOutput Evaluate(TInput x);

    bool TryEvaluate(TInput x, [MaybeNullWhen(false)] out TOutput value);
    public virtual string DisplayName => "?";

    void OnImGuiEdit()
    {
    }

    public IDomain<TInput> Domain { get; }


    public static IVectorField<TInput, TOutput> Constant(TOutput value) => new ConstantField<TInput, TOutput>(value, IDomain<TInput>.Infinite);
    public static IVectorField<TInput, TOutput> Constant(TOutput value, IDomain<TInput> domain) => new ConstantField<TInput, TOutput>(value, domain);

    public static IVectorField<TInput, TOutput> Arbitrary(IDomain<TInput> domain, Func<TInput, TOutput> f) => new ArbitraryField<TInput, TOutput>(domain, f);

    private readonly struct ConstantField<TInput, TOutput> : IVectorField<TInput, TOutput> where TInput : IVec<TInput, double>
    {
        private readonly TOutput Value;
        private readonly IDomain<TInput> domain;
        public IDomain<TInput> Domain => domain;
        public IBounding<TInput> Bounding { get; } = BoundingFunctions.None<TInput>();
        public string DisplayName { get; } = "Constant";

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