using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace FlowExplainer;

public class PerronFrobeniusOperatorUlamsMethod
{
    public struct Particle
    {
        public Vec2 Start;
        public Vec2i startCell;
        public Vec2 End;
        public Vec2i endCell;
    }

    public SparseMatrix TransitionMatrix;
    public PointSpatialPartitioner2D<Vec2, Vec2i, Particle>? partitionerStartPoint;
    public PointSpatialPartitioner2D<Vec2, Vec2i, Particle>? partitionerEndPoints;
    private Dictionary<Vec2i, int> CellToMatrixIndex;
    public void Compute(IVectorField<Vec3, Vec2> vectorField)
    {
        var bounds = vectorField.Domain.RectBoundary;
        Particle[] particles = new Particle[600000];
        var t_start = 0;
        var t_end = 1f;

        var cellSize = bounds.Size.X / 96;

        var flowOperator = IFlowOperator<Vec2, Vec3>.Default;
        Parallel.For(0, particles.Length, i =>
        {
            ref var p = ref particles[i];
            p.Start = Utils.Random(bounds).XY;
            p.End = flowOperator.ComputeEnd(t_start, t_end, p.Start, vectorField);
        });

        partitionerStartPoint = new(cellSize);
        partitionerStartPoint.Init(particles, (ps, i) => ps[i].Start);
        partitionerStartPoint.UpdateEntries();
        Dictionary<(Vec2i startCell, Vec2i endCell), int> transitions = new();
        CellToMatrixIndex = new Dictionary<Vec2i, int>();
        foreach (var p in partitionerStartPoint.Data)
        {
            foreach (int i in p.Value!)
                particles[i].startCell = p.Key;
        }
        partitionerEndPoints = new(cellSize);
        partitionerEndPoints.Init(particles, (ps, i) => ps[i].End);
        partitionerEndPoints.UpdateEntries();
        foreach (var p in partitionerEndPoints.Data)
        {
            foreach (int i in p.Value!)
                particles[i].endCell = p.Key;
        }

        foreach (var p in particles)
        {
            var key = (p.startCell, p.endCell);
            transitions.TryAdd(key, 0);
            transitions[key]++;
        }

        int GetMatrixCellIndex(Vec2i c)
        {
            if (!CellToMatrixIndex.TryGetValue(c, out int value))
            {
                value = CellToMatrixIndex.Count;
                CellToMatrixIndex.Add(c, value);
            }
            return value;
        }

        foreach (var t in partitionerStartPoint.Data.Keys)
            GetMatrixCellIndex(t);
        foreach (var t in partitionerEndPoints.Data.Keys)
            GetMatrixCellIndex(t);

        int minX = int.Min(partitionerStartPoint.Data.Min(m => m.Key.X), partitionerEndPoints.Data.Min(m => m.Key.X));
        int minY = int.Min(partitionerStartPoint.Data.Min(m => m.Key.Y), partitionerEndPoints.Data.Min(m => m.Key.Y));
        int maxX = int.Max(partitionerStartPoint.Data.Max(m => m.Key.X), partitionerEndPoints.Data.Max(m => m.Key.X));
        int maxY = int.Max(partitionerStartPoint.Data.Max(m => m.Key.Y), partitionerEndPoints.Data.Max(m => m.Key.Y));
        for (int x = minX; x <= maxX; x++)
        for (int y = minY; y <= maxY; y++)
        {
            //GetMatrixCellIndex(new Vec2i(x, y));
        }
        TransitionMatrix = SparseMatrix.Create(CellToMatrixIndex.Count + 1, CellToMatrixIndex.Count + 1, 0);

        foreach (var t in transitions)
        {
            var row = GetMatrixCellIndex(t.Key.startCell);
            var column = GetMatrixCellIndex(t.Key.endCell);
            if (row < TransitionMatrix.RowCount && column < TransitionMatrix.ColumnCount)
                TransitionMatrix[row, column] = t.Value / (double)partitionerStartPoint.Data[t.Key.startCell].Count;
        }

        int c = 4;
    }

    public Vec2 w;
    public double GetTransitionValueAt(Vec2 worldpos)
    {
        if (partitionerStartPoint == null)
            return 0;
        int from = WorldToMatrixIndex(worldpos);
        CellToMatrixIndex.TryGetValue(partitionerStartPoint.GetVoxelCoords(w), out int to);
        return TransitionMatrix[from, to];
    }
    public int WorldToMatrixIndex(Vec2 worldpos)
    {

        var partionerCell = partitionerStartPoint.GetVoxelCoords(worldpos);
        CellToMatrixIndex.TryGetValue(partionerCell, out int from);
        return from;
    }
}