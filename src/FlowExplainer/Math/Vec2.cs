using System.Numerics;
using System.Runtime.CompilerServices;

namespace FlowExplainer;

public interface IVec<TVec> : IVec<TVec, float>
    where TVec : IVec<TVec, float>
{
}

public interface IVecIntegerEquivelant<TVeci> where TVeci : IVec<TVeci, int>
{
    TVeci Floor();
    TVeci Round();
}

public interface IVec<TVec, TNumber> :
    IMultiplyOperators<TVec, TNumber, TVec>,
    ISubtractionOperators<TVec, TVec, TVec>,
    IDivisionOperators<TVec, TNumber, TVec>,
    IAdditionOperators<TVec, TVec, TVec>
    where TVec : IVec<TVec, TNumber>
    where TNumber : INumber<TNumber>
{
    public TVec Max(TVec b);
    public int ElementCount { get; }

    public TNumber Sum();
    public abstract static TVec Zero { get; }
    public abstract static TVec One { get; }
    public TNumber Last => this[ElementCount - 1];
    public TNumber this[int n] { get; set; }

    static abstract TVec operator *(TNumber left, TVec right);
    static abstract TVec operator *(TVec left, TVec right);

    public TNumber Volume()
    {
        TNumber n = TNumber.One;

        for (int i = 0; i < ElementCount; i++)
            n *= this[i];

        return n;
    }
}

public struct Vec2 : IVec<Vec2>, IVecUpDimension<Vec3>, IVecDownDimension<Vec1>, IVecIntegerEquivelant<Vec2i>
{
    public float X;
    public float Y;
    public int ElementCount => 2;


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
    public float Sum() => X + Y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2 operator *(Vec2 v1, float f) => new(v1.X * f, v1.Y * f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2 operator /(Vec2 v1, float f) => new(v1.X / f, v1.Y / f);

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


    public static Func<Vec2, Vec2, bool> ApproximateComparer => (a, b) => { return (a - b).LengthSquared() < 1e-6f; };

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

    public Vec2i Floor()
    {
        return new Vec2i((int)float.Floor(X), (int)float.Floor(Y));
    }

    public Vec2i Round()
    {
        return new Vec2i((int)float.Round(X), (int)float.Round(Y));
    }
}