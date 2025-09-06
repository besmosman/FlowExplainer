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
                    t.CurPosition = integrator.Integrate(vectorField, t.CurPosition.Up(time), ddt);
                }

                t.Trajectory.Add(t.CurPosition.Up(time));
            }

            time += dt;
        }
    }
}