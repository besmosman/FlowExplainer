using System.Numerics;

namespace FlowExplainer;

public static class GridComputations
{
    public static RegularGrid<Veci, TData> Multiply<Veci, TData, TN>(TN v, RegularGrid<Veci, TData> grid)
        where Veci : IVec<Veci, int>
        where TData : IMultiplyOperators<TData, TN, TData>
    {
        var r = new RegularGrid<Veci, TData>(grid.GridSize);
        for (int i = 0; i < grid.Data.Length; i++)
            r.Data[i] = grid.Data[i] * v;
        return r;
    }
}