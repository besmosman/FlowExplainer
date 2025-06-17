namespace FlowExplainer;

public class PoincareComputer
{
    public class Trajectory
    {
        public Vec3 StartPhase;
        public List<Vec2> Points;
    }

    public IVectorField<Vec3, Vec2> VectorField { get; set; }
    public IIntegrator<Vec3, Vec2> Integrator { get; set; }


    public PoincareComputer(IVectorField<Vec3, Vec2> vectorField, IIntegrator<Vec3, Vec2> integrator)
    {
        this.VectorField = vectorField;
        this.Integrator = integrator;
    }

    public Trajectory ComputeOne(Vec3 startPhase, float period, int stepsPerPeriod, int periods)
    {
        List<Vec2> positions = new(periods * stepsPerPeriod);
        var pos = startPhase.XY;
        float dt = period / stepsPerPeriod;
        for (int p = 0; p < periods; p++)
        {
            for (int i = 0; i < stepsPerPeriod; i++)
            {
                float t = (p * stepsPerPeriod + i) * dt + startPhase.Z;
                pos = Integrator.Integrate(VectorField.Evaluate, pos.Up(t), dt);
            }
            positions.Add(pos);
        }

        return new Trajectory
        {
            Points = positions,
            StartPhase = startPhase,
        };
    }
}