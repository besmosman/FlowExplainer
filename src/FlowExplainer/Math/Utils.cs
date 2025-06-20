using System.Numerics;

namespace FlowExplainer;

public class Utils
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
}