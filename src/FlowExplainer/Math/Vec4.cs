using System.Numerics;

namespace FlowExplainer;

[Serializable]
public struct Vec4
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

    public static Vec4 One => new Vec4(1);

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
}