namespace FlowExplainer;

public class HeatSimulationViewData : WorldService
{

    public BasicLagrangianHeatSim.Particle[]? ViewParticles;
    public WorldService Controller;


    public override void Initialize()
    {
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
    }


}