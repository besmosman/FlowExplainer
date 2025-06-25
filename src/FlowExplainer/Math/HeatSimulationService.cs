using System.IO.Pipes;
using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using MemoryPack;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

[MemoryPackable]
public partial struct HeatSimulation
{
    public Snapshot[] States;
}

[MemoryPackable]
public partial struct Snapshot
{
    public float Time;
    public float[] ParticleX;
    public float[] ParticleY;
    public float[] ParticleHeat;
    public float[] ParticleDiffusionFlux;
    public float[] ParticleRadiationFlux;
}

public class HeatSimulationService : WorldService
{
    public override ToolCategory Category => ToolCategory.Heat;
    private BasicLagrangianHeatSim basicLagrangianHeatSim = new BasicLagrangianHeatSim();

    private float particleSpacing = 0.1f;
    private float particleRenderRadius = .0045f;


    private static float builderProgress = 0;

    public override void DrawImGuiEdit()
    {
        var dat = GetRequiredWorldService<DataService>();
        ImGuiHelpers.SliderFloat("Particle Spacing", ref particleSpacing, 0, dat.VelocityField.Domain.Size.X / 4f);
        ImGuiHelpers.SliderFloat("Radiation Factor", ref basicLagrangianHeatSim.RadiationFactor, 0, .5f);
        ImGuiHelpers.SliderFloat("Conduction Factor", ref basicLagrangianHeatSim.HeatDiffusionFactor, 0, 1);
        ImGuiHelpers.SliderFloat("Kernel Radius", ref basicLagrangianHeatSim.KernelRadius, 0, .5f);

        if (ImGui.Button("Reset"))
        {
            basicLagrangianHeatSim.Setup(dat.VelocityField.Domain, particleSpacing);
            GetRequiredWorldService<HeatSimulationViewData>().Controller = this;
            GetRequiredWorldService<HeatSimulationViewData>().ViewParticles = basicLagrangianHeatSim.Particles;
        }

        if (ImGui.Button("build"))
        {
            new Thread(() =>
            {
                Snapshot Snapshot(float time, BasicLagrangianHeatSim sim)
                {
                    return new Snapshot
                    {
                        Time = time,
                        ParticleX = sim.Particles.Select(s => s.Position.X).ToArray(),
                        ParticleY = sim.Particles.Select(s => s.Position.Y).ToArray(),
                        ParticleHeat = sim.Particles.Select(s => s.Heat).ToArray(),
                        ParticleDiffusionFlux = sim.Particles.Select(s => s.DiffusionHeatFlux).ToArray(),
                        ParticleRadiationFlux = sim.Particles.Select(s => s.RadiationHeatFlux).ToArray(),
                    };
                }

                HeatSimulationService.builderProgress = 0.001f;
                List<Snapshot> entries = new();
                var dat = GetRequiredWorldService<DataService>();

                var sim = new BasicLagrangianHeatSim();
                sim.RadiationFactor = basicLagrangianHeatSim.RadiationFactor;
                sim.HeatDiffusionFactor = basicLagrangianHeatSim.HeatDiffusionFactor;
                sim.KernelRadius = basicLagrangianHeatSim.KernelRadius;
                sim.Setup(dat.VelocityField.Domain, .01f);
                int steps = 80;
                int substeps = 10;
                float dt = 1 / 30f;
                float t = 0;
                float prewarmTime = .2f;
                var h = dt / substeps;

                while (t < prewarmTime)
                {
                    sim.Update(dat.VelocityField, t, h);
                    t += h;
                }

                for (int i = 0; i < steps; i++)
                {
                    HeatSimulationService.builderProgress = (i + 1) / ((float)steps + 2);

                    for (int j = 0; j < substeps; j++)
                    {
                        sim.Update(dat.VelocityField, t, h);
                        entries.Add(Snapshot(t, sim));
                        t += h;
                    }
                }

                HeatSimulation r = new HeatSimulation
                {
                    States = entries.ToArray(),
                };
                BinarySerializer.Save("heat.sim", r);
                builderProgress = 1;
            })
            {
                IsBackground = true
            }.Start();
        }

        if (builderProgress != 0 && builderProgress != 1)
            ImGui.ProgressBar(builderProgress, new Vector2(400, 10), "progress");

        base.DrawImGuiEdit();
    }


    public override void Draw(RenderTexture rendertarget, View view)
    {
        if(!view.Is2DCamera)
            return;
        
        var dat = GetRequiredWorldService<DataService>();
        basicLagrangianHeatSim.Update(dat.VelocityField, dat.SimulationTime, dat.DeltaTime);

        var viewer = GetRequiredWorldService<HeatSimulationViewData>();

        if (viewer.Controller == this)
            viewer.ViewParticles = basicLagrangianHeatSim.Particles;

        if (basicLagrangianHeatSim.RadiationFactor > 0)
        {
            var off = .01f;
            var thick = .012f;
            var domain = dat.VelocityField.Domain;
            Gizmos2D.Line(view.Camera2D, new Vec2(domain.Min.X, domain.Max.Y + off + thick/2), new Vec2(domain.Max.X, domain.Max.Y  + off + thick/2), dat.ColorGradient.Get(.00f), thick);
            Gizmos2D.Line(view.Camera2D, new Vec2(domain.Min.X, domain.Min.Y - off - thick/2), new Vec2(domain.Max.X, domain.Min.Y  - off - thick/2), dat.ColorGradient.Get(1f), thick);
        }
    }

    public override void Initialize()
    {
    }
}