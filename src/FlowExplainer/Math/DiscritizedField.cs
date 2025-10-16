using System.Numerics;

namespace FlowExplainer;

public class DiscritizedField<Vec, Veci, TData> : IVectorField<Vec, TData>
    where Vec : IVec<Vec>, IVecIntegerEquivelant<Veci>
    where Veci : IVec<Veci, int>, IVecFloatEquivelant<Vec>
    where TData : IMultiplyOperators<TData, float, TData>, IAdditionOperators<TData, TData, TData>
{
    public RegularGridVectorField<Vec, Veci, TData> GridField { get; private set; }
    public IDomain<Vec> Domain => GridField.Domain;
    public IBoundary<Vec> Boundary => GridField.Boundary;

    public DiscritizedField(Veci gridSize, IVectorField<Vec, TData> vectorField)
    {
        GridField = new RegularGridVectorField<Vec, Veci, TData>(gridSize, new RectDomain<Vec>(vectorField.Domain.RectBoundary));
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