using System.Collections.Concurrent;
using ImGuiNET;

namespace FlowExplainer;

public class PoincareVisualizer : WorldService
{
    private ConcurrentBag<PoincareComputer.Trajectory> Trajectories = new();

    public int Periods = 20;
    public int StepsPerPeriod = 500;
    public int StartPoints;
    public override void DrawImGuiEdit()
    {
        ImGui.SliderInt("Periods", ref Periods, 0, 5000);
        ImGui.SliderInt("Integrations per period", ref StepsPerPeriod, 0, 2000);
        ImGui.SliderInt("Start points", ref StartPoints, 0, 2000);
        if (ImGui.Button("Generate"))
        {
            var dat = GetWorldService<DataService>();
            var poincare = new PoincareComputer(dat.VelocityField, dat.Integrator);

            List<Vec2> start = new();
            
            for (int i = 0; i < StartPoints; i++)
            {
                float t = i / (StartPoints-1f);
                start.Add(new Vec2(t * dat.Domain.Size.X, dat.Domain.Center.Y));
            }

            Trajectories.Clear();
            Parallel.ForEach(start, (p) =>
            {
                var traj = poincare.ComputeOne(p, dat.VelocityField.Period, StepsPerPeriod, Periods);
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
        foreach (var t in Trajectories)
        {
            foreach (var p in t.Points)
            {
                var color = new Color(1, 0, 1);
                Gizmos2D.Instanced.RegisterCircle(p, .003f, color);
            }
        }
        Gizmos2D.Instanced.RenderCircles(view.Camera2D);
    }
}