using ImGuiNET;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class ArrowVisualizer : WorldService, IAxisTitle
{
    public override void DrawImGuiSettings()
    {
        var dat = GetRequiredWorldService<DataService>();
        var domainArea = dat.VectorField.Domain.RectBoundary.Size.X * dat.VectorField.Domain.RectBoundary.Size.Y;


        ImGui.SliderInt("Grid Cells", ref GridCells, 0, 1500);
        ImGuiHelpers.Slider("Length", ref Length, 0, 1);
        ImGui.Checkbox("Color by gradient", ref colorByGradient);
        ImGuiHelpers.Slider("Thickness", ref Thickness, 0, dat.VectorField.Domain.RectBoundary.Size.Length() / 10f);
        ImGui.Checkbox("Auto Resize", ref AutoResize);
        base.DrawImGuiSettings();
    }

    public int GridCells = 250;
    public double Length;
    public double Thickness;
    public bool AutoResize = true;
    public bool colorByGradient = true;


    public override string? Name => "Arrow Glyphs";
    public override string? CategoryN => "Vectorfield";
    public override string? Description => "Visualize a vectorfield using arrow glyphs";

    public IVectorField<Vec3, Vec2>? AltVectorfield;
    public double? AltTime;
    public ColorGradient? AltGradient;

    public override void Initialize()
    {
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        if (!view.Is2DCamera)
            return;

        var dat = GetRequiredWorldService<DataService>();
        var vectorfield = AltVectorfield ?? dat.VectorField;
        var t = AltTime ?? dat.SimulationTime;
        var gradient = AltGradient ?? dat.ColorGradient;

        var domain = vectorfield.Domain.RectBoundary;
        var domainSize = domain.Size.Down();
        var domainArea = domainSize.X * domainSize.Y;
        var spacing = Math.Sqrt(domainArea / GridCells);
        var maxDirLenght2 = 0.0;
        var gridSize = (domainSize / spacing).CeilInt();
        var cellSize = domainSize / gridSize.ToVec2();
        var domainBounding = vectorfield.Domain.Bounding;
        for (int x = 0; x < gridSize.X; x++)
        {
            for (int y = 0; y < gridSize.Y; y++)
            {
                var rel = new Vec2(x + .5f, y + .5f) / gridSize.ToVec2();
                var pos = rel * domainSize + domain.Min.Down();
                var dir = vectorfield.Evaluate(pos.Up(t));
                if (double.IsNaN(dir.X) || double.IsNaN(dir.Y))
                    continue;
                maxDirLenght2 = Math.Max(maxDirLenght2, dir.LengthSquared());
            }
        }
        for (int x = 0; x < gridSize.X; x++)
        {
            for (int y = 0; y < gridSize.Y; y++)
            {
                var rel = new Vec2(x + .5f, y + .5f) / gridSize.ToVec2();
                var pos = rel * domainSize + domain.Min.Down();
                var dir = vectorfield.Evaluate(domainBounding.Bound(pos.Up(t)));
                if (double.IsNaN(dir.X) || double.IsNaN(dir.Y))
                    continue;

                dir = double.Clamp(((dir.Length()) / (double.Sqrt(maxDirLenght2))), .2f, .9f) * Vec2.Normalize(dir);
                if (double.IsNaN(dir.X) || double.IsNaN(dir.Y))
                    continue;

                var color = gradient.Get(0);
                if (maxDirLenght2 != 0)
                    color = gradient.Get(dir.Length() * 1);

                if (!colorByGradient)
                    color = Color.Grey(1f).WithAlpha(1f);
                //color = new Color((dir + new Vec2(.1f,.1f)).Up(0).Up(1));
                /*var traj = IFlowOperator<Vec2, Vec3>.Default.Compute(dat.SimulationTime, dat.SimulationTime + .05f, pos, dat.VelocityField);
                var sum =0.0;
                for (int i = 1; i < traj.Entries.Length; i++)
                {
                    var last = traj.Entries[i - 1];
                    var cur = traj.Entries[i];
                    sum += (cur.Down() - last.Down()).Length() / (cur.Last - last.Last);
                }

                var avgSpeed = traj.AverageAlong((prev, cur) => (cur.XY - prev.XY).Abs() / (cur.Last - prev.Last));

                color = new Color(0, 0, avgSpeed.LengthSquared(), 1);
                  color = dat.ColorGradient.Get(avgSpeed.LengthSquared());*/
                //dir = Vec2.Normalize(dir)/1.3f;
                var thick = Thickness;
                var bot = pos - dir * Length / 2 - dir * Length * .1f;
                var top = pos + dir * Length / 2 + dir * Length * .1f;
                var perpDir = new Vec2(dir.Y, -dir.X) * Length * .8f;
                var targetPos = perpDir / 2 + (top * .6f + bot * .4f);
                var targetPos2 = -perpDir / 2 + (top * .6f + bot * .4f);
                var offset = Vec2.Normalize(-(targetPos - top)) * thick / 2;
                if (maxDirLenght2 <= 0)
                {
                    Gizmos2D.Instanced.RegisterCircle(pos, Length / 10, color);

                }
                else
                {
                    Gizmos2D.Instanced.RegisterLine(bot, top, color, thick);
                    Gizmos2D.Instanced.RegisterLine(top + offset, targetPos, color, thick);
                    Gizmos2D.Instanced.RegisterLine(top, targetPos2, color, thick);
                }

            }
        }
        GL.BlendFuncSeparate(
            BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha,
            BlendingFactorSrc.One, BlendingFactorDest.One
        );
        Gizmos2D.Instanced.RenderCircles(view.Camera2D);
        Gizmos2D.Instanced.RenderRects(view.Camera2D);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        if (AutoResize)
        {
            Length = cellSize.X * 1;
            Thickness = cellSize.X / 10;
        }
    }


    public override void DrawImGuiDataSettings()
    {
        var dat = GetRequiredWorldService<DataService>();
        var bounds = (AltVectorfield ?? dat.VectorField).Domain.RectBoundary;
        ImGuiHelpers.OptionalDoubleSlider("Alt time", ref AltTime, bounds.Min.Last, bounds.Max.Last);
        ImGuiHelpers.OptonalVectorFieldSelector(World, ref AltVectorfield);
        ImGuiHelpers.OptionalGradientSelector(ref AltGradient);
        base.DrawImGuiDataSettings();
    }
    public string GetTitle()
    {
        return "Arrows (" + (AltVectorfield?.DisplayName ?? GetRequiredWorldService<DataService>().VectorField.DisplayName) + ")";
    }
}