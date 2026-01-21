using Microsoft.VisualBasic;

namespace FlowExplainer;

public class PointSpatialPartitioner2D<Vec, Veci, T>
    where Vec : IVec<Vec, double>, IVecIntegerEquivalent<Veci>
    where Veci : IVec<Veci, int>
{
    public Dictionary<Veci, List<int>?> Data = new();
    private Dictionary<Veci, List<int>?> TempData = new();
    public Rect<Vec> Bounds;
    public double CellSize;

    private T[] Entries;
    private Func<T[], int, Vec> GetPos;

    public bool IsUpdating;
    public Func<Vec, Vec, double> DistanceSqrtFunc = Utils.DistanceSquared;

    public PointSpatialPartitioner2D(double cellSize)
    {
        CellSize = cellSize;

    }

    public void Init(T[] entries, Func<T[], int, Vec> getPos)
    {
        Entries = entries;
        GetPos = getPos;
    }



    public void UpdateEntries()
    {
        IsUpdating = true;
        foreach (var l in TempData.Values)
            l?.Clear();

        for (int i = 0; i < Entries.Length; i++)
        {
            var pos = GetPos(Entries, i);
            var cell = GetVoxelCoords(pos);
            if (!TempData.TryGetValue(cell, out var list))
            {
                if (list == null)
                {
                    list = new();
                    TempData.Add(cell, list);
                }
            }
            list!.Add(i);
        }
        (TempData, Data) = (Data, TempData);
        IsUpdating = false;
    }

    public Veci GetVoxelCoords(Vec pos)
    {
        return (pos / CellSize).FloorInt();
    }

    public IEnumerable<int> GetWithinRadius(Vec p, double radius)
    {
        var r2 = radius * radius;
        var minCell = GetVoxelCoords(p - (Vec.One * radius)) - Veci.One;
        var maxCel = GetVoxelCoords(p + (Vec.One * radius)) + Veci.One;

        foreach (var coord in Iterate(minCell, maxCel))
        {
            if (Data.TryGetValue(coord, out var list))
                foreach (int e in list!)
                {
                    if (DistanceSqrtFunc(GetPos(Entries, e), p) < r2)
                        yield return e;
                }
        }
    }


    public IEnumerable<int> GetWithinRadiusPeriodicX(Vec p, double radius, double DomainSizeX)
    {
        var r2 = radius * radius;
        var minCell = GetVoxelCoords(p - (Vec.One * radius)) - Veci.One;
        var maxCel = GetVoxelCoords(p + (Vec.One * radius)) + Veci.One;

        int gridSizeX = (int)(DomainSizeX / CellSize);
        foreach (var coord in Iterate(minCell, maxCel))
        {
            if (coord[0] < 0)
            {
                coord[0] += gridSizeX;
            }

            if (coord[0] > gridSizeX)
            {
                coord[0] -= gridSizeX;
            }

            if (Data.TryGetValue(coord, out var list))
                foreach (int e in list!)
                {
                    if (DistanceSqrtFunc(GetPos(Entries, e), p) < r2)
                        yield return e;
                }
        }
    }

    public void AddWithinRadius(Vec p, double radius, List<int> toFill)
    {
        var r2 = radius * radius;
        var minCell = GetVoxelCoords(p - (Vec.One * radius)) - Veci.One;
        var maxCel = GetVoxelCoords(p + (Vec.One * radius)) + Veci.One;

        foreach (var coord in Iterate(minCell, maxCel))
        {
            if (Data.TryGetValue(coord, out var list))
                foreach (int e in list!)
                    if (DistanceSqrtFunc(GetPos(Entries, e), p) < r2)
                        toFill.Add(e);
        }
    }


    private IEnumerable<Veci> Iterate(Veci start, Veci end)
    {
        var dims = (end - start);
        var c = dims.Volume();
        for (int i = 0; i < c; i++)
        {
            int remainder = i;
            Veci cell = start;
            for (int dim = dims.ElementCount - 1; dim >= 0; dim--)
            {
                cell[dim] += remainder % dims[dim];
                remainder /= dims[dim];
            }
            yield return cell;
        }
    }
}