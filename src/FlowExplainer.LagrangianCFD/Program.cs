
using FlowExplainer;

public struct Particle
{
    public Vec2 Position;
    public double T;
}

public class HeatSim
{
    public IDomain<Vec2> Domain;
    public double Pe;
    public int ParticleCount;
    public Particle[] Particles;
    public IIntegrator<Vec3, Vec2> Integrator = new RungeKutta4IntegratorGen<Vec3, Vec2>();



    public HeatSim(IDomain<Vec2> domain, double pe, double particleSpacing)
    {
        Domain = domain;
        Pe = pe;

        particleSpacing;
        ParticleCount = particleCount;
        Particles = new Particle[ParticleCount];
        
        
        domain.RectBoundary
        
    }
    public void Step(double dt)
    {
        
    }
}
