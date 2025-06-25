using System.Numerics;
using System.Runtime.CompilerServices;

namespace FlowExplainer;

[Serializable]
public struct Vec3 :
    IVec<Vec3>,
    IEquatable<Vec3>,
    IVecUpDimension<Vec4>,
    IVecDownDimension<Vec2>
{
    public float X;
    public float Y;
    public float Z;

    public static int SizeInBytes { get; } = 12;
    public int Dimensions => 3;
    
    public float Last => Z;
    
    public static Vec3 operator *(float left, Vec3 right)
    {
        return right * left;
    }

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
    public static Vec3 One => new Vec3(1, 1, 1);
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

    public Vec3 Max(Vec3 b)
    {
        return new Vec3(
            float.Max(X, b.X),
            float.Max(Y, b.Y),
            float.Max(Z, b.Z)
        );
    }

    public Vec2 Down() => XY;

    //source NeuroTrace ????
    public static Matrix4x4 LookAtDirection(Vector3 direction)
    {
        direction = Vector3.Normalize(direction);
        Vector3 forward = new Vector3(0, 0, 1);
        Vector3 rotationAxis = Vector3.Cross(forward, direction);
        float dotProduct = Vector3.Dot(forward, direction);
        float angle = MathF.Acos(dotProduct);
        
        if (rotationAxis.LengthSquared() > 0.0001f)
        {
            rotationAxis = Vector3.Normalize(rotationAxis);
            return Matrix4x4.CreateFromAxisAngle(rotationAxis, angle);
        }
        else
        {
            if (dotProduct < 0)
                return Matrix4x4.CreateRotationX(MathF.PI);
            else
                return Matrix4x4.Identity;
        }
    }
}