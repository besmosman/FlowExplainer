using System.Runtime.InteropServices;

namespace FlowExplainer;

public class GpuLinePartitioner
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Line
    {
        public float StartX;
        public float StartY;
        public float EndX;
        public float EndY;

        public int ParticleId;
        public float StartTimeAliveFactor;
        public float EndTimeAliveFactor;
        public float padding2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Cell
    {
        public int LinesStartIndex;
        public int LinesCount;
        public float padding0;
        public float padding1;
    }

    public AutoExpandStorageBuffer<Line> LinesUnorganized = new();
    public AutoExpandStorageBuffer<Line> LinesOrganized = new();
    public AutoExpandStorageBuffer<Cell> Cells = new();

    public void RegisterLine(Line line)
    {
        LinesUnorganized.Register(line);
    }

    public Vec2i GridSize;
    public Rect<Vec2> WorldViewRect;

    public void Organize()
    {
        var lines = LinesUnorganized.buffer.Data;

        if (Cells.buffer.Length != GridSize.Volume())
            Cells.buffer.Resize(GridSize.Volume());

        Array.Clear(Cells.buffer.Data);
        var cells = Cells.buffer.Data;
        foreach (ref var l in lines.AsSpan(0, LinesUnorganized.GetCurrentIndex()))
        {
            var startPos = WorldToCellSpace(l.StartX, l.StartY);
            var endPos = WorldToCellSpace(l.EndX, l.EndY);
            foreach (var coord in RasterizeLine(startPos,endPos))
            {
                if (coord.X >= 0 && coord.Y >= 0 && coord.X < GridSize.X && coord.Y < GridSize.Y)
                    cells[GetCellIndex(coord.X, coord.Y)].LinesCount++;
            }
        }
        var totalOrganizedEntries = 0;
        foreach (ref var c in cells.AsSpan())
        {
            c.LinesStartIndex = totalOrganizedEntries;
            totalOrganizedEntries += c.LinesCount;
            c.LinesCount = 0;
        }
        if (LinesOrganized.buffer.Length < totalOrganizedEntries)
            LinesOrganized.buffer.Resize(totalOrganizedEntries);

        LinesOrganized.Reset();
        var linesOrganizedData = LinesOrganized.buffer.Data;
        foreach (ref var l in lines.AsSpan(0, LinesUnorganized.GetCurrentIndex()))
        {
            var startPos = WorldToCellSpace(l.StartX, l.StartY);
            var endPos = WorldToCellSpace(l.EndX, l.EndY);
            foreach (var coord in RasterizeLine(startPos,endPos))
            {
                if (coord.X >= 0 && coord.Y >= 0 && coord.X < GridSize.X && coord.Y < GridSize.Y)
                {
                    ref var cell = ref cells[GetCellIndex(coord.X, coord.Y)];
                    linesOrganizedData[cell.LinesStartIndex + cell.LinesCount] = l;
                    cell.LinesCount++;
                }
            }
        }
        LinesOrganized.GetCurrentIndex() = totalOrganizedEntries;

        LinesUnorganized.Reset();
    }

    //gpt
    public static IEnumerable<Vec2i> RasterizeLine(
        Vec2 p0,
        Vec2 p1)
    {
        int x0 = (int)Math.Floor(p0.X);
        int y0 = (int)Math.Floor(p0.Y);
        int x1 = (int)Math.Floor(p1.X);
        int y1 = (int)Math.Floor(p1.Y);

        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);

        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;

        int err = dx - dy;

        while (true)
        {
            yield return new Vec2i(x0, y0);

            if (x0 == x1 && y0 == y1)
                break;

            int e2 = err << 1;

            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    public int GetCellIndex(int x, int y)
    {
        return y * GridSize.X + x;
    }

    public Vec2i WorldToCell(float x, float y)
    {
        var gridSize = WorldToCellSpace(x, y);
        var i = (int)double.Floor(gridSize.X);
        var j = (int)double.Floor(gridSize.Y);
        return new Vec2i(i, j);
    }

    private Vec2 WorldToCellSpace(float x, float y)
    {
        double gridSizeX = (x - WorldViewRect.Min.X) / WorldViewRect.Size.X * GridSize.X;
        double gridSizeY = (y - WorldViewRect.Min.Y) / WorldViewRect.Size.Y * GridSize.Y;
        return new Vec2(gridSizeX, gridSizeY);
    }
}