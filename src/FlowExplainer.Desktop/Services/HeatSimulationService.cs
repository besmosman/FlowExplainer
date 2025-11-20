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
    public double Time;
    public double[] ParticleX;
    public double[] ParticleY;
    public double[] ParticleHeat;
    public double[] ParticleDiffusionFlux;
    public double[] ParticleRadiationFlux;
}

public class HeatSimulationService : WorldService
{
    public BasicLagrangianHeatSim basicLagrangianHeatSim = new BasicLagrangianHeatSim();

    public double particleSpacing = 0.1f;



    private static double builderProgress = 0;

    public override void DrawImGuiSettings()
    {
        var dat = GetRequiredWorldService<DataService>();
        ImGuiHelpers.SliderFloat("Particle Spacing", ref particleSpacing, 0, dat.VectorField.Domain.RectBoundary.Size.X / 4f);
        ImGuiHelpers.SliderFloat("Radiation Factor", ref basicLagrangianHeatSim.RadiationFactor, 0, .05f);
        ImGuiHelpers.SliderFloat("Conduction Factor", ref basicLagrangianHeatSim.HeatDiffusionFactor, 0, .1f);
        ImGuiHelpers.SliderFloat("Kernel Radius", ref basicLagrangianHeatSim.KernelRadius, 0, .5f);

        if (ImGui.Button("Reset"))
        {
            Reset();
        }

        if (ImGui.Button("build"))
        {
            new Thread(() =>
            {
                Snapshot Snapshot(double time, BasicLagrangianHeatSim sim)
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
                sim.Setup(dat.VectorField.Domain.RectBoundary.Reduce<Vec2>(), .01f);
                int steps = 80;
                int substeps = 10;
                double dt = 1 / 30f;
                double t = 0;
                double prewarmTime = .2f;
                var h = dt / substeps;

                while (t < prewarmTime)
                {
                    sim.Update(dat.VectorField, t, h);
                    t += h;
                }

                for (int i = 0; i < steps; i++)
                {
                    HeatSimulationService.builderProgress = (i + 1) / ((double)steps + 2);

                    for (int j = 0; j < substeps; j++)
                    {
                        sim.Update(dat.VectorField, t, h);
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
            ImGui.ProgressBar((float)builderProgress, new Vector2(400, 10), "progress");

        base.DrawImGuiSettings();
    }
    public void Reset()
    {

        var dat = GetRequiredWorldService<DataService>();
        basicLagrangianHeatSim.Setup(dat.VectorField.Domain.RectBoundary.Reduce<Vec2>(), particleSpacing);
        GetRequiredWorldService<HeatSimulationViewData>().Controller = this;
        GetRequiredWorldService<HeatSimulationViewData>().ViewParticles = basicLagrangianHeatSim.Particles;
    }


    public override void Draw(RenderTexture rendertarget, View view)
    {
        if(!view.Is2DCamera)
            return;
        
        var dat = GetRequiredWorldService<DataService>();
        basicLagrangianHeatSim.Update(dat.VectorField, dat.SimulationTime, dat.MultipliedDeltaTime);

        var viewer = GetRequiredWorldService<HeatSimulationViewData>();

        if (viewer.Controller == this)
            viewer.ViewParticles = basicLagrangianHeatSim.Particles;

        if (basicLagrangianHeatSim.RadiationFactor > 0)
        {
            var off = .01f;
            var thick = .012f;
            var domain = dat.VectorField.Domain.RectBoundary.Reduce<Vec2>();
            Gizmos2D.Line(view.Camera2D, new Vec2(domain.Min.X, domain.Max.Y + off + thick/2), new Vec2(domain.Max.X, domain.Max.Y  + off + thick/2), dat.ColorGradient.Get(.00f), thick);
            Gizmos2D.Line(view.Camera2D, new Vec2(domain.Min.X, domain.Min.Y - off - thick/2), new Vec2(domain.Max.X, domain.Min.Y  - off - thick/2), dat.ColorGradient.Get(1f), thick);
        }
    }

    public override void Initialize()
    {
    }
}