using ImGuiNET;
using OpenTK.Graphics.ES20;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common.Input;
using SixLabors.ImageSharp;
using static System.Single;
using GL = OpenTK.Graphics.OpenGL.GL;

namespace FlowExplainer;

//Implementation of https://en.wikipedia.org/wiki/Bickley_jet 
public class BickleyJet : IEditabalePeriodicVectorField<Vec3, Vec2>
{
    public float M = 0.001f;
    public float v = 1.5e-5f;
    public float p = 1.225f;

    public float Period => 1;
    public Rect Domain => new Rect(new Vec2(.1f, -2), new Vec2(5f, 2));

    public float sech(float x)
    {
        return 1f / Cosh(x);
    }

    public Vec2 Evaluate(Vec3 phase)
    {
        float x = phase.X;
        float y = phase.Y;
        var ξ = 0.2752f * Pow(M / (v * v * p), 1f / 3f) * y / Pow(x, 2f / 3f);
        var vel_x = 0.4543f * Pow((M * M) / (v * p * p * x), 1f / 3f) * sech(ξ) * sech(ξ);
        var vel_y = 0.5503f * Pow((M * v) / (p * x * x), 1f / 3f) * (2 * ξ * sech(ξ) * sech(ξ) - Tanh(ξ));
        return new Vec2(vel_x, vel_y);
    }

    public void OnImGuiEdit()
    {
        ImGuiHelpers.SliderFloat("M", ref M, 0, .1f);
    }
}

public class FlowFieldVisualizer : WorldService, IAxisTitle
{
    public override void DrawImGuiEdit()
    {
        var dat = GetRequiredWorldService<DataService>();
        var domainArea = dat.VelocityField.Domain.Size.X * dat.VelocityField.Domain.Size.Y;


        ImGui.SliderInt("Grid Cells", ref GridCells, 0, 1500);
        ImGuiHelpers.SliderFloat("Length", ref Length, 0, 1);
        ImGuiHelpers.SliderFloat("Thickness", ref Thickness, 0, dat.VelocityField.Domain.Size.Length() / 10f);
        ImGui.Checkbox("Auto Resize", ref AutoResize);
        base.DrawImGuiEdit();
    }

    public int GridCells  =250;
    public float Length;
    public float Thickness;
    public bool AutoResize = true;

    public override void Initialize()
    {
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        var dat = GetRequiredWorldService<DataService>();

        var domain = dat.VelocityField.Domain;
        var domainArea = domain.Size.X * domain.Size.Y;
        var spacing = MathF.Sqrt(domainArea / GridCells);
        var maxDirLenght2 = 0f;
        var gridSize = (domain.Size / spacing).CeilInt();
        var cellSize = domain.Size / gridSize.ToVec2();
        for (int x = 0; x < gridSize.X; x++)
        {
            for (int y = 0; y < gridSize.Y; y++)
            {
                var rel = new Vec2(x + .5f, y + .5f) / gridSize.ToVec2();
                //if (y % 2 == 0) rel.X += .5f / gridSize.X;
                var pos = rel * domain.Size + domain.Min;
                var dir = dat.VelocityField.Evaluate(pos.Up(dat.SimulationTime));
                maxDirLenght2 = MathF.Max(maxDirLenght2, dir.LengthSquared());
                var color = dat.ColorGradient.Get(dir.Length() * 1);
                //color = new Color((dir + new Vec2(.1f,.1f)).Up(0).Up(1));
                /*var traj = IFlowOperator<Vec2, Vec3>.Default.Compute(dat.SimulationTime, dat.SimulationTime + .05f, pos, dat.VelocityField);
                var sum = 0f;
                for (int i = 1; i < traj.Entries.Length; i++)
                {
                    var last = traj.Entries[i - 1];
                    var cur = traj.Entries[i];
                    sum += (cur.Down() - last.Down()).Length() / (cur.Last - last.Last);
                }

                var avgSpeed = traj.AverageAlong((prev, cur) => (cur.XY - prev.XY).Abs() / (cur.Last - prev.Last));

                color = new Color(0, 0, avgSpeed.LengthSquared(), 1);
                  color = dat.ColorGradient.Get(avgSpeed.LengthSquared());*/


                var top = pos + dir * Length;
                var dirPerp = new Vec2(-dir.Y, dir.X);
                float length = 1;
                Gizmos2D.Instanced.RegisterCircle(pos, Thickness * .7f, color);
                Gizmos2D.Instanced.RegisterLine(pos, pos + dir * Length, color, Thickness * length);
                //Gizmos2D.Instanced.RegisterLine(top, top + (dirPerp + -dir) /2 *Thickness*3*length,  color, Thickness * length);
                //Gizmos2D.Instanced.RegisterLine(top, top + (-dirPerp + -dir) /2 * Thickness*3*length,  color, Thickness * length);
                //Gizmos2D.Instanced.RegisterCircle(top, Thickness * .5f * length, color);
                //Gizmos2D.Instanced.RegisterLineCentered(pos + dir * Length, new Vec2(-dir.Y, dir.X) * Length/2, color, Thickness);


                //var end = pos + dir * Length / 2;
                // Gizmos2D.Circle(view.Camera2D, pos + dir*Length, new Color(1, 1, 1, 1), Thickness/2);
                //var line = StreamLineGenerator.Generate(dat.VelocityField, dat.Integrator, pos, dat.SimulationTime, 0.3f, 64);
                //var traj = IFlowOperator<Vec2, Vec3>.Default.Compute(dat.SimulationTime, dat.SimulationTime + .1f, pos, dat.VelocityField);
                //Gizmos2D.StreamTube(view.Camera2D, traj.Entries.Select(s => s.XY).ToList(), color, Thickness);
            }
        }

        Gizmos2D.Instanced.RenderCircles(view.Camera2D);
        Gizmos2D.Instanced.RenderRects(view.Camera2D);

        if (AutoResize)
        {
            Length = (spacing / .7f) / Sqrt(maxDirLenght2);
            Thickness = cellSize.X / 4;
        }
    }

    public string GetTitle()
    {
        return "Velocity Field";
    }
}