using System.Numerics;
using System.Runtime.CompilerServices;

namespace FlowExplainer;

public interface IVecDoubleEquivalent<TVecF>
{
    TVecF ToVecF();
}

public struct Vec2i :
    IVec<Vec2i, int>,
    IEquatable<Vec2i>,
    IEqualityOperators<Vec2i, Vec2i, bool>,
    IVecDoubleEquivalent<Vec2>
{
    public int X;
    public int Y;

    public static Vec2i Zero { get; } = new(0, 0);
    public static Vec2i One { get; } = new(1, 1);
    public static readonly Vec2i Right = new(1, 0);
    public static readonly Vec2i Left = new(-1, 0);
    public static readonly Vec2i Up = new(0, 1);
    public static readonly Vec2i Down = new(0, -1);


    public Vec2i(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Vec2 ToVec2()
    {
        return new Vec2(X, Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Sum() => X + Y;

    public static bool operator ==(Vec2i left, Vec2i right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vec2i left, Vec2i right)
    {
        return !(left == right);
    }

    
    public static Vec2i operator +(Vec2i v1, Vec2i v2) => new(v1.X + v2.X, v1.Y + v2.Y);
    public static Vec2i operator -(Vec2i v1, Vec2i v2) => new(v1.X - v2.X, v1.Y - v2.Y);
    public static Vec2i operator *(Vec2i v1, Vec2i v2) => new(v1.X * v2.X, v1.Y * v2.Y);
    public static bool operator >(Vec2i left, Vec2i right)
    {
        return left.X > right.X && left.Y > right.Y;
    }
    public static bool operator <(Vec2i left, Vec2i right)
    {
        return left.X < right.X && left.Y < right.Y;
    }
    public static Vec2i operator /(Vec2i v1, Vec2i v2) => new(v1.X / v2.X, v1.Y / v2.Y);

    public static Vec2i operator /(Vec2i v1, int i) => new(v1.X / i, v1.Y / i);
    public static Vec2i operator *(Vec2i v1, int i) => new(v1.X * i, v1.Y * i);
    public static Vec2i operator -(Vec2i v1) => new(-v1.X, -v1.Y);


    public static implicit operator Vector2(Vec2i vec2I) => new(vec2I.X, vec2I.Y);

    public static implicit operator Vec2i((int, int) tuple) => new(tuple.Item1, tuple.Item2);

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public Vec2 ToVecF()
    {
        return new Vec2(X, Y);
    }
    
    public override bool Equals(object? obj)
    {
        return obj is Vec2i other && Equals(other);
    }

    public bool Equals(Vec2i other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y);
    }

    public bool IsBetweenRect(Vec2i rectBegin, Vec2i rectEnd)
    {
        return X > rectBegin.X && X < rectEnd.X && Y > rectBegin.Y && Y < rectEnd.Y;
    }


    public static int DistanceSquared(Vec2i a, Vec2i b)
    {
        var c = a - b;
        return (c.X * c.X) + (c.Y * c.Y);
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }

    public Vector2 ToNumerics()
    {
        return new Vector2(X, Y);
    }

    public static explicit operator Vec2i(Vector2 v)
    {
        return new Vec2i((int)v.X, (int)v.Y);
    }

    public Vec2i Max(Vec2i b)
    {
        return new Vec2i(int.Max(X, b.X), int.Max(Y, b.Y));
    }
    
    public Vec2i Min(Vec2i b)
    {
        return new Vec2i(int.Min(X, b.X), int.Min(Y, b.Y));
    }

    public int ElementCount => 2;
    public int Last
    {
        get => Y;
        set => Y = value;
    }


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


    public static Vec2i operator *(int left, Vec2i right)
    {
        return right * left;
    }
    public int Volume()
    {
        return X * Y;
    }
}