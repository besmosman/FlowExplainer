using System.Buffers;
using System.Numerics;

namespace FlowExplainer;

public enum BoundaryType
{
    None = 0,
    Periodic,
    Fixed,
    ReflectiveNeumann,
}

class BoundingNone<Vec> : IBounding<Vec> where Vec : IVec<Vec>
{
    public Vec Bound(Vec x)
    {
        return x;
    }
}

class BoundingPeriodicXyPeriodicZ : IBounding<Vec3>
{
    private readonly Rect<Vec3> Rect;

    public BoundingPeriodicXyPeriodicZ(Rect<Vec3> rect)
    {
        Rect = rect;
    }

    public Vec3 Bound(Vec3 x)
    {
        var r = x;
        r.X = (x.X - Rect.Min.X) % (Rect.Max.X - Rect.Min.X) + Rect.Min.X;
        r.Y = x.Z;
        r.Z = (x.Z - Rect.Min.Z) % (Rect.Max.Z - Rect.Min.Z) + Rect.Min.Z;
        return r;
    }
}

/// <summary>
/// Arbitrary dimension grid based vector field with mutlivariate interpolator
/// </summary>
public class RegularGridVectorField<Vec, Veci, TData> : IVectorField<Vec, TData>
    where Vec : IVec<Vec>, IVecIntegerEquivalent<Veci>
    where Veci : IVec<Veci, int>, IVecDoubleEquivalent<Vec>
    where TData : IMultiplyOperators<TData, double, TData>, IAdditionOperators<TData, TData, TData>
{
    public RegularGrid<Veci, TData> Grid { get; private set; }
    public Veci GridSize => Grid.GridSize;

    public bool Interpolate = true;

    public RectDomain<Vec> RectDomain { get; set; }

    public TData Evaluate(Vec x)
    {
        x = ToVoxelCoord(x);
        x = Utils.Clamp<Vec, double>(x, Vec.Zero, GridSize.ToVecF() - Vec.One);
        if (!Interpolate)
        {
            var coord = x.RoundInt();
            return Grid.AtCoords(coord);
        }

        if (!TryMultivariateInterpolation(x, out var value))
            throw new Exception();

        return value;
    }

    public Vec Wrap(Vec x)
    {
        return Bounding.Bound(x);
    }

    public bool TryEvaluate(Vec x, out TData value)
    {
        x = ToVoxelCoord(x);
        if (!Interpolate)
        {
            var coord = x.RoundInt();
            if (Grid.Contains(coord))
            {
                value = Grid.AtCoords(coord);
                return true;
            }
            value = default!;
            return false;
        }
        return TryMultivariateInterpolation(x, out value);
    }

    public IDomain<Vec> Domain => RectDomain;
    public IBounding<Vec> Bounding => genBounding;
    private GenBounding<Vec> genBounding;

    public RegularGridVectorField(TData[] data, Veci gridSize, Vec minCellPos, Vec maxCellPos, GenBounding<Vec>? boundary = null)
    {
        Grid = new RegularGrid<Veci, TData>(data, gridSize);
        genBounding = boundary ?? GenBounding<Vec>.None();
        RectDomain = new RectDomain<Vec>(minCellPos, maxCellPos, genBounding);
    }

    public RegularGridVectorField(Veci gridSize, Vec minCellPos, Vec maxCellPos, GenBounding<Vec>? boundary = null)
    {
        Grid = new RegularGrid<Veci, TData>(gridSize);
        genBounding = boundary ?? GenBounding<Vec>.None();
        RectDomain = new RectDomain<Vec>(minCellPos, maxCellPos, genBounding);

    }


    public RegularGridVectorField(Veci gridSize, RectDomain<Vec> rectDomain)
    {
        Grid = new RegularGrid<Veci, TData>(gridSize);
        RectDomain = rectDomain;
        genBounding = (GenBounding<Vec>)rectDomain.Bounding;

    }


    public ref TData AtCoords(Veci v)
    {
        return ref Grid.AtCoords(v);
    }

    public ref TData AtPos(Vec v)
    {
        return ref Grid.AtCoords(ToVoxelCoord(v).FloorInt());
    }

    public Vec ToVoxelCoord(Vec worldpos)
    {
        var voxelPos = default(Vec)!;
        for (int i = 0; i < GridSize.ElementCount; i++)
        {
            var max = RectDomain.MaxPos[i];
            var min = RectDomain.MinPos[i];
            var wpos = worldpos[i];
            var percentiel = (wpos - min) / (max - min);
            voxelPos[i] = percentiel * (GridSize[i] - 1);
        }

        return voxelPos;
    }

    public Vec ToWorldPos(Vec coords)
    {
        var worldPos = default(Vec)!;
        for (int i = 0; i < GridSize.ElementCount; i++)
        {
            var max = RectDomain.MaxPos[i];
            var min = RectDomain.MinPos[i];
            var voxelCoord = coords[i];
            var percentile = voxelCoord / (GridSize[i] - 0);
            worldPos[i] = min + percentile * (max - min);
        }

        return worldPos;
    }

    public Vec ToWorldPos(Veci coords)
    {
        return ToWorldPos(coords.ToVecF());
    }

    public static RegularGridVectorField<Vec, Veci, TData> Load(string path)
    {
        var save = BinarySerializer.Load<RegularGridVectorFieldSave<Vec, Veci, TData>>(path);
        return new RegularGridVectorField<Vec, Veci, TData>(save.Data, save.GridSize, save.Min, save.Max, save.Boundings);
    }

    public void Save(string path)
    {
        var vectorFieldSave = new RegularGridVectorFieldSave<Vec, Veci, TData>()
        {
            Data = Grid.Data,
            GridSize = GridSize,
            Min = RectDomain.MinPos,
            Max = RectDomain.MaxPos,
            Boundings = genBounding,
        };

        BinarySerializer.Save(path, vectorFieldSave);
    }

    private TData Nearest(Vec x)
    {
        return Grid.AtCoords(x.RoundInt());
    }

    public RegularGridVectorField<Vec, Veci, TOut2> Select<TOut2>(Func<Veci, TOut2> selector)
        where TOut2 : IMultiplyOperators<TOut2, double, TOut2>, IAdditionOperators<TOut2, TOut2, TOut2>
    {
        TOut2[] data = new TOut2[Grid.Data.Length];
        for (int i = 0; i < Grid.Data.Length; i++)
        {
            var coords = Grid.GetIndexCoords(i);
            data[i] = selector(coords);
        }

        return new RegularGridVectorField<Vec, Veci, TOut2>(data, GridSize, RectDomain.MinPos, RectDomain.MaxPos, genBounding);
    }

    //modified from random online source. Tested for 2D and 3D cases, should work in any dimension.
    private bool TryMultivariateInterpolation(Vec coords, out TData result)
    {
        int dim = GridSize.ElementCount;
        var baseCoord = coords.FloorInt();

        var weights = Vec.Zero;
        for (int i = 0; i < dim; i++)
            weights[i] = coords[i] - baseCoord[i];

        if (baseCoord.Last == GridSize.Last)
            baseCoord[coords.ElementCount - 1] -= 1;

        int numCorners = 1 << dim;
        double totalWeight = 0.0f;
        result = default!;

        for (int c = 0; c < numCorners; c++)
        {
            double weight = 1.0f;
            var corner = baseCoord;

            for (int i = 0; i < dim; i++)
            {
                int bit = (c >> i) & 1;
                weight *= bit == 1 ? weights[i] : (1 - weights[i]);
                corner[i] = baseCoord[i] + bit;
            }

            if (Grid.Contains(corner))
            {
                totalWeight += weight;
                var value = Grid.AtCoords(corner);
                result += value * weight;
            }
        }

        if (totalWeight == 0.0f)
            return false;

        result *= 1f / totalWeight;
        return true;
    }

    public void Resize(Veci gridSize, RectDomain<Vec> domain)
    {
        Grid.Resize(gridSize);
        RectDomain = domain;
    }
}