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

    public List<Vec2> ComputeOne(Vec2 startPos, float period, int stepsPerPeriod, int periods)
    {
        List<Vec2> positions = new();
        float t = 0f;
        var pos = startPos;
        float dt = period / stepsPerPeriod;
        for (int p = 0; p < periods; p++)
        {
            for (int i = 0; i <= stepsPerPeriod; i++)
            {
                pos = Integrator.Integrate(VectorField.Evaluate, pos.Up(t), dt);
                t += dt;
            }

            positions.Add(pos);
        }

        return positions;
    }
}