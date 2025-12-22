using MemoryPack;

namespace FlowExplainer;

[MemoryPackable]
public partial class GenBounding<Vec> : IBounding<Vec> where Vec : IVec<Vec, double>
{
    [MemoryPackConstructor]
    public GenBounding()
    {

    }

    public BoundaryType[] Boundaries { get; set; }
    public Rect<Vec> Rect { get; set; }

    [MemoryPackIgnore] private Func<Rect<Vec>, int, double, double>[] wraps = null!;

    [MemoryPackIgnore] private Func<Rect<Vec>, int, Vec, Vec, double>[] distances = null!;

    public static GenBounding<Vec> None()
    {
        var b = new BoundaryType[Vec.Zero.ElementCount];
        Array.Fill(b, BoundaryType.None);
        return new GenBounding<Vec>(b, new Rect<Vec>());
    }

    public GenBounding(BoundaryType[] boundaries, Rect<Vec> rect)
    {
        Boundaries = boundaries;
        Rect = rect;
        RebuildBoundMethod();
    }


    [MemoryPackOnDeserialized]
    private void RebuildBoundMethod()
    {
        wraps = new Func<Rect<Vec>, int, double, double>[Vec.One.ElementCount];
        distances = new Func<Rect<Vec>, int, Vec, Vec, double>[Vec.One.ElementCount];
        for (int i = 0; i < Boundaries.Length; i++)
        {
            switch (Boundaries[i])
            {
                case BoundaryType.None:
                    wraps[i] = static (_, _, x) => x;
                    distances[i] = static (_, i, x, y) => x[i] - y[i];
                    break;
                case BoundaryType.Periodic:
                    wraps[i] = static (r, i, x) =>
                    {
                        var t = (x - r.Min[i]) % r.Size[i];
                        if (t < 0) t += r.Size[i];
                        return t + r.Min[i];
                    };
                    distances[i] = static (r, i, x, y) =>
                    {
                        double L = r.Size[i];
                        double d = (x[i] - y[i]) % L;  
                        if (d > 0.5 * L) d -= L;
                        if (d < -0.5 * L) d += L;
                        return d;
                    };
                    break;
                case BoundaryType.Fixed:
                    wraps[i] = static (r, i, x) => { return double.Clamp(x, r.Min[i], r.Max[i]); };
                    distances[i] = static (_, i, x, y) => x[i] - y[i];
                    break;
                case BoundaryType.ReflectiveNeumann:
                    wraps[i] = static (r, i, x) =>
                    {
                        if (x < r.Min[i])
                            return r.Min[i] - x;
                        if (x > r.Max[i])
                            return r.Max[i] - x + r.Max[i];
                        return x;
                    };

                    break;
                default:
                    throw new NotImplementedException();
            }
        }


    }

    public Vec Bound(Vec x)
    {
        for (int i = 0; i < x.ElementCount; i++)
            x[i] = wraps[i](Rect, i, x[i]);
        return x;
    }
    public double ShortestSpatialDistanceSqrt(Vec a, Vec b)
    {
        Vec delta = Vec.Zero;
        for (int i = 0; i < a.ElementCount; i++)
            delta[i] = distances[i](Rect, i, a, b);
        return (delta*delta).Sum();
    }
}