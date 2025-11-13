using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FlowExplainer;

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct Vec4 : IVec<Vec4>, IVecDownDimension<Vec3>, IVecIntegerEquivalent<Vec4i>, IEquatable<Vec4>
{
    public double X;
    public double Y;
    public double Z;
    public double W;


    public Vec4(double x, double y, double z, double w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public Vec4(double x)
    {
        X = x;
        Y = x;
        Z = x;
        W = x;
    }

    public Vec4(Vec3 p, double w)
    {
        X = p.X;
        Y = p.Y;
        Z = p.Z;
        W = w;
    }

    public static Vec4 One { get; } = new Vec4(1);
    public static Vec4 Zero => default;

    public static Vec4 operator -(Vec4 v1, Vec4 v2) => new(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z, v1.W - v2.W);

    public static implicit operator Vector4(Vec4 v) => new((float)v.X, (float)v.Y, (float)v.Z, (float)v.W);
    public static explicit operator Vec4(Vector4 v) => new(v.X, v.Y, v.Z, v.W);

    public static Vec4 Transform(Vec3 p, Matrix4x4 view)
    {
        return (Vec4)Vector4.Transform(p, view);
    }

    public static Vec4 Transform(Vec2 p, Matrix4x4 view)
    {
        return (Vec4)Vector4.Transform(p, view);
    }

    public static Vec4 Transform(Vec4 up, Matrix4x4 projection)
    {
        return (Vec4)Vector4.Transform(up, projection);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4 operator *(Vec4 left, double right)
    {
        return new Vec4(left.X * right, left.Y * right, left.Z * right, left.W * right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4 operator /(Vec4 left, double right)
    {
        return new Vec4(left.X / right, left.Y / right, left.Z / right, left.W / right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4 operator +(Vec4 left, Vec4 right)
    {
        return new Vec4(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec4 Max(Vec4 b)
    {
        return new Vec4(double.Max(X, b.X), double.Max(Y, b.Y), double.Max(Z, b.Z), double.Max(W, b.W));
    }
    public Vec4 Min(Vec4 b)
    {
        return new Vec4(double.Min(X, b.X), double.Min(Y, b.Y), double.Min(Z, b.Z), double.Min(W, b.W));
    }

    public int ElementCount => 4;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Sum() => X + Y + Z + W;

    public double this[int n]
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
    public static Vec4 operator *(double left, Vec4 right)
    {
        return right * left;
    }

    public static Vec4 operator *(Vec4 left, Vec4 right)
    {
        return new Vec4(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
    }

    public static bool operator >(Vec4 left, Vec4 right)
    {
        return left.X > right.X && left.Y > right.Y && left.Z > right.Z && left.W > right.W;
    }
    public static bool operator <(Vec4 left, Vec4 right)
    {
        return left.X < right.X && left.Y < right.Y && left.Z < right.Z && left.W < right.W;
    }

    public Vec3 Down()
    {
        return new Vec3(X, Y, Z);
    }

    public Vec4i FloorInt()
    {
        return new Vec4i((int)Math.Floor(X), (int)Math.Floor(Y), (int)Math.Floor(Z), (int)Math.Floor(W));
    }

    public Vec4i RoundInt()
    {
        return new Vec4i((int)Math.Round(X), (int)Math.Round(Y), (int)Math.Round(Z), (int)Math.Round(W));
    }
    public bool Equals(Vec4 other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && W.Equals(other.W);
    }
    public override bool Equals(object? obj)
    {
        return obj is Vec4 other && Equals(other);
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z, W);
    }

    public static bool operator ==(Vec4 left, Vec4 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vec4 left, Vec4 right)
    {
        return !(left == right);
    }
}