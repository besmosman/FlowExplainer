using System.Numerics;

namespace FlowExplainer;

public static class Utils
{
    public static T Lerp<T, TC>(T a, T b, TC c) where T : IMultiplyOperators<T, TC, T>, IAdditionOperators<T, T, T>
        where TC : INumber<TC>
    {
        return a * (TC.One - c) + b * c;
    }

    public static T Max<T>(T a, T b) where T : IVec<T>
    {
        return a.Max(b);
    }
    
    public static T Min<T>(T a, T b) where T : IVec<T>
    {
        return a.Min(b);
    }
    
    public static T Clamp<T, TN>(T a, T min, T max) where T : IVec<T, TN> where TN : INumber<TN>
    {
        for (int i = 0; i < a.ElementCount; i++)
            a[i] = TN.Clamp(a[i], min[i], max[i]);
        return a;
    }

    public static IVec<T, TN> Filled<T, TN>(TN value) where T : IVec<T, TN> where TN : INumber<TN>
    {
        IVec<T, TN> v = default;
        for (int i = 0; i < v.ElementCount; i++)
            v[i] = value;
        return v;
    }
    
    public static Rect<Vec2> GetBounds(IEnumerable<Vec2> filled)
    {
        var min = new Vec2(float.PositiveInfinity);
        var max = new Vec2(float.NegativeInfinity);
        foreach (var f in filled)
        {
            min = Min(min, f);
            max = Max(max, f);
        }
        return new Rect<Vec2>(min, max);
    }

    public static Vec Random<Vec>(Rect<Vec> bounds) where Vec : IVec<Vec>
    {
        var r = Vec.Zero;
        
        for (int i = 0; i < r.ElementCount; i++) 
            r[i] = System.Random.Shared.NextSingle();

        return bounds.Min + bounds.Size * r;
    }
}