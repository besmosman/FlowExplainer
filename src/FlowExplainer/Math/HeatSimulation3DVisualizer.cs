using System.Numerics;
using ImGuiNET;
using OpenTK.Graphics.ES20;

namespace FlowExplainer;

public class HeatSimulationToField
{
    public static void Convert(HeatSimulation simulation)
    {
        
        /*Vec3i dimensions = 
        
        
        RegularGridVectorField<Vec3, Vec3i, float> heatField = new(*/
    }
}

public class Heat3DViewer : WorldService
{
    private List<(Vec3, float h)> particles = new();

    HeatSimulation? loaded = BinarySerializer.Load<HeatSimulation>("heat.sim");

    public override void Initialize()
    {
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        if (!view.Is3DCamera)
            return;

        Update();

        var th = .02f;
        GL.Enable(EnableCap.DepthTest);
        Gizmos.DrawLine(view, Vec3.Zero, new Vec3(1, 0, 0), th, new Color(1, 0, 0));
        Gizmos.DrawLine(view, Vec3.Zero, new Vec3(0, 1, 0), th, new Color(0, 1, 0));
        Gizmos.DrawLine(view, Vec3.Zero, new Vec3(0, 0, 1), th, new Color(0, 0, 1));

        var grad = GetRequiredWorldService<DataService>().ColorGradient;
        foreach (var p in particles)
        {
            var z = p.Item1;
            Gizmos.Instanced.RegisterSphere(z, .01f, grad.GetCached(p.h));
        }

        Gizmos.Instanced.DrawSpheres(view.Camera);
    }

    private void Update()
    {
        particles.Clear();
        //thresh *= .001f;
        for (int s = 1; s < loaded.Value.States.Length-1; s++)
        {
            var states = loaded.Value.States;
            var state = states[s];
            for (int i = 0; i < state.ParticleHeat.Length; i++)
            {
                var flux = (states[s].ParticleHeat[i] - states[s - 1].ParticleHeat[i]) / (states[s].Time - states[s - 1].Time);
                var fluxNext = (states[s+1].ParticleHeat[i] - states[s].ParticleHeat[i]) / (states[s+1].Time - states[s].Time);
                var thresh = .8f;
                
                if (flux >= thresh
                   )
                {
                    particles.Add((new Vec3(states[s].ParticleX[i] , states[s].ParticleY[i], states[s].Time), flux));
                }
            }
        }
    }
}

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
        view.CameraOffset = -dat.VelocityField.Domain.Size.Up(-(loaded.Value.States.Length * rad)) / 2;
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