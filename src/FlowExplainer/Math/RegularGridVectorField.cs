using System.Buffers;
using System.Numerics;
using MemoryPack;

namespace FlowExplainer;

[MemoryPackable]
public partial struct RegularGridVectorFieldSave<Vec, Veci, TData>
{
    public TData[] Data;
    public Veci GridSize;
    public Vec Min;
    public Vec Max;
}

/// <summary>
/// Arbitrary dimension grid based vector field with mutlivariate interpolator
/// </summary>
public class RegularGridVectorField<Vec, Veci, TData> : IVectorField<Vec, TData>
    where Vec : IVec<Vec>, IVecIntegerEquivelant<Veci>
    where Veci : IVec<Veci, int>, IVecFloatEquivelant<Vec>
    where TData : IMultiplyOperators<TData, float, TData>, IAdditionOperators<TData, TData, TData>
{
    public RegularGrid<Veci, TData> Grid { get; private set; }
    public Veci GridSize => Grid.GridSize;

    public bool Interpolate = true;

    public RectDomain<Vec> RectDomain { get; set; }

    public TData Evaluate(Vec x)
    {
        x = ToVoxelCoord(x);
        x = Utils.Clamp<Vec, float>(x, Vec.Zero, GridSize.ToVecF() - Vec.One);
        if (!Interpolate)
        {
            var coord = x.Round();
            return Grid.AtCoords(coord);
        }

        if (!TryMultivariateInterpolation(x, out var value))
            throw new Exception();

        return value;
    }

    public bool TryEvaluate(Vec x, out TData value)
    {
        x = ToVoxelCoord(x);
        if (!Interpolate)
        {
            var coord = x.Round();
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

    public RegularGridVectorField(TData[] data, Veci gridSize, Vec minCellPos, Vec maxCellPos)
    {
        Grid = new RegularGrid<Veci, TData>(data, gridSize);
        RectDomain = new RectDomain<Vec>(minCellPos, maxCellPos);
    }

    public RegularGridVectorField(Veci gridSize, Vec minCellPos, Vec maxCellPos)
    {
        Grid = new RegularGrid<Veci, TData>(gridSize);
        RectDomain = new RectDomain<Vec>(minCellPos, maxCellPos);
    }

    public RegularGridVectorField(RegularGrid<Veci, TData> grid, RectDomain<Vec> rectDomain)
    {
        Grid = grid;
        RectDomain = rectDomain;
    }

    public RegularGridVectorField(Veci gridSize, RectDomain<Vec> rectDomain)
    {
        Grid = new RegularGrid<Veci, TData>(gridSize);
        RectDomain = rectDomain;
    }


    public ref TData AtCoords(Veci v)
    {
        return ref Grid.AtCoords(v);
    }

    public ref TData AtPos(Vec v)
    {
        return ref Grid.AtCoords(ToVoxelCoord(v).Floor());
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
        return new RegularGridVectorField<Vec, Veci, TData>(save.Data, save.GridSize, save.Min, save.Max);
    }

    public void Save(string path)
    {
        var vectorFieldSave = new RegularGridVectorFieldSave<Vec, Veci, TData>()
        {
            Data = Grid.Data,
            GridSize = GridSize,
            Min = RectDomain.MinPos,
            Max = RectDomain.MaxPos,
        };

        BinarySerializer.Save(path, vectorFieldSave);
    }

    private TData Nearest(Vec x)
    {
        return Grid.AtCoords(x.Round());
    }

    public RegularGridVectorField<Vec, Veci, TOut2> Select<TOut2>(Func<Veci, TOut2> selector)
        where TOut2 : IMultiplyOperators<TOut2, float, TOut2>, IAdditionOperators<TOut2, TOut2, TOut2>
    {
        TOut2[] data = new TOut2[Grid.Data.Length];
        for (int i = 0; i < Grid.Data.Length; i++)
        {
            var coords = Grid.GetIndexCoords(i);
            data[i] = selector(coords);
        }

        return new RegularGridVectorField<Vec, Veci, TOut2>(data, GridSize, RectDomain.MinPos, RectDomain.MaxPos);
    }

    //modified from random online source. Tested for 2D and 3D cases, should work in any dimension.
    private bool TryMultivariateInterpolation(Vec coords, out TData result)
    {
        int dim = GridSize.ElementCount;
        var baseCoord = coords.Floor();

        var weights = Vec.Zero;
        for (int i = 0; i < dim; i++)
            weights[i] = coords[i] - baseCoord[i];

        if (baseCoord.Last == GridSize.Last)
            baseCoord[coords.ElementCount - 1] -= 1;

        int numCorners = 1 << dim;
        float totalWeight = 0.0f;
        result = default!;

        for (int c = 0; c < numCorners; c++)
        {
            float weight = 1.0f;
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