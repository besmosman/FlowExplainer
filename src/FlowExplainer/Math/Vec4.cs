using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FlowExplainer;

public struct Rect
{
    public Vec2 Min;
    public Vec2 Max;

    public Vec2 Size => Max - Min;
    public Vec2 Center => (Max + Min) / 2;

    public Rect(Vec2 min, Vec2 max)
    {
        Min = min;
        Max = max;
    }
}

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct Vec4 : IVec<Vec4>, IVecDownDimension<Vec3>, IVecIntegerEquivelant<Vec4i>
{
    public float X;
    public float Y;
    public float Z;
    public float W;


    public Vec4(float x, float y, float z, float w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public Vec4(float x)
    {
        X = x;
        Y = x;
        Z = x;
        W = x;
    }

    public Vec4(Vec3 p, float w)
    {
        X = p.X;
        Y = p.Y;
        Z = p.Z;
        W = w;
    }

    public static Vec4 One { get; } = new Vec4(1);
    public static Vec4 Zero => default;

    public static Vec4 operator -(Vec4 v1, Vec4 v2) => new(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z, v1.W - v2.W);

    public static implicit operator Vector4(Vec4 v) => new(v.X, v.Y, v.Z, v.W);
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
    public static Vec4 operator *(Vec4 left, float right)
    {
        return new Vec4(left.X * right, left.Y * right, left.Z * right, left.W * right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec4 operator /(Vec4 left, float right)
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
        return new Vec4(float.Max(X, b.X), float.Max(Y, b.Y), float.Max(Z, b.Z), float.Max(W, b.W));
    }

    public int ElementCount => 4;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Sum() => X + Y + Z + W;

    public float this[int n]
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
    public static Vec4 operator *(float left, Vec4 right)
    {
        return right * left;
    }

    public static Vec4 operator *(Vec4 left, Vec4 right)
    {
        return new Vec4(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
    }

    public Vec3 Down()
    {
        return new Vec3(X, Y, Z);
    }

    public Vec4i Floor()
    {
        return new Vec4i((int)Math.Floor(X), (int)Math.Floor(Y), (int)Math.Floor(Z), (int)Math.Floor(W));
    }

    public Vec4i Round()
    {
        return new Vec4i((int)Math.Round(X), (int)Math.Round(Y), (int)Math.Round(Z), (int)Math.Round(W));
    }
}