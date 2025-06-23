using System.Numerics;
using System.Runtime.CompilerServices;

namespace FlowExplainer;

public interface IVec<TVec> : IVec<TVec, float>
    where TVec : IVec<TVec, float>
{
}

public interface IVec<TVec, TNumber> :
    IMultiplyOperators<TVec, TNumber, TVec>,
    ISubtractionOperators<TVec, TVec, TVec>,
    IDivisionOperators<TVec, TNumber, TVec>,
    IAdditionOperators<TVec, TVec, TVec>
    where TVec : IVec<TVec, TNumber>
{
    public TVec Max(TVec b);
    public int Dimensions { get; }
    public TNumber Last { get; }
    static abstract TVec operator *(TNumber left, TVec right);

}

public struct Vec2 : IVec<Vec2>, IVecUpDimension<Vec3>, IVecDownDimension<Vec1>
{
    public float X;
    public float Y;
    public int Dimensions => 2;
    public float Last => Y;

    public Vec2(float x, float y)
    {
        X = x;
        Y = y;
    }

    public static Vec2 Zero => new Vec2();
    public static Vec2 One { get; } = new Vec2(1, 1);

    public Vec3 Up(float f)
    {
        return new Vec3(X, Y, f);
    }

    public Vec2 Down(Vec3 x) => x.XY;

    public Vec3 Construct(Vec2 x, float t) => new Vec3(x, t);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2 operator +(Vec2 v1, Vec2 v2) => new(v1.X + v2.X, v1.Y + v2.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2 operator -(Vec2 v1, Vec2 v2) => new(v1.X - v2.X, v1.Y - v2.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2 operator *(Vec2 v1, Vec2 v2) => new(v1.X * v2.X, v1.Y * v2.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2 operator /(Vec2 v1, Vec2 v2) => new(v1.X / v2.X, v1.Y / v2.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2 operator -(Vec2 v1) => new(-v1.X, -v1.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Dot(Vec2 v1, Vec2 v2)
    {
        return (v1.X * v2.X) + (v1.Y * v2.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2 operator *(Vec2 v1, float f) => new(v1.X * f, v1.Y * f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2 operator /(Vec2 v1, float f) => new(v1.X / f, v1.Y / f);

    public static Vec2 operator *(float f, Vec2 v1) => new(f * v1.X, f * v1.Y);

    public static Vec2 operator /(float f, Vec2 v1) => new(f / v1.X, f / v1.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistanceSquared(Vec2 a, Vec2 b)
    {
        var c = a - b;
        return (c.X * c.X) + (c.Y * c.Y);
    }

    public static float Distance(Vec2 a, Vec2 b) => float.Sqrt(DistanceSquared(a, b));

    public static Vec2 Normalize(Vec2 p)
    {
        return p / p.Length();
    }

    public float Length()
    {
        return MathF.Sqrt((X * X) + (Y * Y));
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float LengthSquared()
    {
        return (X * X) + (Y * Y);
    }

    public static implicit operator Vector2(Vec2 v) => new(v.X, v.Y);
    public static explicit operator Vec2(Vector2 v) => new(v.X, v.Y);

    public Vec2i RoundInt()
    {
        return new Vec2i((int)float.Round(X), (int)float.Round(Y));
    }

    public Vec2i CeilInt()
    {
        return new Vec2i((int)float.Ceiling(X), (int)float.Ceiling(Y));
    }


    public Vec2 Max(Vec2 b)
    {
        return new Vec2(float.Max(X, b.X), float.Max(Y, b.Y));
    }

    public Vec1 Down()
    {
        return new Vec1(X);
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }

    public Vec2 Abs()
    {
        return new Vec2(float.Abs(X), float.Abs(Y));
    }

    public Vector2 ToNumerics()
    {
        return new Vector2(X, Y);
    }
}