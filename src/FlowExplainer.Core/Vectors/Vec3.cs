using System.Numerics;
using System.Runtime.CompilerServices;

namespace FlowExplainer;


[Serializable]
public struct Vec3 :
    IVec<Vec3>,
    IEquatable<Vec3>,
    IVecUpDimension<Vec4>,
    IVecDownDimension<Vec2>,
    IVecIntegerEquivalent<Vec3i>
{
    public double X;
    public double Y;
    public double Z;

    public static int SizeInBytes { get; } = 12;
    public int ElementCount => 3;
    public double Last => Z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Sum() => X + Y+Z;
    

    public static Vec3 operator *(double left, Vec3 right)
    {
        return right * left;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec3(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec3(Vec2 v, double z = 0)
    {
        X = v.X;
        Y = v.Y;
        Z = z;
    }

    

    public static bool operator ==(Vec3 v1, Vec3 v2)
    {
        return v1.X == v2.X && v1.Y == v2.Y;
    }

    public static bool operator !=(Vec3 v1, Vec3 v2)
    {
        return !(v1 == v2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3 operator -(Vec3 v1, Vec3 v2) => new(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3 operator +(Vec3 v1, Vec3 v2) => new(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3 operator *(Vec3 v1, Vec3 v2) => new(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
   
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Vec3 left, Vec3 right) => left.X > right.X && left.Y > right.Y && left.Z > right.Z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Vec3 left, Vec3 right) => left.X < right.X && left.Y < right.Y && left.Z < right.Z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3 operator /(Vec3 v1, Vec3 v2) => new(v1.X / v2.X, v1.Y / v2.Y, v1.Z / v2.Z);

    public static Vec3 operator /(Vec3 v1, double f) => new(v1.X / f, v1.Y / f, v1.Z / f);
    public static Vec3 operator -(Vec3 v) => new(-v.X, -v.Y, -v.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3 operator *(Vec3 v1, double f) => new(v1.X * f, v1.Y * f, v1.Z * f);

    public double Magnitude => (double)Math.Sqrt((X * X) + (Y * Y) + (Z * Z));
    public double SquaredMagnitude => (X * X) + (Y * Y);

    public Vec3 Normalized
    {
        get
        {
            double mag = Magnitude;
            if (mag == 0)
            {
                return default;
            }

            return this / Magnitude;
        }
    }

    public static Vec3 Zero => new();
    public static Vec3 One => new Vec3(1, 1, 1);
    public static Vec3 UnitZ => new(0, 0, 1);
    public Vec2 XY => new Vec2(X, Y);

    public bool Equals(Vec3 other)
    {
        return this == other;
    }

    public Vec4 Up(double f)
    {
        return new Vec4(this, f);
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return $"({X}, {Y}, {Z})";
    }

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
    
    /*
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Vec3(Vec2 v)
    {
        return new Vec3(v.X, v.Y, 1);
    }
    */


    public static implicit operator Vector3(Vec3 v)
    {
        return new Vector3((float)v.X, (float)v.Y, (float)v.Z);
    }

    public static explicit operator Vec3(Vector3 v)
    {
        return new Vec3(v.X, v.Y, v.Z);
    }

    public readonly double LengthSquared() => (double)Math.Sqrt((X * X) + (Y * Y) + (Z * Z));

    public double Length()
    {
        return double.Sqrt(LengthSquared());
    }

    public static Vec3 Normalize(Vec3 v)
    {
        return v / v.Length();
    }

    public static Vec3 Transform(Vec3 vec3, Quaternion rotation)
    {
        return (Vec3)Vector3.Transform(vec3, rotation);
    }

    public static Vec3 Transform(Vec3 vec3, Matrix4x4 m)
    {
        return (Vec3)Vector3.Transform(vec3, m);
    }

    public Vec3 Max(Vec3 b)
    {
        return new Vec3(
            double.Max(X, b.X),
            double.Max(Y, b.Y),
            double.Max(Z, b.Z)
        );
    }
    public Vec3 Min(Vec3 b)
    {
        return new Vec3(
            double.Min(X, b.X),
            double.Min(Y, b.Y),
            double.Min(Z, b.Z)
        );
    }

    public Vec2 Down() => XY;


    public double GetDimension(int n)
    {
        switch (n)
        {
            case 0: return X;
            case 1: return Y;
            case 2: return Z;
            default: throw new Exception();
        }
    }


    //source NeuroTrace ????
    public static Matrix4x4 LookAtDirection(Vector3 direction)
    {
        direction = Vector3.Normalize(direction);
        Vector3 forward = new Vector3(0, 0, 1);
        Vector3 rotationAxis = Vector3.Cross(forward, direction);
        var dotProduct = Vector3.Dot(forward, direction);
        var angle = Math.Acos(dotProduct);

        if (rotationAxis.LengthSquared() > 0.0001)
        {
            rotationAxis = Vector3.Normalize(rotationAxis);
            return Matrix4x4.CreateFromAxisAngle(rotationAxis, (float)angle);
        }
        else
        {
            if (dotProduct < 0)
                return Matrix4x4.CreateRotationX(MathF.PI);
            else
                return Matrix4x4.Identity;
        }
    }

    public Vec3i FloorInt()
    {
        return new Vec3i((int)Math.Floor(X), (int)Math.Floor(Y), (int)Math.Floor(Z));
    }

    public Vec3i RoundInt()
    {
        return new Vec3i((int)Math.Round(X), (int)Math.Round(Y), (int)Math.Round(Z));
    }
    public Vector3 ToNumerics()
    {
        return new Vector3((float)X, (float)Y, (float)Z);
    }
}