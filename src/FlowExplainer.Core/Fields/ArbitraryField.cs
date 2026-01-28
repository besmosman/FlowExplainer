namespace FlowExplainer;

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
    
}