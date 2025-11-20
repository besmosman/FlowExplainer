namespace FlowExplainer;

public class ArbitraryField<Veci, Data> : IVectorField<Veci, Data> where Veci : IVec<Veci>
{
    public Func<Veci, Data> eval;
    public string DisplayName { get; set; }

    public ArbitraryField(IDomain<Veci> domain, Func<Veci, Data> eval)
    {
        this.eval = eval;
        Domain = domain;
    }

    public IDomain<Veci> Domain { get; set; }

    public Data Evaluate(Veci x)
    {
        return eval(x);
    }
    
    public Veci Wrap(Veci x)
    {
        return x;
    }

    public bool TryEvaluate(Veci x, out Data value)
    {
        value = eval(x);
        return true;
    }
}