using ImGuiNET;

namespace FlowExplainer;

public class HeatSimulationVisualizer : WorldService, IAxisTitle
{
    public override ToolCategory Category => ToolCategory.Heat;
    public double RenderRadius = .01f;

    public Colorings Coloring;
    public bool Scaled;

    public enum Colorings
    {
        Heat,
        DiffusionFlux,
        ConvectionFlux,
        RadiationFlux,
        Tag,
    }

    public override void Initialize()
    {
    }

    public override void DrawImGuiEdit()
    {
        ImGuiHelpers.SliderFloat("Render Radius", ref RenderRadius, 0, .06f);
        int selected = (int)Coloring;
        var names = Enum.GetNames<Colorings>();
        if (ImGui.Combo("Color by", ref selected, names, names.Length))
        {
            Coloring = (Colorings)selected;
        }

        ImGui.Checkbox("Scaled", ref Scaled);


        base.DrawImGuiEdit();
    }

    private double lastMin;
    private double lastMax;

    public override void Draw(RenderTexture rendertarget, View view)
    {
        if (!view.Is2DCamera)
            return;

        var particles = GetRequiredWorldService<HeatSimulationViewData>().ViewParticles;
        if (particles != null)
        {
            GetRequiredWorldService<AxisVisualizer>().titler = this;
            var min = double.MaxValue;
            var max = double.MinValue;
            foreach (ref var p in particles.AsSpan())
            {
                var c = p.Heat;
                switch (Coloring)
                {
                    case Colorings.Heat:
                        c = p.Heat;
                        break;
                    case Colorings.Tag:
                        c = p.Tag;
                        break;
                    case Colorings.DiffusionFlux:
                        c = .5f + p.DiffusionHeatFlux / 2;
                        break;
                    case Colorings.ConvectionFlux:
                        c = .5f + p.TotalConvectionHeatFlux / 2;
                        break;
                    case Colorings.RadiationFlux:
                        c = .5f + p.RadiationHeatFlux / 2;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (double.IsRealNumber(c))
                {
                    min = double.Min(min, c);
                    max = double.Max(max, c);
                }
            }

            var grad = GetRequiredWorldService<DataService>().ColorGradient;

            min = Utils.Lerp(lastMin, min, .06);
            min = 0;
            max = Utils.Lerp(lastMax, max, .06);
            lastMin = min;
            lastMax = max;
            /*max *= .8f;
            min *= 1.2f;*/
            foreach (ref var p in particles.AsSpan())
            {
                var c = p.Heat;
                switch (Coloring)
                {
                    case Colorings.Heat:
                        c = p.Heat;
                        break;
                    case Colorings.Tag:
                        c = p.Tag;
                        break;
                    case Colorings.DiffusionFlux:
                        c = .5f + p.DiffusionHeatFlux / 2;
                        break;
                    case Colorings.ConvectionFlux:
                        c = .5f + p.TotalConvectionHeatFlux / 2;
                        break;
                    case Colorings.RadiationFlux:
                        c = .5f + p.RadiationHeatFlux / 2;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                double t = c;

                if (Scaled)
                    t = (c - min) / (max - min);

                //t = -(p.LastHeat - p.Heat)*500 + .5f;
                Gizmos2D.Instanced.RegisterCircle(p.Position, RenderRadius, grad.GetCached(t));
            }


            Gizmos2D.Instanced.RenderCircles(view.Camera2D);
        }
    }

    public string GetTitle()
    {
        return "Particles (" + Enum.GetName(Coloring) + ")";
    }
}