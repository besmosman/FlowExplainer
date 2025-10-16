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
    public BoundaryType[] Boundaries;
}

public enum BoundaryType
{
    None = 0,
    Periodic,
    Fixed,
    ReflectiveNeumann,
}

public interface IBoundary<Vec>
{
    Vec Wrap(Vec x);
}

public static class Boundaries
{
    public static IBoundary<Vec3> PeriodicXPeriodicZ(Rect<Vec3> rect)
    {
        return new BoundaryPeriodicXYPeriodicZ(rect);
    }
    public static IBoundary<Vec> None<Vec>()
    {
        return new BoundaryNone<Vec>();
    }

    public static IBoundary<Vec> Build<Vec>(BoundaryType[] boundaries, Rect<Vec> rect) where Vec : IVec<Vec>
    {
        return new GenBoundary<Vec>(boundaries, rect);
    }
}

class BoundaryNone<Vec> : IBoundary<Vec>
{
    public Vec Wrap(Vec x)
    {
        return x;
    }
}

class BoundaryPeriodicXYPeriodicZ : IBoundary<Vec3>
{
    private readonly Rect<Vec3> Rect;

    public BoundaryPeriodicXYPeriodicZ(Rect<Vec3> rect)
    {
        Rect = rect;
    }

    public Vec3 Wrap(Vec3 x)
    {
        var r = x;
        r.X = (x.X - Rect.Min.X) % (Rect.Max.X - Rect.Min.X) + Rect.Min.X;
        r.Y = x.Z;
        r.Z = (x.Z - Rect.Min.Z) % (Rect.Max.Z - Rect.Min.Z) + Rect.Min.Z;
        return r;
    }
}

public class GenBoundary<Vec> : IBoundary<Vec> where Vec : IVec<Vec>
{
    public BoundaryType[] Boundaries;
    private Func<Rect<Vec>, int, float, float>[] wraps;
    private Rect<Vec> Rect;

    public GenBoundary(BoundaryType[] boundaries, Rect<Vec> rect)
    {
        Boundaries = boundaries;
        wraps = new Func<Rect<Vec>, int, float, float>[Vec.One.ElementCount];
        for (int i = 0; i < boundaries.Length; i++)
        {
            switch (boundaries[i])
            {
                case BoundaryType.None:
                    wraps[i] = static (_, _, x) => x;
                    break;
                case BoundaryType.Periodic:
                    wraps[i] = static (r, i, x) =>
                    {
                        var t = (x - r.Min[i]) % r.Size[i];
                        if (t < 0) t += r.Size[i];
                        return t + r.Min[i];
                    };
                    break;
                case BoundaryType.Fixed:
                    wraps[i] = static (r, i, x) => float.Clamp(x, r.Min[i], r.Max[i]);
                    break;
                case BoundaryType.ReflectiveNeumann:
                    wraps[i] = static (r, i, x) =>
                    {
                        if (x < r.Min[i])
                            return r.Min[i] - x;
                        if (x > r.Max[i])
                            return r.Max[i] - x + r.Max[i];
                        return x;
                    };
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        Rect = rect;
    }

    public Vec Wrap(Vec x)
    {
        for (int i = 0; i < x.ElementCount; i++)
            x[i] = wraps[i](Rect, i, x[i]);
        return x;
    }
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

    public Vec Wrap(Vec x)
    {
        return Boundary.Wrap(x);
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
    public IBoundary<Vec> Boundary { get; set; }

    public RegularGridVectorField(TData[] data, Veci gridSize, Vec minCellPos, Vec maxCellPos, IBoundary<Vec>? boundary = null)
    {
        Grid = new RegularGrid<Veci, TData>(data, gridSize);
        RectDomain = new RectDomain<Vec>(minCellPos, maxCellPos);
        boundary ??= Boundaries.None<Vec>();
        Boundary = boundary;    }

    public RegularGridVectorField(Veci gridSize, Vec minCellPos, Vec maxCellPos, IBoundary<Vec>? boundary = null)
    {
        Grid = new RegularGrid<Veci, TData>(gridSize);
        RectDomain = new RectDomain<Vec>(minCellPos, maxCellPos);
        boundary ??= Boundaries.None<Vec>();
        Boundary = boundary;
    }


    public RegularGridVectorField(Veci gridSize, RectDomain<Vec> rectDomain, IBoundary<Vec>? boundary = null)
    {
        Grid = new RegularGrid<Veci, TData>(gridSize);
        RectDomain = rectDomain;
        if (boundary == null)
            boundary = Boundaries.Build(new BoundaryType[gridSize.ElementCount], rectDomain.Rect);
        Boundary = boundary;
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
        var boundary = new GenBoundary<Vec>(save.Boundaries, new Rect<Vec>(save.Min, save.Max));
        return new RegularGridVectorField<Vec, Veci, TData>(save.Data, save.GridSize, save.Min, save.Max, boundary);
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

        return new RegularGridVectorField<Vec, Veci, TOut2>(data, GridSize, RectDomain.MinPos, RectDomain.MaxPos, Boundary);
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