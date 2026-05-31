using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace FlowExplainer;


/*
public class OrientedDiscretizedField2D : IVectorField<Vec2, Vec2>
{
    public DiscretizedField<Vec2, Vec2i,Vec2> Grid { get; private set; }

    
    
    public Vec2 Evaluate(Vec2 x)
    {
        x = Grid.GridField.ToVoxelCoord(x);
        x = Utils.Clamp<Vec2, double>(x, Vec2.Zero, Grid.GridField.GridSize.ToVecF() - Vec2.One);
        var grid = Grid.GridField.Grid;
        grid.AtCoords(Grid.Evaluate())
    }

    public bool TryEvaluate(Vec2 x, [MaybeNullWhen(false)] out Vec2 value)
    {
        
    }

    public IDomain<Vec> Domain { get; }
}*/

public class DiscretizedField<Vec, Veci, TData> : IVectorField<Vec, TData>
    where Vec : IVec<Vec, double>, IVecIntegerEquivalent<Veci>
    where Veci : IVec<Veci, int>, IVecDoubleEquivalent<Vec>
    where TData : IMultiplyOperators<TData, double, TData>, IAdditionOperators<TData, TData, TData>
{
    public RegularGridVectorField<Vec, Veci, TData> GridField { get; }
    public IDomain<Vec> Domain => GridField.Domain;

    public string DisplayName { get => GridField.DisplayName; set => GridField.DisplayName = value; }

    public DiscretizedField(Veci gridSize, IVectorField<Vec, TData> vectorField, GenBounding<Vec>? altbounding = null)
    {
        var bounding = vectorField.Domain.Bounding;
        if (altbounding != null)
            bounding = altbounding;
        GridField = new RegularGridVectorField<Vec, Veci, TData>(gridSize, new RectDomain<Vec>(vectorField.Domain.RectBoundary, bounding));
        //for (int i = 0; i < GridField.Grid.Data.Length; i++)
        Parallel.For(0, GridField.Grid.Data.Length, i =>
        {
            var pos = GridField.ToWorldPos(GridField.Grid.GetIndexCoords(i));
            GridField.Grid.Data[i] = vectorField.Evaluate(pos);
        });
    }

    public TData Evaluate(Vec x)
    {
        return GridField.Evaluate(x);
    }

    public bool TryEvaluate(Vec x, out TData value)
    {
        return GridField.TryEvaluate(x, out value);
    }
    
  

}