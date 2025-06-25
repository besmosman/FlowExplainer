using System.Collections.Concurrent;
using System.Numerics;
using ImGuiNET;

namespace FlowExplainer;


public class PoincareVisualizer : WorldService
{
    private ConcurrentBag<PoincareComputer.Trajectory> Trajectories = new();

    public int Periods = 20;
    public int StepsPerPeriod = 500;
    public int StartPoints;
    public float RenderRadius;
    public float Offset;

    public override void DrawImGuiEdit()
    {
        var dat = GetRequiredWorldService<DataService>();
        ImGui.SliderInt("Periods", ref Periods, 0, 5000);
        ImGui.SliderInt("Integrations per period", ref StepsPerPeriod, 0, 2000);
        ImGui.SliderInt("Start points", ref StartPoints, 0, 2000);
        ImGuiHelpers.SliderFloat("Render radius", ref RenderRadius, 0, .1f);
        ImGuiHelpers.SliderFloat("Offset", ref Offset, 0, dat.VelocityField.Period);
        if (ImGui.Button("Generate"))
        {
            var poincare = new PoincareComputer(dat.VelocityField, dat.Integrator);

            List<Vec2> start = new();

            for (int i = 0; i < StartPoints; i++)
            {
                float t = i / (StartPoints - 1f);
                start.Add(new Vec2(t * dat.VelocityField.Domain.Size.X, 1 / 4f));
            }

            Trajectories.Clear();
            Parallel.ForEach(start, (p) =>
            {
                var traj = poincare.ComputeOne(p.Up(Offset), dat.VelocityField.Period, StepsPerPeriod, Periods);
                Trajectories.Add(traj);
            });
        }

        base.DrawImGuiEdit();
    }

    public override void Initialize()
    {
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        if(!view.Is2DCamera)
            return;
        
        foreach (var t in Trajectories)
        {
            foreach (var p in t.Points)
            {
                var color = new Color(1, 1, 1, 1f);
                Gizmos2D.Instanced.RegisterCircle(p, RenderRadius, color);
            }
        }

        Gizmos2D.Instanced.RenderCircles(view.Camera2D);
    }
}