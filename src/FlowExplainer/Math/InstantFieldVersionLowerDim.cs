using System.Diagnostics.CodeAnalysis;

namespace FlowExplainer;

public struct InstantFieldVersionLowerDim<VecOri, VecNew, Data> : IVectorField<VecNew, Data>
    where VecNew : IVec<VecNew>, IVecUpDimension<VecOri>
    where VecOri : IVec<VecOri>, IVecDownDimension<VecNew>
{
    public float Time { get; private set; }
    public IDomain<VecNew> Domain { get; set; }
    public IBoundary<VecNew> Boundary { get; set; }

    private readonly IVectorField<VecOri, Data> orifield;

    public InstantFieldVersionLowerDim(IVectorField<VecOri, Data> orifield, float time)
    {
        this.orifield = orifield;
        Time = time;
        Domain = new RectDomain<VecNew>(orifield.Domain.RectBoundary.Reduce<VecNew>());
        Boundary = new BoundaryDownDim(orifield.Boundary, Time);
    }
    public Data Evaluate(VecNew x)
    {
        return orifield.Evaluate(x.Up(Time));
    }

    public bool TryEvaluate(VecNew x, [MaybeNullWhen(false)] out Data value)
    {
        return orifield.TryEvaluate(x.Up(Time), out value);
    }

    public class BoundaryDownDim : IBoundary<VecNew>
    {
        private IBoundary<VecOri> OriBoundary;
        private float Time;
        public BoundaryDownDim(IBoundary<VecOri> oriBoundary, float time)
        {
            OriBoundary = oriBoundary;
            Time = time;
        }

        public VecNew Wrap(VecNew x)
        {
            return OriBoundary.Wrap(x.Up(Time)).Down();
        }
    }
}

public struct InstantField<VecOri, Data> : IVectorField<VecOri, Data>
    where VecOri : IVec<VecOri>
{
    public float Time { get; set; }
    public IDomain<VecOri> Domain => orifield.Domain;
    public IBoundary<VecOri> Boundary => orifield.Boundary;

    private readonly IVectorField<VecOri, Data> orifield;

    public InstantField(IVectorField<VecOri, Data> orifield, float time)
    {
        this.orifield = orifield;
        Time = time;
    }

    public Data Evaluate(VecOri x)
    {
        x[x.ElementCount - 1] = Time;
        return orifield.Evaluate(x);
    }

    public bool TryEvaluate(VecOri x, [MaybeNullWhen(false)] out Data value)
    {
        x[x.ElementCount - 1] = Time;
        return orifield.TryEvaluate(x, out value);
    }
}