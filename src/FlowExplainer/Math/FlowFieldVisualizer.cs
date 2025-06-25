using ImGuiNET;
using OpenTK.Graphics.ES20;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common.Input;
using SixLabors.ImageSharp;
using static System.Single;
using GL = OpenTK.Graphics.OpenGL.GL;

namespace FlowExplainer;

//https://coherentstructures.github.io/CoherentStructures.jl/stable/generated/bickley/
public class BickleyJet2 : IEditabalePeriodicVectorField<Vec3, Vec2>
{
    public float Period => 1;
    public Rect Domain => new Rect(new Vec2(0, -3), new Vec2(20, 3));

    float r0 = 4371e-3f;
    float U0 = 62.66e-3f;
    float ε1 = 0.0075f;
    float ε2 = 0.15f;
    float ε3 = 0.3f;
    float L0 = 1770e-3f;

    public float streamFunction(Vec3 phase)
    {
        float k1 = 2f / r0;
        float k2 = 4f / r0;
        float k3 = 6f / r0;

        float c2 = 0.205f * U0;
        float c3 = 0.461f * U0;
        float c1 = c3 + (Sqrt(5) - 1f) * (c2 - c3);
        var x = phase.X;
        var y = phase.Y;
        var t = phase.Z;

        var psi0 = -U0 * L0 * Tanh(y / L0);

        var Σ1 = ε1 * Cos(k1 * (x - c1 * t));
        var Σ2 = ε2 * Cos(k2 * (x - c2 * t));
        var Σ3 = ε3 * Cos(k3 * (x - c3 * t));
        var re_sum_term = Σ1 + Σ2 + Σ3;
        var psi1 = U0 * L0 * Pow(sech(y / L0), 2) * re_sum_term;
        var psi = psi0 + psi1;
        return psi;
    }

    public float sech(float x)
    {
        return 1f / Cosh(x);
    }

    //source online differentiate tool
    public Vec2 Evaluate(Vec3 phase)
    {
        float r0 = 4371e-3f;
        float U0 = 62.66e-3f;
        float ε1 = 0.0075f;
        float ε2 = 0.15f;
        float ε3 = 0.3f;
        float L0 = 1770e-3f;

        float k1 = 2f / r0;
        float k2 = 4f / r0;
        float k3 = 6f / r0;

        float c2 = 0.205f * U0;
        float c3 = 0.461f * U0;
        float c1 = c3 + (Sqrt(5) - 1f) * (c2 - c3);

        var x = phase.X;
        var y = phase.Y;
        var t = phase.Z;

        // Common terms
        float sech_y_L0 = sech(y / L0);
        float tanh_y_L0 = Tanh(y / L0);

        var Σ1 = ε1 * Cos(k1 * (x - c1 * t));
        var Σ2 = ε2 * Cos(k2 * (x - c2 * t));
        var Σ3 = ε3 * Cos(k3 * (x - c3 * t));
        var re_sum_term = Σ1 + Σ2 + Σ3;

        // u = ∂ψ/∂y
        float dpsi0_dy = -U0 * sech_y_L0 * sech_y_L0;
        float dpsi1_dy = -2f * U0 * sech_y_L0 * sech_y_L0 * tanh_y_L0 * re_sum_term / L0;
        float u = dpsi0_dy + dpsi1_dy;

        // v = -∂ψ/∂x  
        var dΣ1_dx = -ε1 * k1 * Sin(k1 * (x - c1 * t));
        var dΣ2_dx = -ε2 * k2 * Sin(k2 * (x - c2 * t));
        var dΣ3_dx = -ε3 * k3 * Sin(k3 * (x - c3 * t));
        var dre_sum_dx = dΣ1_dx + dΣ2_dx + dΣ3_dx;

        float v = U0 * L0 * sech_y_L0 * sech_y_L0 * dre_sum_dx;

        // return FiniteDifferences(phase);
        return new Vec2(u, v);
    }

    private Vec2 FiniteDifferences(Vec3 x)
    {
        var d = .01f;
        var dx = (streamFunction(x + new Vec3(-d, 0, 0)) - streamFunction(x + new Vec3(d, 0, 0))) / (2 * d);
        var dy = (streamFunction(x + new Vec3(0, -d, 0)) - streamFunction(x + new Vec3(0, d, 0))) / (2 * d);
        return new Vec2(-dy, dx);
    }


    public void OnImGuiEdit()
    {
    }
}

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

public class FlowFieldVisualizer : WorldService
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

    public int GridCells;
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
}