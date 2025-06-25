using ImGuiNET;
using static System.Single;

namespace FlowExplainer;

public class PhaseComputer
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

public class SpeetjensAdaptedVelocityField : IEditabalePeriodicVectorField<Vec3, Vec2>
{
    public float elipson = 1f;
    public float w = 2*Pi;

    public float Period => 1;
    public Rect Domain => new Rect(new Vec2(0, 0), new Vec2(1, .5f));

    public void OnImGuiEdit()
    {
        ImGuiHelpers.SliderFloat("Elipson", ref elipson, 0, 2);
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
        x *= 2;
        y *= 2;
        //var dx = x + elipson * Sin(2 * Pi * t);
        var u = Sin(Pi * f(x, t)) * -Cos(Pi * y);
        var dfdx = 2 * a(t) * x + b(t);
        var v = Cos(Pi * f(x,t)) * Sin(Pi * y) * dfdx;
        return -new Vec2(u, v);
    }
}

public class SpeetjensVelocityField : IEditabalePeriodicVectorField<Vec3, Vec2>
{
    public float Epsilon = 0.0f;

    public float Period => 1;
    public Rect Domain => new Rect(Vec2.Zero, new Vec2(1, .5f));

    public void OnImGuiEdit()
    {
        ImGuiHelpers.SliderFloat("Epsilon", ref Epsilon, 0, 1);
    }


    public Vec2 ubar(Vec2 x, float t)
    {
        var ux = Sin(2 * Pi * x.X) * Cos(2 * Pi * x.Y);
        var uy = -Cos(2 * Pi * x.X) * Sin(2 * Pi * x.Y);
        return new Vec2(ux, uy);
    }
    

    public float DeltaX(float t)
    {
        return Epsilon * Sin(2 * Pi * t);
    }

    public Vec2 Evaluate(Vec3 p)
    {
        var x = p.X;
        var y = p.Y;
        var t = p.Z;

        var x_plus = new Vec2(x, y) - new Vec2(1 / 4f - DeltaX(t), 1 / 4f);
        var x_minus = new Vec2(x, y) - new Vec2(3 / 4f - DeltaX(t), 1 / 4f);
        return ubar(x_plus, t) + ubar(x_minus, t);
    }
}