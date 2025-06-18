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
        Vec2 p = x.XY;
        float t = x.Z;
        var k1 = f(new(p, t));
        var k2 = f(new(p + dt * (k1 / 2), t + dt / 2));
        var k3 = f(new(p + dt * (k2 / 2), t + dt / 2));
        var k4 = f(new(p + dt * (k3), t + dt));
        return p + (dt / 6) * (k1 + 2 * k2 + 2 * k3 + k4);
    }
}

public interface IIntegrator<TInput, TOutput>
    where TInput : IVec<TInput>, IVecDownDimension<TOutput>
    where TOutput : IVec<TOutput>, IVecUpDimension<TInput>
{
    TOutput Integrate(Func<TInput, TOutput> f, TInput x, float dt);

    public static IIntegrator<TInput, TOutput> Rk4 { get; } = new RungeKutta4IntegratorGen<TInput, TOutput>();
}

public class RungeKutta4IntegratorGen<TInput, TOutput> : IIntegrator<TInput, TOutput>
    where TInput : IVec<TInput>, IVecDownDimension<TOutput>
    where TOutput : IVec<TOutput>, IVecUpDimension<TInput>
{
    public static TOutput Integrate(Func<float, TOutput, TOutput> f, TOutput y, float t, float dt)
    {
        var k1 = f(t, y);
        var k2 = f(t + dt / 2, y + (k1 / 2f) * dt);
        var k3 = f(t + dt / 2, y + (k2 / 2f) * dt);
        var k4 = f(t + dt, y + k3 * dt);
        return y + (dt / 6) * (k1 + 2 * k2 + 2 * k3 + k4);
        //linear: return y + f(t, y) * dt;
    }

    public TOutput Integrate(Func<TInput, TOutput> f, TInput x, float dt)
    {
        TOutput p = x.Down();
        float t = x.Last;

        var k1 = f(p.Up(t));
        var k2 = f((p + dt * (k1 / 2)).Up(t + dt / 2));
        var k3 = f((p + dt * (k2 / 2)).Up(t + dt / 2));
        var k4 = f((p + dt * (k3)).Up(t + dt));
        return p + (dt / 6) * (k1 + 2 * k2 + 2 * k3 + k4);
    }
}