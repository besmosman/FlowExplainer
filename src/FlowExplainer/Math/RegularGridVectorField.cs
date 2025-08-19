using System.Buffers;
using System.Numerics;
using MemoryPack;

namespace FlowExplainer;

/// <summary>
/// Arbitrary dimension grid based vector field with mutlivariate interpolator
/// </summary>
public class RegularGridVectorField<Vec, Veci, TData> : IVectorField<Vec, TData>
    where Vec : IVec<Vec>, IVecIntegerEquivelant<Veci>
    where Veci : IVec<Veci, int>
    where TData : IMultiplyOperators<TData, float, TData>, IAdditionOperators<TData, TData, TData>
{
    public RegularGrid<Veci, TData> Data { get; private set; }
    public Veci GridSize => Data.GridSize;

    public bool Interpolate = true;

    public Vec MinCellPos { get; set; }
    public Vec MaxCellPos { get; set; }

    public IDomain<Vec> Domain => new RectDomain<Vec>(MinCellPos, MaxCellPos);

    
    public RegularGridVectorField(TData[] data, Veci gridSize, Vec minCellPos, Vec maxCellPos)
    {
        Data = new RegularGrid<Veci, TData>(data, gridSize);
        MinCellPos = minCellPos;
        MaxCellPos = maxCellPos;
    }

    public RegularGridVectorField(Veci gridSize, Vec minCellPos, Vec maxCellPos)
    {
        Data = new RegularGrid<Veci, TData>(gridSize);
        MinCellPos = minCellPos;
        MaxCellPos = maxCellPos;
    }

    public TData Evaluate(Vec x)
    {
        x = ToVoxelCoord(x);

        if (!Interpolate)
            return Nearest(x);

        return MultivariateInterpolation(x);
    }

    public Vec ToVoxelCoord(Vec worldpos)
    {
        var voxelPos = default(Vec)!;
        for (int i = 0; i < GridSize.ElementCount; i++)
        {
            var max = MaxCellPos[i];
            var min = MinCellPos[i];
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
            var max = MaxCellPos[i];
            var min = MinCellPos[i];
            var voxelCoord = coords[i];
            var percentile = voxelCoord / (GridSize[i] - 0);
            worldPos[i] = min + percentile * (max - min);
        }

        return worldPos;
    }


    private TData Nearest(Vec x)
    {
        return Data.AtCoords(x.Round());
    }

    public RegularGridVectorField<Vec, Veci, TOut2> Select<TOut2>(Func<Veci, TOut2> selector)
        where TOut2 : IMultiplyOperators<TOut2, float, TOut2>, IAdditionOperators<TOut2, TOut2, TOut2>
    {
        TOut2[] data = new TOut2[Data.Data.Length];
        for (int i = 0; i < Data.Data.Length; i++)
        {
            var coords = Data.GetIndexCoords(i);
            data[i] = selector(coords);
        }
        return new RegularGridVectorField<Vec, Veci, TOut2>(data, GridSize, MinCellPos, MaxCellPos);
    }

    //modified from random online source. Tested for 2D and 3D cases.
    private TData MultivariateInterpolation(Vec x)
    {
        var dim = GridSize.ElementCount;
        var baseCoord = x.Floor();
        var weights = ArrayPool<float>.Shared.Rent(dim);
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

            var coordsIndex = Data.GetCoordsIndex(corner);
            if (coordsIndex < 0 || coordsIndex >= Data.Data.Length)
            {
                //if a neighbor does not exist we just return the nearest neighbor valur for now. 
                //TODO: make it weighted based on existing neighbors in these cases would be better.
                return Nearest(x);
            }

            var value = Data.AtCoords(corner);
            result = result + (value * weight);
        }

        ArrayPool<float>.Shared.Return(weights);
        return result;
    }
}