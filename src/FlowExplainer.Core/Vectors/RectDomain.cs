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

    public void MakeFinalAxisSlice(double t_start, double t_end, double new_t_start = 0, double new_t_end = 1)
    {
        Rect.Min[Rect.Min.ElementCount - 1] = new_t_start;
        Rect.Max[Rect.Min.ElementCount - 1] = new_t_end;
        Bounding = new LastSliceBounding<Vec>(Bounding, t_start, t_end, new_t_start, new_t_end);
    }
    public void MakeFinalAxisPeriodicSlice(double t, double period)
    {
        Rect.Max[Rect.Min.ElementCount - 1] = period;
        Rect.Min[Rect.Min.ElementCount - 1] = 0;
        Bounding = new LastPeriodicBounding<Vec>(Bounding, t, period);
    }
    
    public void MakeFinalAxisPeriodic()
    {
        Bounding = new LastPeriodicBounding<Vec>(Bounding, Rect.Min[Rect.Min.ElementCount - 1], Rect.Max[Rect.Min.ElementCount - 1]);
    }
}