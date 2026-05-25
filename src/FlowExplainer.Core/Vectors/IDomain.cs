namespace FlowExplainer;

public interface IDomain<Vec> where Vec : IVec<Vec, double>
{
    bool IsWithinBounds(Vec p);
    Rect<Vec> RectBoundary { get; }

    public IBounding<Vec> Bounding { get; }
    public static IDomain<Vec> Infinite => new InfiniteDomain();


    private struct InfiniteDomain : IDomain<Vec>
    {
        public bool IsWithinBounds(Vec p) => true;
        public Rect<Vec> RectBoundary => throw new Exception();
        public IBounding<Vec> Bounding => BoundingFunctions.None<Vec>();
    }
}

public class DomainSlice<TUp, TDown> : IDomain<TDown>
    where TDown : IVec<TDown, double>, IVecUpDimension<TUp>
    where TUp : IVec<TUp, double>, IVecDownDimension<TDown>
{
    public IDomain<TUp> oriDomain;
    public Func<double> Time;

    public DomainSlice(IDomain<TUp> oriDomain, Func<double> time)
    {
        this.oriDomain = oriDomain;
        Time = time;
    }

    public DomainSlice(IDomain<TUp> oriDomain)
    {
        this.oriDomain = oriDomain;
        double centerLast = oriDomain.RectBoundary.Center.Last;
        Time = () => centerLast;
    }

    public bool IsWithinBounds(TDown p)
    {
        return oriDomain.IsWithinBounds(p.Up(Time()));
    }

    public Rect<TDown> RectBoundary => oriDomain.RectBoundary.Reduce<TDown>();
    public IBounding<TDown> Bounding => new BoundingDownDim<TUp, TDown>(oriDomain.Bounding, Time());
}

public static class DomainExtentions
{
    extension<TUp, TDown>(IDomain<TUp> domain)
        where TUp : IVec<TUp, double>, IVecDownDimension<TDown> where TDown : IVec<TDown, double>, IVecUpDimension<TUp>
    {
        public IDomain<TDown> ReducedSlice(Func<double> time)
        {
            return new DomainSlice<TUp, TDown>(domain, time);
        }

        public IDomain<TDown> ReducedSlice()
        {
            return new DomainSlice<TUp, TDown>(domain);
        }
    }

    extension<TUp, TDown, TData>(IVectorField<TUp, TData> vectorField)
        where TUp : IVec<TUp, double>, IVecDownDimension<TDown> where TDown : IVec<TDown, double>, IVecUpDimension<TUp>
    {
        public IVectorField<TDown, TData> ReducedSlice(Func<double> time)
        {
            return new VectorfieldSlice<TUp, TDown, TData>(vectorField, time);
        }
    }
}