using System.Numerics;
using ImGuiNET;
using OpenTK.Graphics.ES20;

namespace FlowExplainer;

public class HeatSimulation3DVisualizer : WorldService
{
    private HeatSimulation? loaded;
    private float heatfilterMax = 1;
    private float heatfilterMin;
    private float timeFilter;

    public override void Initialize()
    {
        loaded = BinarySerializer.Load<HeatSimulation>("heat.sim");
    }


    public override void DrawImGuiEdit()
    {
        ImGui.SliderFloat("Heat Filter Min", ref heatfilterMin, 0, 1);
        ImGui.SliderFloat("Heat Filter Max", ref heatfilterMax, heatfilterMin, 1);
        var min = loaded.Value.States[0].Time;
        var max = loaded.Value.States.Last().Time;
        ImGui.SliderFloat("Time Filter", ref timeFilter, min, max);
        base.DrawImGuiEdit();
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        GL.Enable(EnableCap.DepthTest);
        foreach (var state in loaded.Value.States)
        {
            /*if(Math.Abs(state.Time - (Math.Sin(FlowExplainer.Time.TotalSeconds)*1 - 1f)) > .1f)
                continue;*/
            if (timeFilter < state.Time)
                continue;

            var grad = Gradients.GetGradient("matlab_turbo");

            float rad = .01f;
            for (int i = 0; i < state.ParticleX.Length; i++)
            {
                var pos = new Vec3(state.ParticleX[i], state.ParticleY[i], state.Time * 1);
                if (state.ParticleHeat[i] > heatfilterMin && state.ParticleHeat[i] < heatfilterMax)
                {
                    Gizmos.Instanced.RegisterSphere(pos, rad, grad.GetCached(state.ParticleHeat[i]));
                }
            }
        }

        Gizmos.Instanced.DrawSpheres(view.Camera);
    }
}