using static System.Single;

namespace FlowExplainer;

public class PoincareSectionsComputer
{
    public class Tracer
    {
        public Vec2 StartPosition;
        public Vec2 CurPosition;
        public List<Vec3> Trajectory = new();
    }

    public List<Tracer> tracers = new();

    public void Compute(IVectorField<Vec3, Vec2> vectorField, IIntegrator<Vec3, Vec2> integrator, Vec3 minPhase, Vec3 maxPhase)
    {
        int ts = 400;
        for (int i = 0; i < ts; i++)
        {
            var pos = new Vec2(i / (float)ts * (minPhase.X + maxPhase.X), (minPhase.Y + maxPhase.Y) / 2);
            tracers.Add(new Tracer
            {
                StartPosition = pos,
                CurPosition = pos,
                Trajectory = [pos.Up(0)],
            });
        }

        float simTime = maxPhase.Z - minPhase.Z;
        int steps = 1000;
        var dt = simTime / steps;
        float time = minPhase.Z;
        for (int i = 0; i < steps; i++)
        {
            foreach (var t in tracers)
            {
                int subSteps = 32;
                var ddt = dt / subSteps;
                for (int j = 0; j < subSteps; j++)
                {
                    t.CurPosition = integrator.Integrate(vectorField.Evaluate, t.CurPosition.Up(time), ddt);
                }

                t.Trajectory.Add(t.CurPosition.Up(time));
            }

            time += dt;
        }
    }
}

public class SpeetjensAdaptedVelocityField : IVectorField<Vec3, Vec2>
{
    public float elipson = 0.0f;

    public Vec2 ubar(Vec2 p) => ubar(p.X, p.Y);

    public Vec2 ubar(float x, float y)
    {
        var ux = Sin(2 * Pi * x) * Cos(2 * Pi * y);
        var uy = -Cos(2 * Pi * x) * Sin(2 * Pi * y);
        return new Vec2(ux, uy);
    }

    public float DeltaX(float t)
    {
        return elipson * Sin(2 * Pi * t);
    }

    public Vec2 Evaluate(Vec3 p)
    {
        var x = p.X;
        var y = p.Y;
        var t = p.Z;

        var x_plus = new Vec2(x, y) + new Vec2(1 / 4f - DeltaX(t), 1 / 4f);
        var x_minus = new Vec2(x, y) + new Vec2(3 / 4f - DeltaX(t), 1 / 4f);

        return ubar(new Vec2(x + DeltaX(t), y));
    }
}

public class SpeetjensVelocityField : IVectorField<Vec3, Vec2>
{
    public float Elipson = 0.5f;

    public Vec2 Evaluate(Vec3 x)
    {
        //var r= GetVelocity(x.X, x.Y,x.Z);
        //return new Vec2((float)r.ux, (float)r.uy);

        var t = x.Z;
        var pos = new Vec2(x.X, x.Y);

        var xPlus = new Vec2(1 / 4f - DeltaX(t), 1 / 4f);
        var xMinus = new Vec2(3 / 4f - DeltaX(t), 1 / 4f);

        return UBar(pos - new Vec2(-DeltaX(t), 0));
    }


    private float DeltaX(float t)
    {
        return Elipson * Sin(2 * Pi * t);
    }

    private static Vec2 UBar(Vec2 x) => UBar(x.X, x.Y);

    private static Vec2 UBar(float x, float y)
    {
        float ux = Sin(2 * Pi * x) * Cos(2 * Pi * y);
        float uy = -Cos(2 * Pi * x) * Sin(2 * Pi * y);
        return new Vec2(ux, uy);
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