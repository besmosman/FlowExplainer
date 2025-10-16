using System.Collections.Concurrent;
using System.Numerics;
using ImGuiNET;

namespace FlowExplainer;

public interface IAxisTitle
{
    public string GetTitle();
}

public interface IGradientScaler
{
    public (float min, float max) GetScale();
}

public class PoincareVisualizer : WorldService, IAxisTitle
{
    private ConcurrentBag<Trajectory<Vec2>> Trajectories = new();

    public int Periods = 200;
    public int StepsPerPeriod = 100;
    public int StartPoints = 12;
    public float RenderRadius = .004f;
    public float Offset;


    public string GetTitle() => "Poincaré Section";

    public override void DrawImGuiEdit()
    {
        var dat = GetRequiredWorldService<DataService>();
        ImGui.SliderInt("Periods", ref Periods, 0, 5000);
        ImGui.SliderInt("Integrations per period", ref StepsPerPeriod, 0, 2000);
        ImGui.SliderInt("Start points", ref StartPoints, 0, 2000);
        ImGuiHelpers.SliderFloat("Render radius", ref RenderRadius, 0, .1f);
        ImGuiHelpers.SliderFloat("Offset", ref Offset, 0, dat.VectorField.Domain.RectBoundary.Size.Last);
        if (ImGui.Button("Generate"))
        {
            var integrator = IIntegrator<Vec3, Vec2>.Rk4;
            var poincare = new PoincareComputer(dat.VectorField, integrator);

            List<Vec2> start = new();

            for (int i = 0; i < StartPoints; i++)
            {
                float t = i / (StartPoints - 1f);
                if (StartPoints == 1)
                    t = .5f;

                //start.Add(new Vec2(t * dat.VectorField.Domain.RectBoundary.Size.X, 1 / 4f));
                start.Add(Utils.Random(dat.VectorField.Domain.RectBoundary).XY);
            }

            Trajectories.Clear();
            Parallel.ForEach(start, (p) =>
            {
                var traj = poincare.ComputeOne(p.Up(Offset), dat.VectorField.Domain.RectBoundary.Size.Z, StepsPerPeriod, Periods);
                Trajectories.Add(traj);
            });


            /*Parallel.ForEach(start, (p) =>
            {
                var totalTime = 0f;
                var x = p.Up(Offset);
                float dt = 1f / StepsPerPeriod;
                List<Vec2> hits = new();
                while (totalTime < Periods)
                {
                    var last = x;
                    float slice = .3f;
                    var prewrap = integrator.Integrate(dat.VectorField, x, dt).Up(x.Last + dt);
                    x = dat.VectorField.Boundary.Wrap(prewrap);
                    if ((x.X <= slice && last.X > slice) || 
                        (x.X > slice && last.X <= slice))
                    {
                        var hitPhase = (x + last) / 2f;
                        hits.Add(new Vec2(hitPhase.Z, hitPhase.Y));
                    }
                    totalTime += dt;
                }
                Trajectories.Add(new Trajectory<Vec2>(hits.ToArray()));
            });*/

        }

        base.DrawImGuiEdit();
    }

    public override void Initialize()
    {
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        if (!view.Is2DCamera)
            return;

        foreach (var t in Trajectories)
        {
            foreach (var p in t.Entries)
            {
                var color = new Color(1, 1, 1, 1f);
                Gizmos2D.Instanced.RegisterCircle(p, RenderRadius, new Color((t.GetHashCode() / 100f) % 1, 0, 1));
            }
        }

        Gizmos2D.Instanced.RenderCircles(view.Camera2D);
    }
}