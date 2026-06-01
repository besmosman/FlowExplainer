using ImGuiNET;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;


public class ArrowVisualizer : WorldService, IAxisTitle
{
    public override string? Name => "Arrow Glyphs";
    public override string? CategoryName => "Vectorfield";
    public override string? Description => "Visualize a vectorfield using arrow glyphs";

    [Input(Min = 0, Max = 1500)] public int GridCells = 250;
    [Input(Min = 0.0, Max = 1.0)] public double Length;
    [Input(Min = 0.0, Max = 1.0)] public double Thickness;
    [Input] public bool AutoResize = true;
    [Input] public bool colorByGradient = true;
    [Input] public Artifact<IVectorField<Vec2, Vec2>> Vectorfield;
    public ColorGradient? AltGradient;

    private int lastFieldHash;

    public override void Initialize()
    {
    }


    private Dictionary<Vec2, Vec2> cached = new();

    public Vec2 EvaluateCached(Vec2 pos)
    {
        if (!cached.TryGetValue(pos, out Vec2 v))
        {
            v = Vectorfield.Value.Evaluate(pos);
            cached[pos] = v;
        }

        return v;
    }

    public override void Draw(View view)
    {
        if (!view.Is2DCamera || Vectorfield == null)
            return;

        int fieldHash = Vectorfield.Value.GetHashCode();
        if (fieldHash != lastFieldHash)
        {
            cached.Clear();
            lastFieldHash = fieldHash;
        }
        
        var dat = GetRequiredWorldService<DataService>();
        var vectorfield = Vectorfield.Value;
        var gradient = AltGradient ?? dat.ColorGradient;

        var domain = vectorfield.Domain.RectBoundary;
        var domainSize = domain.Size;
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
                var pos = rel * domainSize + domain.Min;
                var dir = EvaluateCached(pos);
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
                var pos = rel * domainSize + domain.Min;
                var dir = EvaluateCached(domainBounding.Bound(pos));
                if (double.IsNaN(dir.X) || double.IsNaN(dir.Y))
                    continue;

                dir = double.Clamp(((dir.Length()) / (double.Sqrt(maxDirLenght2))), .0f, 1f) * Vec2.NormalizeSafe(dir);
                if (double.IsNaN(dir.X) || double.IsNaN(dir.Y))
                    continue;

                var color = gradient.Get(0);
                if (maxDirLenght2 != 0)
                    color = gradient.Get(dir.Length() * 1);

                if (!colorByGradient)
                    color = Color.Grey(1f).WithAlpha(1f);

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

        Gizmos2D.Instanced.RenderCircles(view.Camera2D);
        Gizmos2D.Instanced.RenderRects(view.Camera2D);

        if (AutoResize)
        {
            Length = cellSize.Y * .6;
            Thickness = cellSize.Y / 10;
        }
    }

    public override void DrawImGuiSettings()
    {
        ImGui.Checkbox("Color by gradient", ref colorByGradient);
        base.DrawImGuiSettings();
    }

    public string GetTitle()
    {
        return "Arrows (" + (Vectorfield?.DisplayName ?? GetRequiredWorldService<DataService>().VectorField.DisplayName) + ")";
    }
}