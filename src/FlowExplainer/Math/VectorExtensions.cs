using System.Numerics;

namespace FlowExplainer;

public static class VectorExtensions
{
    public static Vector2 ToNumerics(this OpenTK.Mathematics.Vector2i v)
    {
        return new Vector2(v.X, v.Y);
    }

    //source: chat gpt :<
    public static Vector3 RotatePointAroundPivot(this Vector3 point, Vector3 pivot, Vector3 direction)
    {
        Vector3 relativePos = point - pivot; // Get the vector from pivot to point
        Quaternion rotation = Quaternion.CreateFromAxisAngle(direction, direction.Length()); // Create a rotation quaternion from the direction vector
        Vector3 rotatedPos = Vector3.Transform(relativePos, rotation); // Rotate the relative position vector
        return rotatedPos + pivot; // Add the pivot back to get the rotated point
    }


    public static Vector3 Floor(this Vector3 v)
    {
        return new Vector3(MathF.Floor(v.X), MathF.Floor(v.Y), MathF.Floor(v.Z));
    }
        
        
    public static Vector3 Ceil(this Vector3 v)
    {
        return new Vector3(MathF.Ceiling(v.X), MathF.Ceiling(v.Y), MathF.Ceiling(v.Z));
    }

    public static Vector3 Abs(this Vector3 v)
    {
        return new Vector3(MathF.Abs(v.X), MathF.Abs(v.Y), MathF.Abs(v.Z));
    }
        
    public static Vector3i CeilInt(this Vector3 v)
    {
        return new Vector3i((int)MathF.Ceiling(v.X), (int)MathF.Ceiling(v.Y), (int)MathF.Ceiling(v.Z));
    }

    public static Vector3i FloorInt(this Vector3 v)
    {
        return new Vector3i((int)MathF.Floor(v.X), (int)MathF.Floor(v.Y), (int)MathF.Floor(v.Z));
    }

    public static Vector3i RoundInt(this Vector3 v)
    {
        return new Vector3i((int)MathF.Round(v.X), (int)MathF.Round(v.Y), (int)MathF.Round(v.Z));
    }
    //source: Nurose which has as source someone else??? 
    public static Vector4 FromHSL(float h, float s, float l, float alpha = 1)
    {
        float p2;
        if (l <= 0.5f) p2 = l * (1 + s);
        else p2 = l + s - l * s;

        float p1 = 2 * l - p2;
        float double_r, double_g, double_b;
        if (s == 0)
        {
            double_r = l;
            double_g = l;
            double_b = l;
        }
        else
        {
            double_r = QqhToRgb(p1, p2, h + 120);
            double_g = QqhToRgb(p1, p2, h);
            double_b = QqhToRgb(p1, p2, h - 120);
        }

        return new Vector4(double_r, double_g, double_b, alpha);
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
        
        
    public static Vector4 FromHexString(string hex)
    {
        float r = GetFloat(hex, 0);
        float g = GetFloat(hex, 2);
        float b = GetFloat(hex, 4);
        int a = 1;

        return new Vector4(r, g, b, a);

        static float GetFloat(string hex, int i)
        {
            return Convert.ToInt32(hex.Substring(i, 2), 16) / 255f;
        }
    }

}