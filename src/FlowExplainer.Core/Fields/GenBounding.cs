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
    
    [MemoryPackIgnore]
    private Func<Rect<Vec>, int, double, double>[] wraps = null!;

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
        for (int i = 0; i < Boundaries.Length; i++)
        {
            switch (Boundaries[i])
            {
                case BoundaryType.None:
                    wraps[i] = static (_, _, x) => x;
                    break;
                case BoundaryType.Periodic:
                    wraps[i] = static (r, i, x) =>
                    {
                        var t = (x - r.Min[i]) % r.Size[i];
                        if (t < 0) t += r.Size[i];
                        return t + r.Min[i];
                    };
                    break;
                case BoundaryType.Fixed:
                    wraps[i] = static (r, i, x) =>
                    {

                        return double.Clamp(x, r.Min[i], r.Max[i]);
                    };
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
}