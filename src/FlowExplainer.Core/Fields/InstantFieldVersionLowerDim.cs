using System.Diagnostics.CodeAnalysis;

namespace FlowExplainer;

/*
public struct InstantFieldVersionLowerDim<VecOri, VecNew, Data> : IVectorField<VecNew, Data>
    where VecNew : IVec<VecNew, double>, IVecUpDimension<VecOri>
    where VecOri : IVec<VecOri, double>, IVecDownDimension<VecNew>
{
    public double Time { get; private set; }
    public IDomain<VecNew> Domain { get; set; }
    public IBounding<VecNew> Bounding { get; set; }

    private readonly IVectorField<VecOri, Data> orifield;

    public InstantFieldVersionLowerDim(IVectorField<VecOri, Data> orifield, double time)
    {
        this.orifield = orifield;
        Time = time;
        Domain = orifield.Domain.ReducedSlice<VecOri,VecNew>(Time);
    }
    public Data Evaluate(VecNew x)
    {
        return orifield.Evaluate(x.Up(Time));
    }

    public bool TryEvaluate(VecNew x, [MaybeNullWhen(false)] out Data value)
    {
        return orifield.TryEvaluate(x.Up(Time), out value);
    }

}*/
    public struct BoundingDownDim<VecOri,VecNew> : IBounding<VecNew>
        where VecNew : IVec<VecNew, double>, IVecUpDimension<VecOri>
        where VecOri : IVec<VecOri, double>, IVecDownDimension<VecNew>
    {
        private IBounding<VecOri> oriBounding;
        private double Time;
        public BoundingDownDim(IBounding<VecOri> oriBounding, double time)
        {
            this.oriBounding = oriBounding;
            Time = time;
        }

        public VecNew Bound(VecNew x)
        {
            return oriBounding.Bound(x.Up(Time)).Down();
        }
        public double ShortestSpatialDistanceSqrt(VecNew a, VecNew b) => oriBounding.ShortestSpatialDistanceSqrt(a.Up(Time), b.Up(Time));
    }

public struct InstantField<VecOri, Data> : IVectorField<VecOri, Data>
    where VecOri : IVec<VecOri, double>
{
    public double Time { get; set; }
    public IDomain<VecOri> Domain => orifield.Domain;

    private readonly IVectorField<VecOri, Data> orifield;

    public InstantField(IVectorField<VecOri, Data> orifield, double time)
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