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
    public override ToolCategory Category => ToolCategory.Heat;

    public override void Initialize()
    {
    }

    private void LoadHeatSim()
    {
        loaded = BinarySerializer.Load<HeatSimulation>("heat.sim");
    }


    public override void DrawImGuiEdit()
    {
        if (loaded.HasValue)
        {
            ImGuiHelpers.SliderFloat("Heat Filter Min", ref heatfilterMin, 0, 1);
            ImGuiHelpers.SliderFloat("Heat Filter Max", ref heatfilterMax, heatfilterMin, 1);
            var min = loaded.Value.States[0].Time;
            var max = loaded.Value.States.Last().Time;
            ImGuiHelpers.SliderFloat("Time Filter", ref timeFilter, min, max);
        }

        if (ImGui.Button("Load heat.sim"))
        {
            LoadHeatSim();
        }

        base.DrawImGuiEdit();
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        if (!view.Is3DCamera)
            return;
        
        var dat = GetRequiredWorldService<DataService>();
        
        if (!loaded.HasValue)
            return;
            float rad = .01f;
            view.CameraOffset = -dat.VelocityField.Domain.Size.Up(-(loaded.Value.States.Length * rad))/2 ;
//            view.CameraOffset = new Vec3(-.5f, .25f, -.25f);

        GL.Enable(EnableCap.DepthTest);
        for (int index = 0; index < loaded.Value.States.Length; index++)
        {
            var state = loaded.Value.States[index];
            /*if(Math.Abs(state.Time - (Math.Sin(FlowExplainer.Time.TotalSeconds)*1 - 1f)) > .1f)
                continue;*/
            if (timeFilter < state.Time)
                continue;

            var grad = dat.ColorGradient;

            for (int i = 0; i < state.ParticleX.Length; i++)
            {
                var pos = new Vec3(state.ParticleX[i], state.ParticleY[i], -index * rad);
                if (state.ParticleHeat[i] > heatfilterMin && state.ParticleHeat[i] < heatfilterMax)
                {
                    Gizmos.Instanced.RegisterSphere(pos, rad, grad.GetCached(state.ParticleHeat[i]));
                }
            }
        }

        Gizmos.Instanced.DrawSpheres(view.Camera);
    }
}