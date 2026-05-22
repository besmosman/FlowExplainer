using System.Diagnostics.CodeAnalysis;

namespace FlowExplainer;

public class DelayedField<Vec, Data> : IVectorField<Vec, Data> where Vec : IVec<Vec, double>
{
    private Func<IVectorField<Vec, Data>> builder;
    private IVectorField<Vec, Data>? Field;

    public DelayedField(Func<IVectorField<Vec, Data>> builder)
    {
        this.builder = builder;
    }

    public Data Evaluate(Vec x)
    {
        Field ??= builder();
        return Field.Evaluate(x);
    }

    public bool TryEvaluate(Vec x, [MaybeNullWhen(false)] out Data value)
    {
        Field ??= builder();
        return Field.TryEvaluate(x, out value);
    }

    public IDomain<Vec> Domain
    {
        get
        {
            Field ??= builder();
            return Field.Domain;
        }
    }

}

public class ArbitraryField<Vec, Data> : IVectorField<Vec, Data> where Vec : IVec<Vec, double>
{
    public Func<Vec, Data> eval;
    public string DisplayName { get; set; }

    public ArbitraryField(IDomain<Vec> domain, Func<Vec, Data> eval)
    {
        this.eval = eval;
        Domain = domain;
    }

    public IDomain<Vec> Domain { get; set; }

    public Data Evaluate(Vec x)
    {
        return eval(x);
    }

    public Vec Wrap(Vec x)
    {
        return x;
    }

    public bool TryEvaluate(Vec x, out Data value)
    {
        value = eval(x);
        return true;
    }


    public IVectorField<Vec, D> Select<D>(Func<Data, D> selector)
    {
        return new ArbitraryField<Vec, D>(Domain, p => selector(Evaluate(p)));
    }

    public IVectorField<Vec, D> Select<D>(Func<Vec, Data, D> selector)
    {
        return new ArbitraryField<Vec, D>(Domain, p => selector(p, Evaluate(p)));
    }
}