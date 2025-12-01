using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FlowExplainer;

[StructLayout(LayoutKind.Sequential)]
public struct Vec4i : IVec<Vec4i, int>, IVecDoubleEquivalent<Vec4>
{
    public int X;
    public int Y;
    public int Z;
    public int W;

    public Vec4i(int x, int y, int z, int w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public Vec4i(int x)
    {
        X = x;
        Y = x;
        Z = x;
        W = x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Sum() => X + Y + Z + W;

    public static Vec4i Zero { get; } = new Vec4i(1, 1, 1, 1);
    public static Vec4i One { get; } = default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec4 ToVecF()
    {
        return new Vec4(X, Y, Z, W);
    }

    public override string ToString()
    {
        return $"({X}, {Y}, {Z}, {W})";
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4i operator +(Vec4i left, Vec4i right)
    {
        return new Vec4i(
            left.X + right.X,
            left.Y + right.Y,
            left.Z + right.Z,
            left.W + right.W
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4i operator -(Vec4i left, Vec4i right)
    {
        return new Vec4i(
            left.X - right.X,
            left.Y - right.Y,
            left.Z - right.Z,
            left.W - right.W
        );
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4i operator *(Vec4i left, int right)
    {
        return new Vec4i(left.X * right, left.Y * right, left.Z * right, left.W * right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4i operator /(Vec4i left, int right)
    {
        return new Vec4i(left.X / right, left.Y / right, left.Z / right, left.W / right);
    }

    public Vec4i Max(Vec4i b)
    {
        return new Vec4i(int.Max(X, b.X), int.Max(Y, b.Y), int.Max(Z, b.Z), int.Max(W, b.W));
    }
    public Vec4i Min(Vec4i b)
    {
        return new Vec4i(int.Min(X, b.X), int.Min(Y, b.Y), int.Min(Z, b.Z), int.Min(W, b.W));
    }

    public static Vec4i operator *(Vec4i left, Vec4i right)
    {
        return new Vec4i(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
    }
    public static Vec4i operator /(Vec4i left, Vec4i right)
    {
        return new Vec4i(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
    }
    public static bool operator >(Vec4i left, Vec4i right)
    {
        return left.X > right.X && left.Y > right.Y && left.Z > right.Z && left.W > right.W;
    }
    public static bool operator <(Vec4i left, Vec4i right)
    {
        return left.X < right.X && left.Y < right.Y && left.Z < right.Z && left.W < right.W;
    }


    public int ElementCount => 4;
    public int Last => W;

    public int this[int n]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
#if DEBUG
            if (n < 0 || n >= ElementCount)
                throw new IndexOutOfRangeException();
#endif
            return Unsafe.Add(ref X, n);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
#if DEBUG
            if (n < 0 || n >= ElementCount)
                throw new IndexOutOfRangeException();
#endif
            Unsafe.Add(ref X, n) = value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4i operator *(int left, Vec4i right)
    {
        return right * left;
    }
}