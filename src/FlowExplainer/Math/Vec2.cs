using System.Numerics;
using System.Runtime.CompilerServices;

namespace FlowExplainer;

public struct Vec2 : IAddDimension<Vec2, Vec3>
{
    public float X;
    public float Y;

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

    private float Length()
    {
        return MathF.Sqrt((X * X) + (Y * Y));
    }
    
    public static implicit operator Vector2(Vec2 v) => new(v.X, v.Y);
    public static explicit operator Vec2(Vector2 v) => new(v.X, v.Y);
}