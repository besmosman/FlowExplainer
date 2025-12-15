namespace FlowExplainer;


public interface IDomain<Vec> where Vec : IVec<Vec, double>
{
    bool IsWithinPhase(Vec p);
    bool IsWithinSpace<T>(T p) where T : IVec<T, double>;
    Rect<Vec> RectBoundary { get; }

    public IBounding<Vec> Bounding { get; }
    public static IDomain<Vec> Infinite => new InfiniteDomain();

    

    private struct InfiniteDomain : IDomain<Vec>
    {
        public bool IsWithinPhase(Vec p) => true;
        public bool IsWithinSpace<T>(T p) where T : IVec<T, double> => true;
        public Rect<Vec> RectBoundary => throw new Exception();
        public IBounding<Vec> Bounding => BoundingFunctions.None<Vec>();
    }
}