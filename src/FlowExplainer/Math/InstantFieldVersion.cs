using System.Diagnostics.CodeAnalysis;

namespace FlowExplainer;

public class InstantFieldVersion<VecOri, VecNew, Data> : IVectorField<VecNew, Data>
    where VecNew : IVec<VecNew>, IVecUpDimension<VecOri>
    where VecOri : IVec<VecOri>
{
    public float Time { get; set; }
    public IDomain<VecNew> Domain => new RectDomain<VecNew>(orifield.Domain.Boundary.Reduce<VecNew>());
    
    private readonly IVectorField<VecOri, Data> orifield;
    
    public InstantFieldVersion(IVectorField<VecOri, Data> orifield, float time)
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