using System.Numerics;

namespace FlowExplainer;

[Serializable]
public struct Color : 
    IMultiplyOperators<Color, float, Color>,
    IMultiplyOperators<Color, double, Color>,
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

    public Color(double r, double g, double b, double a = 1f)
    {
        R =(float)r;
        G =(float)g;
        B =(float)b;
        A =(float)a;
    }
    
    public Color(Vec4 v)
    {
        R = (float)v.X;
        G = (float)v.Y;
        B = (float)v.Z;
        A = (float)v.W;
    }

    public static readonly Color White = new Color(1, 1, 1);
    public static Color Black => new Color(0, 0, 0);
    public static Color Grey(float f) => new Color(f, f, f);

    public static Color operator *(Color left, float right)
    {
        return new Color(left.R * right, left.G * right, left.B * right, left.A * right);
    }
    
    public static Color operator *(Color left, double right)
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
        float r = Getfloat(hex, 0);
        float g = Getfloat(hex, 2);
        float b = Getfloat(hex, 4);
        int a = 1;

        return new Color(r, g, b, a);

        static float Getfloat(string hex, int i)
        {
            return Convert.ToInt32(hex.Substring(i, 2), 16) / 255f;
        }
    }

    //source: Nurose which has as source someone else?? 
    public static Color FromHSL(float h, float s, float l, float alpha = 1)
    {
        float p2;
        if (l <= 0.5f) p2 = l * (1 + s);
        else p2 = l + s - l * s;

        float p1 = 2 * l - p2;
        float float_r, float_g, float_b;
        if (s == 0)
        {
            float_r = l;
            float_g = l;
            float_b = l;
        }
        else
        {
            float_r = QqhToRgb(p1, p2, h + 120);
            float_g = QqhToRgb(p1, p2, h);
            float_b = QqhToRgb(p1, p2, h - 120);
        }

        return new Color(float_r, float_g, float_b, alpha);
    }

    private static float QqhToRgb(float q1, float q2, float hue)
    {
        if (hue > 360) hue -= 360;
        else if (hue < 0) hue += 360;

        if (hue < 60) return q1 + (q2 - q1) * hue / 60;
        if (hue < 180) return q2;
        if (hue < 240) return q1 + (q2 - q1) * (240 - hue) / 60;
        return q1;
    }
    
    public Vec4 ToVec4()
    {
        return new Vec4(R, G, B, A);
    }
    
    public System.Numerics.Vector4 ToNumerics()
    {
        return new System.Numerics.Vector4((float)R, (float)G, (float)B, (float)A);
    }
    public string ToHex()
    {
        var c = this * 255f;
        return $"{(int)c.R:X2}{(int)c.G:X2}{(int)c.B:X2}";
    }

    public Color WithAlpha(double alpha)
    {
        return new Color(R, G, B, alpha);
    }
}