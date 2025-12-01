using System.Runtime.CompilerServices;

namespace FlowExplainer;

public struct Vec1 : IVec<Vec1>
{
    public double X;

    public Vec1(double x)
    {
        X = x;
    }


    public int ElementCount => 1;
    public static Vec1 One => 1;
    public static Vec1 Zero => 0;
    public double Last => X;

    public double this[int n]
    {
        get
        {
#if DEBUG
            if (n != 0)
                throw new Exception();
#endif
            return X;
        }
        set
        {
#if DEBUG
            if (n != 0)
                throw new Exception();
#endif
            X = value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Sum() => X;

    public static Vec1 operator *(Vec1 left, double right) => left.X * right;
    public static Vec1 operator -(Vec1 left, Vec1 right) => left.X - right.X;
    public static Vec1 operator /(Vec1 left, double right) => left.X / right;
    public static Vec1 operator +(Vec1 left, Vec1 right) => left.X + right.X;

    public static Vec1 operator *(double left, Vec1 right) => left * right.X;
    public static Vec1 operator *(Vec1 left, Vec1 right) => left.X * right.X;
    public static Vec1 operator /(Vec1 left, Vec1 right)
    {
        return left.X / right.X;
    }
    public static bool operator >(Vec1 left, Vec1 right)
    {
        return left.X > right.X;
    }
    public static bool operator <(Vec1 left, Vec1 right)
    {
        return left.X < right.X;
    }

    public Vec1 Max(Vec1 b) => double.Max(X, b.X);
    public Vec1 Min(Vec1 b) => double.Min(X, b.X);

    public static implicit operator double(Vec1 v) => v.X;
    public static implicit operator Vec1(double v) => new Vec1(v);
}