namespace FlowExplainer;

public static class BoundingFunctions
{
    public static IBounding<Vec3> PeriodicXPeriodicZ(Rect<Vec3> rect)
    {
        return new BoundingPeriodicXyPeriodicZ(rect);
    }
    public static IBounding<Vec> None<Vec>() where Vec : IVec<Vec, double>
    {
        return new BoundingNone<Vec>();
    }

    public static IBounding<Vec> Build<Vec>(BoundaryType[] boundaries, Rect<Vec> rect) where Vec : IVec<Vec, double>
    {
        return new GenBounding<Vec>(boundaries, rect);
    }
}