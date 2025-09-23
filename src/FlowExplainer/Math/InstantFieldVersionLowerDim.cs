using System.Diagnostics.CodeAnalysis;

namespace FlowExplainer;

public struct InstantFieldVersionLowerDim<VecOri, VecNew, Data> : IVectorField<VecNew, Data>
    where VecNew : IVec<VecNew>, IVecUpDimension<VecOri>
    where VecOri : IVec<VecOri>
{
    public float Time { get; set; }
    public IDomain<VecNew> Domain => new RectDomain<VecNew>(orifield.Domain.Boundary.Reduce<VecNew>());
    
    private readonly IVectorField<VecOri, Data> orifield;
    
    public InstantFieldVersionLowerDim(IVectorField<VecOri, Data> orifield, float time)
    {
        this.orifield = orifield;
        Time = time;
    }
    public Data Evaluate(VecNew x)
    {
        return orifield.Evaluate(x.Up(Time));
    }
    public bool TryEvaluate(VecNew x, [MaybeNullWhen(false)] out Data value)
    {
        return orifield.TryEvaluate(x.Up(Time), out value);
    }
}

public struct InstantField<VecOri, Data> : IVectorField<VecOri, Data>
    where VecOri : IVec<VecOri>
{
    public float Time { get; set; }
    public IDomain<VecOri> Domain => orifield.Domain;
    
    private readonly IVectorField<VecOri, Data> orifield;
    
    public InstantField(IVectorField<VecOri, Data> orifield, float time)
    {
        this.orifield = orifield;
        Time = time;
    }
    
    public Data Evaluate(VecOri x)
    {
        x[x.ElementCount-1] = Time;
        return orifield.Evaluate(x);
    }
    public bool TryEvaluate(VecOri x, [MaybeNullWhen(false)] out Data value)
    {
        x[x.ElementCount-1] = Time;
        return orifield.TryEvaluate(x, out value);
    }
}