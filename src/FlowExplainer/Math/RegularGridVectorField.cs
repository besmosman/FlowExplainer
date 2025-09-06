using System.Buffers;
using System.Collections.Concurrent;
using System.Numerics;
using MemoryPack;

namespace FlowExplainer;

public static class Rental<T>
{
    private static Dictionary<int, ConcurrentStack<T[]>> stacks = new();

    public static T[] Rent(int length)
    {
        if (!stacks.TryGetValue(length, out var stack))
        {
            stack = new();
            stacks[length] = stack;
        }

        if (stack.TryPop(out var ts))
            return ts;

        return new T[length];
    }

    public static void Return(T[] array)
    {
        stacks[array.Length].Push(array);
    }
}

/// <summary>
/// Arbitrary dimension grid based vector field with mutlivariate interpolator
/// </summary>
public class RegularGridVectorField<Vec, Veci, TData> : IVectorField<Vec, TData>
    where Vec : IVec<Vec>, IVecIntegerEquivelant<Veci>
    where Veci : IVec<Veci, int>
    where TData : IMultiplyOperators<TData, float, TData>, IAdditionOperators<TData, TData, TData>
{
    public RegularGrid<Veci, TData> Grid { get; private set; }
    public Veci GridSize => Grid.GridSize;

    public bool Interpolate = true;
    
    public RectDomain<Vec> RectDomain { get; set; }
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

    public TData Evaluate(Vec x)
    {
        x = ToVoxelCoord(x);

        if (!Interpolate)
            return Nearest(x);

        return MultivariateInterpolation(x);
    }

    public ref TData AtCoords(Veci v)
    {
        return ref Grid.AtCoords(v);
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
            voxelPos[i] = percentiel * GridSize[i];
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

    //modified from random online source. Tested for 2D and 3D cases.
    private TData MultivariateInterpolation(Vec x)
    {
        var dim = GridSize.ElementCount;
        var baseCoord = x.Floor();
        var weights = Vec.Zero;

        for (int i = 0; i < dim; i++)
            weights[i] = x[i] - baseCoord[i];

        TData result = default!;
        int numCorners = 1 << dim;
        for (int c = 0; c < numCorners; c++)
        {
            float weight = 1.0f;
            var corner = baseCoord;
            for (int i = 0; i < dim; i++)
            {
                int bit = (c >> i) & 1;
                int offset = bit;
                weight *= bit == 1 ? weights[i] : (1 - weights[i]);
                corner[i] = baseCoord[i] + offset;
            }

            var coordsIndex = Grid.GetCoordsIndex(corner);
            if (coordsIndex < 0 || coordsIndex >= Grid.Data.Length)
            {
                //if a neighbor does not exist we just return the nearest neighbor valur for now. 
                //TODO: make it weighted based on existing neighbors in these cases would be better.
                return Nearest(x);
            }

            var value = Grid.AtCoords(corner);
            result = result + (value * weight);
        }
        return result;
    }
    
    public void Resize(Veci gridSize, RectDomain<Vec> domain)
    {
        Grid.Resize(gridSize);
        RectDomain = domain;
    }
}