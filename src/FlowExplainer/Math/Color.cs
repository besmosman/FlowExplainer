using System.Numerics;

namespace FlowExplainer;

[Serializable]
public struct Color : IMultiplyOperators<Color, float, Color>,
    IAdditionOperators<Color, Color, Color>
{
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
}