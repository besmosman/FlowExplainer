namespace FlowExplainer;

public class PoincareComputer
{

    public IVectorField<Vec3, Vec2> VectorField { get; set; }
    public IIntegrator<Vec3, Vec2> Integrator { get; set; }
    
    public PoincareComputer(IVectorField<Vec3, Vec2> vectorField, IIntegrator<Vec3, Vec2> integrator)   
    {
        this.VectorField = vectorField;
        this.Integrator = integrator;
    }

    
    public Trajectory<Vec2> ComputeOne(Vec3 startPhase, float period, int stepsPerPeriod, int periods)
    {
        List<Vec2> positions = new(periods * stepsPerPeriod);
        var pos = startPhase.XY;
        float dt = period / stepsPerPeriod;
        for (int p = 0; p < periods; p++)
        {
            for (int i = 0; i < stepsPerPeriod; i++)
            {
                float t = (p * stepsPerPeriod + i) * dt + startPhase.Z;
                pos = Integrator.Integrate(VectorField, pos.Up(t), dt);
                pos = VectorField.Domain.Bounding.Bound(pos.Up(t)).XY;
                if (pos.X > 1)
                {
                    throw new Exception();  
                }
            }
            positions.Add(pos);
        }

        return new Trajectory<Vec2>(positions.ToArray());
    }
}