using ImGuiNET;

namespace FlowExplainer;

public class HeatSimulationVisualizer : WorldService
{
    public float RenderRadius = .01f;

    public override void Initialize()
    {
    }

    public override void DrawImGuiEdit()
    {
        ImGui.SliderFloat("Render Radius", ref RenderRadius, 0, .06f);
        base.DrawImGuiEdit();
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        var particles = GetRequiredWorldService<HeatSimulationViewData>().ViewParticles;
        if (particles != null)
        {
            var grad = Gradients.GetGradient("matlab_turbo");

            foreach (ref var p in particles.AsSpan())
            {
                Gizmos2D.Instanced.RegisterCircle(p.Position, RenderRadius, grad.GetCached(p.Heat));
            }

            Gizmos2D.Instanced.RenderCircles(view.Camera2D);
        }
    }
}