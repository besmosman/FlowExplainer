using ImGuiNET;
using OpenTK.Graphics.ES20;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common.Input;
using SixLabors.ImageSharp;

namespace FlowExplainer;

public class FlowFieldVisualizer : WorldService
{
    public override void DrawImGuiEdit()
    {
        var dat = GetRequiredWorldService<DataService>();
        var domainArea = dat.Domain.Size.X * dat.Domain.Size.Y;

        ImGui.SliderInt("Grid Cells", ref GridCells, 0, 1500);
        ImGui.SliderFloat("Length", ref Length, 0, 1);
        ImGui.SliderFloat("Thickness", ref Thickness, 0, dat.Domain.Size.Length() / 10f);
        ImGui.Checkbox("Auto Resize", ref AutoResize);
        base.DrawImGuiEdit();
    }

    public int GridCells;
    public float Length;
    public float Thickness;
    public bool AutoResize;

    public override void Initialize()
    {
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        var dat = GetRequiredWorldService<DataService>();

        var domainArea = dat.Domain.Size.X * dat.Domain.Size.Y;
        var spacing = MathF.Sqrt(domainArea / GridCells);
        var maxDirLenght2 = 0f;
        var gridSize = (dat.Domain.Size / spacing).CeilInt();
        var cellSize = dat.Domain.Size / gridSize.ToVec2();
        for (int x = 0; x < gridSize.X; x++)
        {
            for (int y = 0; y < gridSize.Y; y++)
            {
                var rel = new Vec2(x + .5f, y + .5f) / gridSize.ToVec2();
                var pos = rel * dat.Domain.Size + dat.Domain.Min;
                // Gizmos2D.RectCenter(view.Camera2D, pos, cellSize, new Vec4(x % 2 == 0 ? .4f : 0, .4f, y % 2 == 1 ? .8f : 0, 1));
                var endpos = dat.Integrator.Integrate(dat.VelocityField.Evaluate, pos.Up(dat.SimulationTime), .1f);
                var dir = dat.VelocityField.Evaluate(pos.Up(dat.SimulationTime));
                maxDirLenght2 = MathF.Max(maxDirLenght2, dir.LengthSquared());
                var color = new Color(1, 1, 1, 1);
                Gizmos2D.Circle(view.Camera2D, pos, color, Thickness / 2 * 1.0f);
                Gizmos2D.LineCentered(view.Camera2D, pos, dir * Length, color, Thickness);
                var end = pos + dir * Length / 2;
                //Gizmos2D.Circle(view.Camera2D, pos + dir*Length, new Color(1, 1, 1, 1), Thickness/2 * 1.8f);
                // var line = StreamLineGenerator.Generate(dat.VelocityField, dat.Integrator, pos, dat.SimulationTime, 0.3f, 64);
                // Gizmos2D.StreamTube(view.Camera2D, line.points, new Vec4(0, .6f, 1, 1), Thickness);
            }
        }

        if (AutoResize)
        {
            Length = (spacing / 1) / float.Sqrt(maxDirLenght2);
            Thickness = cellSize.X / 7;
        }
    }
}