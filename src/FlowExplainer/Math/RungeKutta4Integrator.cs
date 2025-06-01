
namespace FlowExplainer;

public class RungeKutta4Integrator : IIntegrator<Vec3, Vec2>
{
    public static Vec2 Integrate(Func<float, Vec2, Vec2> f, Vec2 y, float t, float dt)
    {
        var k1 = f(t, y);
        var k2 = f(t + dt / 2, y + dt * (k1 / 2));
        var k3 = f(t + dt / 2, y + dt * (k2 / 2));
        var k4 = f(t + dt, y + dt * k3);
        return y + (dt / 6) * (k1 + 2 * k2 + 2 * k3 + k4);
        //return y + f(t, y) * dt;
    }

    public Vec2 Integrate(Func<Vec3, Vec2> f, Vec3 x, float dt)
    {
        Vec2 p = new Vec2(x.X, x.Y);
        float t = x.Z;
        var k1 = f(new(p, t));
        var k2 = f(new(p + dt * (k1 / 2), t + dt / 2));
        var k3 = f(new(p + dt * (k2 / 2), t + dt / 2));
        var k4 = f(new(p + dt * (k3 / 1), t + dt / 1));
        return p + (dt / 6) * (k1 + 2 * k2 + 2 * k3 + k4);
    }
}