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
    private BasicLagrangianHeatSim basicLagrangianHeatSim = new BasicLagrangianHeatSim();
    private Material material = Material.NewDefaultUnlit;
    private Material instanceMat = new Material(new Shader("Assets/Shaders/instanced.vert", ShaderType.VertexShader), Shader.DefaultUnlitFragment);

    private float particleSpacing = 0.1f;
    private float particleRenderRadius = .009f;


    private static float builderProgress = 0;

    public override void DrawImGuiEdit()
    {
        var dat = GetRequiredWorldService<DataService>();
        ImGui.SliderFloat("Particle Spacing", ref particleSpacing, 0, dat.Domain.Size.X / 4f);
        ImGui.SliderFloat("Radiation Factor", ref basicLagrangianHeatSim.RadiationFactor, 0, .5f);
        ImGui.SliderFloat("Conduction Factor", ref basicLagrangianHeatSim.HeatDiffusionFactor, 0, 1);
        ImGui.SliderFloat("Kernel Radius", ref basicLagrangianHeatSim.KernelRadius, 0, .5f);

        if (ImGui.Button("Reset"))
        {
            basicLagrangianHeatSim.Setup(dat.Domain, particleSpacing);
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
                sim.Setup(dat.Domain, .03f);
                entries.Add(Snapshot(0, sim));
                int steps = 100;
                float dt = 1 / 60f;
                float t = 0;
                for (int i = 0; i < steps; i++)
                {
                    HeatSimulationService.builderProgress = (i + 1) / ((float)steps + 2);

                    sim.Update(dat.VelocityField, t, dt);
                    entries.Add(Snapshot(t, sim));
                    t += dt;
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
        var dat = GetRequiredWorldService<DataService>();
        basicLagrangianHeatSim.Update(dat.VelocityField, dat.SimulationTime, dat.DeltaTime);

        var gradient = Gradients.GetGradient("matlab_turbo");

        var viewer = GetRequiredWorldService<HeatSimulationViewData>();
        
        if (viewer.Controller == this)
            viewer.ViewParticles = basicLagrangianHeatSim.Particles;

        if (basicLagrangianHeatSim.RadiationFactor > 0)
        {
            var off = .02f;

            Gizmos2D.Line(view.Camera2D, new Vec2(0, 1 + off), new Vec2(2, 1 + off), gradient.Get(.00f), .01f);
            Gizmos2D.Line(view.Camera2D, new Vec2(0, 0 - off), new Vec2(2, 0 - off), gradient.Get(1f), .01f);
        }
    }

    public override void Initialize()
    {
    }
}