using System.Numerics;

namespace FlowExplainer;

public class DiscretizedField<Vec, Veci, TData> : IVectorField<Vec, TData>
    where Vec : IVec<Vec>, IVecIntegerEquivalent<Veci>
    where Veci : IVec<Veci, int>, IVecDoubleEquivalent<Vec>
    where TData : IMultiplyOperators<TData, double, TData>, IAdditionOperators<TData, TData, TData>
{
    public RegularGridVectorField<Vec, Veci, TData> GridField { get; }
    public IDomain<Vec> Domain => GridField.Domain;

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