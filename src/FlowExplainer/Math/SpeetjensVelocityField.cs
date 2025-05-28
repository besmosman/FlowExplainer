using System.Numerics;
using static System.Single;

namespace FlowExplainer;

public class SpeetjensVelocityField : IVectorField<Vector3, Vector2>
{
    public float Elipson = 0.0f;
    public Vector2 Evaluate(Vector3 x)
    {
        //var r= GetVelocity(x.X, x.Y,x.Z);
        //return new Vector2((float)r.ux, (float)r.uy);
        
        var t = x.Z;
        var pos = new Vector2(x.X, x.Y);

        var xPlus =  new Vector2(1 / 4f - DeltaX(t), 1 / 4f);
        var xMinus = new Vector2(3 / 4f - DeltaX(t), 1 / 4f);

        return UBar(pos - new Vector2(-DeltaX(t),0));
    }


    private float DeltaX(float t)
    {
        return Elipson * Sin(2 * Pi * t);
    }

    private static Vector2 UBar(Vector2 x) => UBar(x.X, x.Y);
    private static Vector2 UBar(float x, float y)
    {
        float ux = Sin(2 * Pi * x) * Cos(2 * Pi * y);
        float uy = -Cos(2 * Pi * x) * Sin(2 * Pi * y);
        return new Vector2(ux, uy);
    }
    
    /// <summary>
    /// Calculates the horizontal time-periodic oscillation Δx(t)
    /// </summary>
    private double DeltaX(double t)
    {
        return Epsilon * Math.Sin(2.0 * Math.PI * t);
    }
    
    /// <summary>
    /// Computes the x-component of velocity: ux = sin(2πx)cos(2πy)
    /// This creates the base solenoidal field
    /// </summary>
    public double GetVelocityX(double x, double y)
    {
        return Math.Sin(2.0 * Math.PI * x) * Math.Cos(2.0 * Math.PI * y);
    }
    
    /// <summary>
    /// Computes the y-component of velocity: uy = -cos(2πx)sin(2πy)
    /// This ensures divergence-free condition: ∂ux/∂x + ∂uy/∂y = 0
    /// </summary>
    public double GetVelocityY(double x, double y)
    {
        return -Math.Cos(2.0 * Math.PI * x) * Math.Sin(2.0 * Math.PI * y);
    }
    
    /// <summary>
    /// Computes the complete velocity vector at position (x,y) and current time
    /// Incorporates time-periodic vortex oscillation
    /// </summary>
    public (double ux, double uy) GetVelocity(double x, double y, float t)
    {
        // Apply time-dependent transformation for oscillating vortices
        double deltaX = DeltaX(t);
        
        // The vortices are centered at (1/4 - Δx(t), 1/4) and (3/4 - Δx(t), 1/4)
        // Transform coordinates to account for vortex movement
        double transformedX = x + deltaX;
        
        double ux = GetVelocityX(transformedX, y);
        double uy = GetVelocityY(transformedX, y);
        
        return (ux, uy);
    }
}