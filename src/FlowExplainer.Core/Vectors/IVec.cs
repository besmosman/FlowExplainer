using System.Numerics;

namespace FlowExplainer;

public interface IVec<TVec, TNumber> :
    IMultiplyOperators<TVec, TNumber, TVec>,
    ISubtractionOperators<TVec, TVec, TVec>,
    IDivisionOperators<TVec, TNumber, TVec>,
    IAdditionOperators<TVec, TVec, TVec>,
    IEqualityOperators<TVec, TVec, bool>,
    IEquatable<TVec>
    where TVec : IVec<TVec, TNumber>
    where TNumber : INumber<TNumber>
{
    public TVec Max(TVec b);
    public TVec Min(TVec b);

    public int ElementCount { get; }

    public TNumber Sum();
    public static abstract TNumber Dot(TVec a, TVec b);
    public static abstract TVec Zero { get; }
    public static abstract TVec One { get; }

    public TNumber Last { get; set; }
    public TNumber this[int n] { get; set; }


    static abstract TVec operator *(TNumber left, TVec right);
    static abstract TVec operator *(TVec left, TVec right);
    static abstract TVec operator /(TVec left, TVec right);
    static abstract bool operator >(TVec left, TVec right);
    static abstract bool operator <(TVec left, TVec right);


    public TNumber Volume()
    {
        TNumber n = TNumber.One;

        for (int i = 0; i < ElementCount; i++)
            n *= this[i];

        return n;
    }
}