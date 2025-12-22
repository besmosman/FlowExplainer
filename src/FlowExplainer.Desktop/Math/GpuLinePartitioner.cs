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
        
        public float TimeAliveFactor;
        public float padding0;
        public float padding1;
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
            var minCell = WorldToCell(float.Min(l.StartX, l.EndX), float.Min(l.StartY, l.EndY));
            var maxCell = WorldToCell(float.Max(l.StartX, l.EndX), float.Max(l.StartY, l.EndY));
            for (int i = minCell.X; i <= maxCell.X; i++)
            for (int j = minCell.Y; j <= maxCell.Y; j++)
            {
                if (i >= 0 && j >= 0 && i < GridSize.X && j < GridSize.Y)
                    cells[GetCellIndex(i, j)].LinesCount++;
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
            var minCell = WorldToCell(float.Min(l.StartX, l.EndX), float.Min(l.StartY, l.EndY));
            var maxCell = WorldToCell(float.Max(l.StartX, l.EndX), float.Max(l.StartY, l.EndY));
            for (int i = minCell.X; i <= maxCell.X; i++)
            for (int j = minCell.Y; j <= maxCell.Y; j++)
            {
                if (i >= 0 && j >= 0 && i < GridSize.X && j < GridSize.Y)
                {
                    ref var cell = ref cells[GetCellIndex(i, j)];
                    linesOrganizedData[cell.LinesStartIndex + cell.LinesCount] = l;
                    cell.LinesCount++;
                }
            }
        }
        LinesOrganized.GetCurrentIndex() = totalOrganizedEntries;

        LinesUnorganized.Reset();
    }

    public int GetCellIndex(int x, int y)
    {
        return y * GridSize.X + x;
    }

    public Vec2i WorldToCell(float x, float y)
    {
        var i = (int)double.Floor((x - WorldViewRect.Min.X) / WorldViewRect.Size.X * GridSize.X);
        var j = (int)double.Floor((y - WorldViewRect.Min.Y) / WorldViewRect.Size.Y * GridSize.Y);
        return new Vec2i(i, j);
    }
}