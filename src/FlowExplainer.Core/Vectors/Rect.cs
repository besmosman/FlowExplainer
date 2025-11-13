using MemoryPack;

namespace FlowExplainer;

[MemoryPackable]
public partial struct Rect<Vec> where Vec : IVec<Vec>
{
    public Vec Min;
    public Vec Max;

    public Vec Size => Max - Min;
    public Vec Center => (Max + Min) / 2;

    public Rect(Vec min, Vec max)
    {
        Min = min;
        Max = max;
    }

    public static Rect<Vec> FromSize(Vec min, Vec size)
    {
        return new Rect<Vec>(min, min + size);
    }
    
    public bool Contains(Vec p)
    {
        return p > Min && p < Max;
    }

    public Vec Relative(Vec p)
    {
        return Min + Size * p;
    }

    public Rect<T> Reduce<T>() where T : IVec<T>, IVecUpDimension<Vec>
    {
        var min = T.Zero;
        var max = T.Zero;
        for (int i = 0; i < min.ElementCount; i++)
        {
            min[i] = Min[i];
            max[i] = Max[i];
        }
        return new Rect<T>(min, max);
    }
    public Vec Clamp(Vec p)
    {
        return Utils.Clamp<Vec, double>(p, Min, Max);
    }
}