using System.Numerics;
using System.Runtime.CompilerServices;

namespace FlowExplainer;

[Serializable]
public struct Vec3 :
    IEquatable<Vec3>,
    IMultiplyOperators<Vec3, float, Vec3>,
    IAdditionOperators<Vec3, Vec3, Vec3>,
    IEqualityOperators<Vec3, Vec3, bool>,
    IAddDimension<Vec3, Vec4>
{
    public float X;
    public float Y;
    public float Z;

    public static int SizeInBytes { get; } = 12;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vec3(Vec2 v, float z = 0)
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
    public static Vec3 operator /(Vec3 v1, Vec3 v2) => new(v1.X / v2.X, v1.Y / v2.Y, v1.Z / v2.Z);

    public static Vec3 operator /(Vec3 v1, float f) => new(v1.X / f, v1.Y / f, v1.Z / f);
    public static Vec3 operator -(Vec3 v) => new(-v.X, -v.Y, -v.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec3 operator *(Vec3 v1, float f) => new(v1.X * f, v1.Y * f, v1.Z * f);

    public float Magnitude => (float)Math.Sqrt((X * X) + (Y * Y) + (Z * Z));
    public float SquaredMagnitude => (X * X) + (Y * Y);

    public Vec3 Normalized
    {
        get
        {
            float mag = Magnitude;
            if (mag == 0)
            {
                return default;
            }

            return this / Magnitude;
        }
    }

    public static Vec3 Zero => new();
    public static Vec3 UnitZ => new(0, 0, 1);
    public Vec2 XY => new Vec2(X, Y);

    public bool Equals(Vec3 other)
    {
        return this == other;
    }

    public Vec4 Up(float f)
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

    /*
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Vec3(Vec2 v)
    {
        return new Vec3(v.X, v.Y, 1);
    }
    */


    public static implicit operator Vector3(Vec3 v)
    {
        return new Vector3(v.X, v.Y, v.Z);
    }

    public static explicit operator Vec3(Vector3 v)
    {
        return new Vec3(v.X, v.Y, v.Z);
    }

    public readonly float LengthSquared() => (float)Math.Sqrt((X * X) + (Y * Y) + (Z * Z));

    public float Length()
    {
        return float.Sqrt(LengthSquared());
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
    
}