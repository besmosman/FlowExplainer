
using System.Numerics;

namespace FlowExplainer;

[Serializable]
public struct Color : IMultiplyOperators<Color, float, Color>,
    IAdditionOperators<Color, Color, Color>
{
    public Vec3 RGB => new Vec3(R, G, B);
    public float R { get; set; }
    public float G { get; set; }
    public float B { get; set; }
    public float A { get; set; }

    public Color(float r, float g, float b, float a = 1f)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public Color(Vec4 v)
    {
        R = v.X;
        G = v.Y;
        B = v.Z;
        A = v.W;
    }

    public static Color White => new Color(1, 1, 1);
    public static Color Black => new Color(0, 0, 0);

    public static Color operator *(Color left, float right)
    {
        return new Color(left.R * right, left.G * right, left.B * right, left.A * right);
    }

    public static Color operator /(Color left, float right)
    {
        return new Color(left.R / right, left.G / right, left.B / right, left.A / right);
    }

    public static Color operator *(float left, Color right)
    {
        return new Color(right.R * left, right.G * left, right.B * left, right.A * left);
    }

    public static Color operator /(float left, Color right)
    {
        return new Color(right.R / left, right.G / left, right.B / left, right.A / left);
    }

    //
    public static Color operator *(Color left, Color right)
    {
        return new Color(left.R * right.R, left.G * right.G, left.B * right.B, left.A * right.A);
    }

    public static Color operator /(Color left, Color right)
    {
        return new Color(left.R / right.R, left.G / right.G, left.B / right.B, left.A / right.A);
    }

    public static Color operator +(Color left, Color right)
    {
        return new Color(left.R + right.R, left.G + right.G, left.B + right.B, left.A + right.A);
    }

    public static Color operator -(Color left, Color right)
    {
        return new Color(left.R - right.R, left.G - right.G, left.B - right.B, left.A - right.A);
    }
    
    public static Color FromHexString(string hex)
    {
        float r = GetFloat(hex, 0);
        float g = GetFloat(hex, 2);
        float b = GetFloat(hex, 4);
        int a = 1;

        return new Color(r, g, b, a);

        static float GetFloat(string hex, int i)
        {
            return Convert.ToInt32(hex.Substring(i, 2), 16) / 255f;
        }
    }

    public Vec4 ToVec4()
    {
        return new Vec4(R, G, B, A);
    }
}