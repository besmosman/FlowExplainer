using System.Numerics;
using System.Runtime.CompilerServices;

namespace FlowExplainer;

public interface IVec<TVec> : IVec<TVec, double>
    where TVec : IVec<TVec, double>
{
}

public interface IVecIntegerEquivalent<TVeci> where TVeci : IVec<TVeci, int>
{
    TVeci FloorInt();
    TVeci RoundInt();
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
    public TVec Min(TVec b);
    public int ElementCount { get; }

    public TNumber Sum();
    public abstract static TVec Zero { get; }
    public abstract static TVec One { get; }

    public TNumber Last => this[ElementCount - 1];
    public TNumber this[int n] { get; set; }

    static abstract TVec operator *(TNumber left, TVec right);
    static abstract TVec operator *(TVec left, TVec right);
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

public struct Vec2 : IVec<Vec2>, IVecUpDimension<Vec3>, IVecDownDimension<Vec1>, IVecIntegerEquivalent<Vec2i>
{
    public double X;
    public double Y;
    public int ElementCount => 2;


    public double Last => Y;

    public Vec2(double c)
    {
        X = c;
        Y = c;
    }
    
    public Vec2(double x, double y)
    {
        X = x;
        Y = y;
    }

    public static Vec2 Zero => new Vec2();
    public static Vec2 One { get; } = new Vec2(1, 1);

    public Vec3 Up(double f)
    {
        return new Vec3(X, Y, f);
    }

    public Vec2 Down(Vec3 x) => x.XY;

    public Vec3 Construct(Vec2 x, double t) => new Vec3(x, t);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2 operator +(Vec2 v1, Vec2 v2) => new(v1.X + v2.X, v1.Y + v2.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2 operator -(Vec2 v1, Vec2 v2) => new(v1.X - v2.X, v1.Y - v2.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2 operator *(Vec2 v1, Vec2 v2) => new(v1.X * v2.X, v1.Y * v2.Y);
    public static bool operator >(Vec2 left, Vec2 right)
    {
        return left.X > right.X && left.Y > right.Y;
    }
    public static bool operator <(Vec2 left, Vec2 right)
    {
        return left.X < right.X && left.Y < right.Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2 operator /(Vec2 v1, Vec2 v2) => new(v1.X / v2.X, v1.Y / v2.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2 operator -(Vec2 v1) => new(-v1.X, -v1.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Dot(Vec2 v1, Vec2 v2)
    {
        return (v1.X * v2.X) + (v1.Y * v2.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Sum() => X + Y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2 operator *(Vec2 v1, double f) => new(v1.X * f, v1.Y * f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2 operator /(Vec2 v1, double f) => new(v1.X / f, v1.Y / f);

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

    public static Vec2 operator *(double f, Vec2 v1) => new(f * v1.X, f * v1.Y);

    public static Vec2 operator /(double f, Vec2 v1) => new(f / v1.X, f / v1.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double DistanceSquared(Vec2 a, Vec2 b)
    {
        var c = a - b;
        return (c.X * c.X) + (c.Y * c.Y);
    }

    public static double Distance(Vec2 a, Vec2 b) => double.Sqrt(DistanceSquared(a, b));

    public static Vec2 Normalize(Vec2 p)
    {
        double length = p.Length();
        return p / length;
    }

    public double Length()
    {
        return Math.Sqrt((X * X) + (Y * Y));
    }


    public static Func<Vec2, Vec2, bool> ApproximateComparer => (a, b) => { return (a - b).LengthSquared() < 1e-6f; };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double LengthSquared()
    {
        return (X * X) + (Y * Y);
    }

    public static implicit operator Vector2(Vec2 v) => new((float)v.X, (float)v.Y);
    public static explicit operator Vec2(Vector2 v) => new(v.X, v.Y);

    public Vec2i RoundInt()
    {
        return new Vec2i((int)double.Round(X), (int)double.Round(Y));
    }

    public Vec2i CeilInt()
    {
        return new Vec2i((int)double.Ceiling(X), (int)double.Ceiling(Y));
    }

    public Vec2 Min(Vec2 b)
    {
        return new Vec2(double.Min(X, b.X), double.Min(Y, b.Y));
    }

    public Vec2 Max(Vec2 b)
    {
        return new Vec2(double.Max(X, b.X), double.Max(Y, b.Y));
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
        return new Vec2(double.Abs(X), double.Abs(Y));
    }

    public Vector2 ToNumerics()
    {
        return new Vector2((float)X, (float)Y);
    }

    public Vec2i FloorInt()
    {
        return new Vec2i((int)double.Floor(X), (int)double.Floor(Y));
    }
}