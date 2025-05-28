using System.Numerics;

namespace FlowExplainer;

public class RungeKutta4Integrator : IIntegrator<Vector3, Vector2>
{
    public static Vector2 Integrate(Func<float, Vector2, Vector2> f, Vector2 y, float t, float dt)
    {
        var k1 = f(t, y);
        var k2 = f(t + dt / 2, y + dt * (k1 / 2));
        var k3 = f(t + dt / 2, y + dt * (k2 / 2));
        var k4 = f(t + dt, y + dt * k3);
        return y + (dt / 6) * (k1 + 2 * k2 + 2 * k3 + k4);
        //return y + f(t, y) * dt;
    }

    public Vector2 Integrate(Func<Vector3, Vector2> f, Vector3 x, float dt)
    {
        Vector2 p = new Vector2(x.X, x.Y);
        float t = x.Z;
        var k1 = f(new(p, t));
        var k2 = f(new(p + dt * (k1 / 2), t + dt / 2));
        var k3 = f(new(p + dt * (k2 / 2), t + dt / 2));
        var k4 = f(new(p + dt * (k3 / 1), t + dt / 1));
        return p + (dt / 6) * (k1 + 2 * k2 + 2 * k3 + k4);
    }
}