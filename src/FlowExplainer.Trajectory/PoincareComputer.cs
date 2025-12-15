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

    
    public Trajectory<Vec2> ComputeOne(Vec3 startPhase, double period, int stepsPerPeriod, int periods)
    {
        List<Vec2> positions = new(periods * stepsPerPeriod);
        var phase = startPhase;
        double dt = period / stepsPerPeriod;
        for (int p = 0; p < periods; p++)
        {
            //phase.Last = startPhase.Last;
            for (int i = 0; i < stepsPerPeriod; i++)
            {
                //double t = (p * stepsPerPeriod + i) * dt + startPhase.Z;
                phase = Integrator.Integrate(VectorField, phase, dt);
                phase = VectorField.Domain.Bounding.Bound(phase);
            }
            positions.Add(phase.XY);
        }

        return new Trajectory<Vec2>(positions.ToArray());
    }
}