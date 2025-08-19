using Newtonsoft.Json.Linq;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FlowExplainer;

public class StructureIdentifier : WorldService
{
    public override ToolCategory Category => ToolCategory.Flow;

    private Structure structure;

    public class Structure
    {
        public Vec2i SeedCoords;
        public HashSet<Vec2i> lastFilled;
        public float dis;
    }

    public override void Initialize()
    {

    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        
        var window = GetRequiredGlobalService<WindowService>();
        var grid = GetRequiredWorldService<GridVisualizer>();
        if (window.Window.IsMouseButtonPressed(MouseButton.Left) && view.IsSelected)
        {
            grid.MarkDirty = true;
            structure = new Structure
            {
                SeedCoords = grid.RegularGrid.ToVoxelCoord(CoordinatesConverter2D.ViewToWorld(view, view.RelativeMousePosition)).Floor(),
                lastFilled = null,
                dis = radius,
            };
        }

        if (structure != null)
        {
            if (structure.lastFilled != null)
            {
                int samples = 64;
                var bestScore = 0f;
                var bestdis = 0f;
                var bestOffset = new Vec2i(0, 0);
                for (int i = 0; i < samples; i++)
                {
                    var offset = new Vec2i(Random.Shared.Next(-1, 2), Random.Shared.Next(-1, 2));
                    var val = structure.dis + (Random.Shared.NextSingle() -.5f)*.1f;

                    if (i == 0)
                    {
                        offset = new Vec2i(0, 0);
                        val = structure.dis;
                    }

                    var sampleStructure = FindStructure(grid, structure.SeedCoords + offset, val);
                    var union = new List<Vec2i>();
                    union.AddRange(sampleStructure);
                    union.AddRange(structure.lastFilled);
                    union = union.Distinct().ToList();
                    var overlapping = new List<Vec2i>();
                    
                    
                    foreach (var c in sampleStructure)
                    {
                        if (structure.lastFilled.Contains(c))
                        {
                            overlapping.Add(c);
                        }
                    }
                    var totScore = (float)overlapping.Count / union.Count;
                    if (totScore > bestScore)
                    {
                        bestOffset = offset;
                        bestScore = totScore;
                        bestdis = val;
                    }
                }
                structure.SeedCoords += bestOffset;
                structure.dis = float.Lerp(structure.dis, bestdis, .4f);
            }
            var filled = FindStructure(grid, structure.SeedCoords, structure.dis);

            for (int i = 0; i < grid.RegularGrid.Data.Data.Length; i++)
                grid.RegularGrid.Data.Data[i].Marked = 0;

            foreach (var c in filled)
            {
                grid.RegularGrid.Data.AtCoords(c).Marked = 1;
                // if (filled.Contains(c - new Vec2i(0, 1)) && filled.Contains(c - new Vec2i(0, -1)) && filled.Contains(c - new Vec2i(1, 0)) && filled.Contains(c - new Vec2i(-1, 0)))
                //     continue;
                /*var start = grid.RegularGrid.ToWorldPos(c.ToVec2());
                var end = grid.RegularGrid.ToWorldPos(c.ToVec2() + new Vec2(1, 1));
                Gizmos2D.Rect(view.Camera2D, start, end, Vec4.One);*/
            }

            structure.lastFilled = filled;

            Gizmos2D.Circle(view.Camera2D, grid.RegularGrid.ToWorldPos(structure.SeedCoords.ToVec2() + Vec2.One / 2), new Color(0, 1, 0), .01f);

            /*var newBounds = Utils.GetBounds(filled.Select(s => grid.RegularGrid.ToWorldPos(s.ToVec2())));
            var lastBounds = structure.lastBounds;
            if (lastBounds.HasValue)
            {
                var movement = newBounds.Center - lastBounds.Value.Center;
                structure.Seed += movement / 10f;

            }
            structure.lastBounds = newBounds;*/
        }
    }
    private static HashSet<Vec2i> FindStructure(GridVisualizer grid, Vec2i coord, float dis)
    {

        var centerVal = grid.RegularGrid.Data.AtCoords(coord);
        HashSet<Vec2i> filled = new();
        Queue<Vec2i> toCheck = new();
        toCheck.Enqueue(coord);
        for (int i = 0; i < 1; i++)
        {
            while (toCheck.TryDequeue(out var c))
            {
                if (!filled.Add(c))
                    continue;

                if (!grid.RegularGrid.Data.IsWithin(c))
                    continue;

                if (float.Abs(grid.RegularGrid.Data.AtCoords(c).Value - centerVal.Value) < dis)
                {
                    if (!toCheck.Contains(c + new Vec2i(1, 0))) toCheck.Enqueue(c + new Vec2i(1, 0));
                    if (!toCheck.Contains(c + new Vec2i(-1, 0))) toCheck.Enqueue(c + new Vec2i(-1, 0));
                    if (!toCheck.Contains(c + new Vec2i(0, 1))) toCheck.Enqueue(c + new Vec2i(0, 1));
                    if (!toCheck.Contains(c + new Vec2i(0, -1))) toCheck.Enqueue(c + new Vec2i(0, -1));
                }
            }
        }
        return filled;
    }
    private float radius = .1f;
    public override void DrawImGuiEdit()
    {
        ImGuiHelpers.SliderFloat("Identify radius", ref radius, 0, .3f);
        base.DrawImGuiEdit();
    }
}