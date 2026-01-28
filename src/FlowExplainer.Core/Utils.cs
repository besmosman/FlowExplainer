using System.Numerics;
using System.Runtime.CompilerServices;

namespace FlowExplainer;

public static class Utils
{
    public static T Lerp<T, TC>(T a, T b, TC c) where T : IMultiplyOperators<T, TC, T>, IAdditionOperators<T, T, T>
        where TC : INumber<TC>
    {
        return a * (TC.One - c) + b * c;
    }

    public static T Max<T>(T a, T b) where T : IVec<T, double>
    {
        return a.Max(b);
    }
    
    /*
    Vector2 Rotate(Vec v, float angleRadians)
    {
        float cos = MathF.Cos(angleRadians);
        float sin = MathF.Sin(angleRadians);
        return new Vector2(
            v.X * cos - v.Y * sin,
            v.X * sin + v.Y * cos
        );
    }
    */

    public static T Min<T>(T a, T b) where T : IVec<T, double>
    {
        return a.Min(b);
    }

    public static T Clamp<T, TN>(T a, T min, T max) where T : IVec<T, TN> where TN : INumber<TN>
    {
        for (int i = 0; i < a.ElementCount; i++)
            a[i] = TN.Clamp(a[i], min[i], max[i]);
        return a;
    }

    extension<TVec, TNumber>(TVec) where TVec : IVec<TVec, TNumber> where TNumber : INumber<TNumber>
    {
        public static IVec<TVec, TNumber> Clamp(IVec<TVec, TNumber> a, IVec<TVec, TNumber> min, IVec<TVec, TNumber> max)
        {
            for (int i = 0; i < a.ElementCount; i++)
                a[i] = TNumber.Clamp(a[i], min[i], max[i]);
            return a;
        }
    }

    public static TNumber DistanceSquaredGeneric<TVec, TNumber>(TVec left, TVec right) where TVec : IVec<TVec, TNumber> where TNumber : INumber<TNumber>
    {
        var c = right - left;
        c *= c;
        return c.Sum();
        /*TNumber r = TNumber.Zero;
        for (int i = 0; i < left.ElementCount; i++)
            r += c[i] * c[i];
        return r;*/
    }
    
    extension<TVec>(TVec a) where TVec : IVec<TVec, double>
    {

        public static double DistanceSquared(TVec left, TVec right) => DistanceSquaredGeneric<TVec, double>(left, right);
        public double DistanceSquared1(TVec right) => DistanceSquaredGeneric<TVec, double>(a, right);

        /*public TNumber Volume()
        {
            TNumber n = TNumber.One;

            for (int i = 0; i < cur.ElementCount; i++)
                n *= cur[i];

            return n;
        }*/
    }

    /*extension<TVec>(TVec cur) where TVec : IVec<TVec>
    {
        public static double DistanceSquared(TVec left, TVec right)
        {
            var r = 0.0;
            for (int i = 0; i < left.ElementCount; i++)
            {
                var d = right[i] - left[i];
                r += d * d;
            }
            return r;
        }

        public double Volume()
        {
            double n = 0.0;

            for (int i = 0; i < cur.ElementCount; i++)
                n *= cur[i];

            return n;
        }
    }*/

    public static IVec<T, TN> Filled<T, TN>(TN value) where T : IVec<T, TN> where TN : INumber<TN>
    {

        IVec<T, TN> v = default;
        for (int i = 0; i < v.ElementCount; i++)
            v[i] = value;
        return v;
    }

    public static Rect<Vec2> GetBounds(IEnumerable<Vec2> filled)
    {
        var min = new Vec2(double.PositiveInfinity);
        var max = new Vec2(double.NegativeInfinity);
        foreach (var f in filled)
        {
            min = Min(min, f);
            max = Max(max, f);
        }
        return new Rect<Vec2>(min, max);
    }

    public static Double Random(double min, double max)
        => System.Random.Shared.NextDouble() * (max - min) + min;

    public static Vec Random<Vec>(Rect<Vec> bounds) where Vec : IVec<Vec, double>
    {
        var r = Vec.Zero;

        for (int i = 0; i < r.ElementCount; i++)
            r[i] = System.Random.Shared.NextSingle();

        return bounds.Min + bounds.Size * r;
    }
}