using System.Numerics;
using ImGuiNET;

namespace FlowExplainer;

public class SphSimulationService : WorldService
{
    private Sph sph = new Sph();
    private Material material = Material.NewDefaultUnlit;

    private float particleSpacing = 0.1f;

    public override void DrawImGuiEdit()
    {
        var dat = GetRequiredWorldService<DataService>();
        ImGui.SliderFloat("Particle Spacing", ref particleSpacing, 0, dat.Domain.Size.X / 4f);
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

        /*var gradient = new Gradient<Color>([
            (0.00f, new(0, 0, .4f)),
            (0.02f, new(0, 0, 1f)),
            (0.4f, new(0, 1f, 1)),
            (0.6f, new(1, 1, 0)),
            (0.98f, new(1f, 0, 0f)),
            (1.00f, new(.4f, 0, 0f)),
        ]);*/
        var gradient = Gradients.GetGradient("matlab_turbo");
        var camera = view.Camera2D;
        material.Use();
        material.SetUniform("view", camera.GetViewMatrix());
        material.SetUniform("projection", camera.GetProjectionMatrix());
        foreach (ref var p in sph.Particles.AsSpan())
        {
            //var color = gradient.Get(float.Clamp(p.DiffusionHeatFlux / dt * -.7f + .5f, 0, 1));
            var color = gradient.Get(float.Clamp(p.Heat, 0, 1));
            //var color = gradient.Get(float.Clamp(p.tag, 0,1));

            var center = p.Position;
            var radius = .009f;
            material.SetUniform("tint", color);
            material.SetUniform("model", Matrix4x4.CreateScale(radius, radius, 1) * Matrix4x4.CreateTranslation(center.X, center.Y, 0));
            Gizmos2D.circleMesh.Draw();
        }
    }
}