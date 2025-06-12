using ImGuiNET;

namespace FlowExplainer;

public class HeatSimulationReplayer : WorldService
{
    private HeatSimulation? loaded;
    private float time = 0;
    private float replaySpeed = 1;

    public override void DrawImGuiEdit()
    {
        if (ImGui.Button("Load"))
        {
            loaded = BinarySerializer.Load<HeatSimulation>("heat.sim");
            GetRequiredWorldService<HeatSimulationViewData>().Controller = this;
            GetRequiredWorldService<HeatSimulationViewData>().ViewParticles = new BasicLagrangianHeatSim.Particle[loaded.Value.States.First().ParticleX.Length];
        }

        if (loaded.HasValue)
        {
            ImGui.SliderFloat("time", ref time, loaded.Value.States.First().Time, loaded.Value.States.Last().Time);
            time += FlowExplainer.DeltaTime * replaySpeed;
            time = float.Clamp(time, loaded.Value.States.First().Time, loaded.Value.States.Last().Time);
        }

        base.DrawImGuiEdit();
    }

    public override void Initialize()
    {
    }

    public int GetCurrentReplayStep()
    {
        if (!loaded.HasValue)
            throw new Exception();
        var curStep = loaded.Value.States.Length - 2;
        var States = loaded.Value.States;
        if (time < States[0].Time)
            return 0;

        for (int i = 0; i < States.Length - 1; i++)
        {
            if (time >= States[i].Time && time < States[i + 1].Time)
            {
                curStep = i;
                break;
            }
        }

        return curStep;
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        if (loaded.HasValue)
        {
            UpdateParticles(GetRequiredWorldService<HeatSimulationViewData>().ViewParticles, loaded.Value, time);
        }
    }

    private void UpdateParticles(BasicLagrangianHeatSim.Particle[] particles, HeatSimulation heatSimulation, float f)
    {
        var curStep = GetCurrentReplayStep();
        var prevStep = heatSimulation.States[curStep];
        var nextState = heatSimulation.States[curStep + 1];
        var c = (time - prevStep.Time) / (nextState.Time - prevStep.Time);
        for (int i = 0; i < particles.Length; i++)
        {
            ref var p = ref particles[i];
            var posX = Utils.Lerp(prevStep.ParticleX[i], nextState.ParticleX[i], c);
            var posY = Utils.Lerp(prevStep.ParticleY[i], nextState.ParticleY[i], c);
            p.Position = new Vec2(posX, posY);
            p.Heat = Utils.Lerp(prevStep.ParticleHeat[i], nextState.ParticleHeat[i], c);
            p.RadiationHeatFlux = Utils.Lerp(prevStep.ParticleRadiationFlux[i], nextState.ParticleRadiationFlux[i], c);
            p.DiffusionHeatFlux = Utils.Lerp(prevStep.ParticleDiffusionFlux[i], nextState.ParticleDiffusionFlux[i], c);
        }
    }
}