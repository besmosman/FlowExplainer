namespace FlowExplainer;

public struct RectDomain<Vec> : IDomain<Vec> where Vec : IVec<Vec>
{
    public Rect<Vec> Rect;

    public Vec MinPos => Rect.Min;
    public Vec MaxPos => Rect.Max;

    public Rect<Vec> RectBoundary => Rect;
    public IBounding<Vec> Bounding { get; set; }

    public RectDomain(Vec min, Vec max, IBounding<Vec>? bounding = null)
    {
        Bounding = bounding ?? BoundingFunctions.None<Vec>();
        Rect = new(min, max);
    }

    public RectDomain(Rect<Vec> rect, IBounding<Vec>? bounding = null)
    {
        Rect = rect;
        Bounding = bounding ?? BoundingFunctions.None<Vec>();
    }


    public bool IsWithinPhase(Vec p)
    {
        return p > Rect.Min && p < Rect.Max;
    }
    public bool IsWithinSpace<T>(T p) where T : IVec<T>
    {
#if DEBUG
        if (p.ElementCount != Rect.Min.ElementCount - 1)
            throw new Exception("Check dimensions");
#endif

        for (int i = 0; i < p.ElementCount; i++)
        {
            if (p[i] < Rect.Min[i] || p[i] > Rect.Max[i])
                return false;
        }
        return true;
    }

    public void MakeFinalAxisPeriodicSlice(double t, double period)
    {
        Rect.Max[Rect.Min.ElementCount - 1] = period;
        Rect.Min[Rect.Min.ElementCount - 1] = 0;
        Bounding = new LastPeriodicBounding<Vec>(Bounding, t, period);
    }
}