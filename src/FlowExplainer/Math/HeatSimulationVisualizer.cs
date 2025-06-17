using ImGuiNET;

namespace FlowExplainer;

public class HeatSimulationVisualizer : WorldService
{
    public float RenderRadius = .01f;

    public Colorings Coloring;
    public bool Scaled;

    public enum Colorings
    {
        Heat,
        DiffusionFlux,
        RadiationFlux,
    }

    public override void Initialize()
    {
    }

    public override void DrawImGuiEdit()
    {
        ImGui.SliderFloat("Render Radius", ref RenderRadius, 0, .06f);
        int selected = (int)Coloring;
        var names = Enum.GetNames<Colorings>();
        if (ImGui.Combo("Color by", ref selected, names, names.Length))
        {
            Coloring = (Colorings)selected;
        }

        ImGui.Checkbox("Scaled", ref Scaled);


        base.DrawImGuiEdit();
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        if(!view.Is2DCamera)
            return;
        
        var particles = GetRequiredWorldService<HeatSimulationViewData>().ViewParticles;
        if (particles != null)
        {
            var min = float.MaxValue;
            var max = float.MinValue;
            foreach (ref var p in particles.AsSpan())
            {
                var c = p.Heat;
                switch (Coloring)
                {
                    case Colorings.Heat:
                        c = p.Heat;
                        break;
                    case Colorings.DiffusionFlux:
                        c = .5f  -p.DiffusionHeatFlux;
                        break;
                    case Colorings.RadiationFlux:
                        c = .5f  -p.RadiationHeatFlux;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                min = float.Min(min, c);
                max = float.Max(max, c);
            }

            var grad = Gradients.GetGradient("matlab_jet");

            foreach (ref var p in particles.AsSpan())
            {
                var c = p.Heat;
                switch (Coloring)
                {
                    case Colorings.Heat:
                        c = p.Heat;
                        break;
                    case Colorings.DiffusionFlux:
                        c = .5f  -p.DiffusionHeatFlux;
                        break;
                    case Colorings.RadiationFlux:
                        c = .5f -p.RadiationHeatFlux;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                float t = c;
                
                if(Scaled)
                    t = (c - min) / (max - min);

                //t = -(p.LastHeat - p.Heat)*500 + .5f;
                Gizmos2D.Instanced.RegisterCircle(p.Position, RenderRadius, grad.GetCached(t));
            }

            Gizmos2D.Instanced.RenderCircles(view.Camera2D);
        }
    }
}