using System.Collections.Concurrent;
using static System.Single;

namespace FlowExplainer;

public class PeriodicDiscritizedField : IPeriodicVectorField<Vec3, Vec2>
{
    public float Period => Original.Period;
    public IPeriodicVectorField<Vec3, Vec2> Original;
    public Vec3 CellSize;

    private ConcurrentDictionary<Vec3i, Vec2> samples = new();

    public PeriodicDiscritizedField(IPeriodicVectorField<Vec3, Vec2> original, Vec3 cellSize)
    {
        Original = original;
        CellSize = cellSize;
    }

    public Vec2 Evaluate(Vec3 x)
    {
        var c = (x / CellSize).FloorInt();

        var rel = (x - c.ToVec3() * CellSize) / CellSize;

        var curT = LinearInterpolateXY(c, rel.XY);
        var nextT = LinearInterpolateXY(c + new Vec3i(0, 0, 1), rel.XY);
        var value = Utils.Lerp(curT, nextT, rel.Z); 
        return value;
    }

    Vec2 LinearInterpolateXY(Vec3i ltCoord, Vec2 rel)
    {
        var c = ltCoord;
        var lb = GetAt(c);
        var rb = GetAt(c + new Vec3i(1, 0, 0));
        var lt = GetAt(c + new Vec3i(0, 1, 0));
        var rt = GetAt(c + new Vec3i(1, 1, 0));
        return Utils.Lerp(Utils.Lerp(lb, rb, rel.X), Utils.Lerp(lt, rt, rel.X), rel.Y);
    }

    private Vec2 GetAt(Vec3i cell)
    {
        return samples.GetOrAdd(cell, GetOriginalAtCell);
    }

    private Vec2 GetOriginalAtCell(Vec3i k)
    {
        return Original.Evaluate(k.ToVec3() * CellSize);
    }
}

//https://shaddenlab.berkeley.edu/uploads/LCS-tutorial/examples.html#x1-1200812
public class AnalyticalEvolvingVelocityField : IPeriodicVectorField<Vec3, Vec2>
{
    public float elipson = 0.0f;
    public float A = 1f;
    public float w = 1f;

    public float Period => (2f * Pi) / w;

    float streamFunction(float x, float y, float t)
    {
        return A * Sin(Pi * f(x, t)) * Sin(Pi * y);
    }

    float a(float t)
    {
        return elipson * Sin(w * t);
    }

    float b(float t)
    {
        return 1f - 2 * elipson * Sin(w * t);
    }

    public float f(float x, float t)
    {
        return a(t) * x * x + b(t) * x;
    }

    public Vec2 Evaluate(Vec3 x)
    {
        return Velocity(x.X, x.Y, x.Z);
    }

    public Vec2 Velocity(float x, float y, float t)
    {
        var u = -Pi * A * Sin(Pi * f(x, t)) * Cos(Pi * y);
        var dfdx = 2 * a(t) * x + b(t);
        var v = Pi * A * Cos(Pi * f(x, t)) * Sin(Pi * y) * dfdx;
        return -new Vec2(u, v);
    }
}