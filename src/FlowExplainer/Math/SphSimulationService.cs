using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;


public class SphSimulationService : WorldService
{
    private Sph sph = new Sph();
    private Material material = Material.NewDefaultUnlit;
    private Material instanceMat = new Material(new Shader("Assets/Shaders/instanced.vert", ShaderType.VertexShader), Shader.DefaultUnlitFragment);

    private float particleSpacing = 0.1f;
    private float particleRenderRadius = .009f;

    public override void DrawImGuiEdit()
    {
        var dat = GetRequiredWorldService<DataService>();
        ImGui.SliderFloat("Particle Spacing", ref particleSpacing, 0, dat.Domain.Size.X / 4f);
        ImGui.SliderFloat("Render Radius", ref particleRenderRadius, 0, .1f);
        ImGui.SliderFloat("Radiation Factor", ref sph.RadiationFactor, 0, .5f);
        ImGui.SliderFloat("Conduction Factor", ref sph.HeatDiffusionFactor, 0, 1);
        ImGui.SliderFloat("Kernel Radius", ref sph.KernelRadius, 0, .5f);

        if (ImGui.Button("Reset"))
        {
            sph.Setup(dat.Domain, particleSpacing);
        }

        base.DrawImGuiEdit();
    }


    public override void Initialize()
    {
        var dat = GetRequiredWorldService<DataService>();
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        var dat = GetRequiredWorldService<DataService>();
        sph.Update(dat.VelocityField, dat.SimulationTime, dat.DeltaTime);

        var gradient = Gradients.GetGradient("matlab_turbo");

        foreach (var p in sph.Particles)
        {
            var c = p.Heat;
            Gizmos2D.Instanced.RegisterCircle(p.Position, particleRenderRadius, gradient.GetCached(p.Heat));
        }

        Gizmos2D.Instanced.RenderCircles(view.Camera2D);

        var off = .02f;
        Gizmos2D.Line(view.Camera2D, new Vec2(0, 1 + off), new Vec2(2, 1 + off), gradient.Get(.00f), .01f);
        Gizmos2D.Line(view.Camera2D, new Vec2(0, 0 - off), new Vec2(2, 0 - off), gradient.Get(1f), .01f);
    }
}