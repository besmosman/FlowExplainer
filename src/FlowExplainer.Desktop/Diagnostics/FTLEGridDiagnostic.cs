using System.Runtime.InteropServices;
using ImGuiNET;
using OpenTK.Mathematics;

namespace FlowExplainer;

public class FTLEGridDiagnostic : IGridDiagnostic
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FTLEData
    {
        public Vec2 StartPosition;
        public Vec2 FinalPosition;
        public Vec3 padding;
        public double FTLE;
    }

    public double T = 1;
    private FTLEData[] Data;

    public void UpdateGridData(GridVisualizer gridVisualizer, CancellationToken token)
    {
        //var renderGrid = Data;

        var renderGrid = gridVisualizer.RegularGrid.Grid;
        var dat = gridVisualizer.GetWorldService<DataService>()!;
        var vectorField = dat.VectorField;
        var domain = vectorField.Domain;

        var t = dat.SimulationTime;
        var tau = dat.SimulationTime + T;
        if (Data == null || Data.Length != renderGrid.Data.Length)
            Data = new FTLEData[renderGrid.Data.Length];

        var spatialBounds = domain.RectBoundary.Reduce<Vec2>();
        var flowOperator = IFlowOperator<Vec2, Vec3>.Default;

        ParallelGrid.For(renderGrid.GridSize, token, (i, j) =>
        {
            var pos = spatialBounds.FromRelative(new Vec2(i, j) / renderGrid.GridSize.ToVec2());
            var center = flowOperator.Compute(t, tau, pos, vectorField);
            var index = renderGrid.GetCoordsIndex(new Vec2i(i, j));
            Data[index] = new FTLEData
            {
                FinalPosition = center.Entries.Last().XY,
                StartPosition = center.Entries.First().XY,
            };
        });

        ParallelGrid.For(renderGrid.GridSize , token, (i, j) =>
        {
            ref var center = ref renderGrid.AtCoords(new Vec2i(i, j));
            if (i > 0 && j > 0 && i < renderGrid.GridSize.X - 1 && j < renderGrid.GridSize.Y - 1)
            {
                var end_left = Data[renderGrid.GetCoordsIndex(new Vec2i(i - 1, j))].FinalPosition;
                var end_right = Data[renderGrid.GetCoordsIndex(new Vec2i(i + 1, j))].FinalPosition;
                var end_up = Data[renderGrid.GetCoordsIndex(new Vec2i(i, j - 1))].FinalPosition;
                var end_down = Data[renderGrid.GetCoordsIndex(new Vec2i(i, j + 1))].FinalPosition;

                var start_right = Data[renderGrid.GetCoordsIndex(new Vec2i(i - 1, j))].StartPosition;
                var start_left = Data[renderGrid.GetCoordsIndex(new Vec2i(i + 1, j))].StartPosition;
                var start_down = Data[renderGrid.GetCoordsIndex(new Vec2i(i, j + 1))].StartPosition;
                var start_up = Data[renderGrid.GetCoordsIndex(new Vec2i(i, j - 1))].StartPosition;
                double dX = start_left.X - start_right.X;
                double dY = (start_down.Y - start_up.Y);

                Matrix2d gradient = new Matrix2d(
                    (end_left.X - end_right.X) / dX,
                    (end_down.X - end_up.X) / dY,
                    (end_left.Y - end_right.Y) / dX,
                    (end_down.Y - end_up.Y) / dY
                );
                

                var delta = gradient * gradient.Transposed();

                var m = delta.Trace * .5f;
                var p = delta.Determinant;
                var n = m * m - p;

                if (n < 1e-05)
                    n = 0;

                var right = double.Sqrt(n);
                var max_eigen = double.Max(m + right, m - right);
                center.Value = (1f / double.Abs(T)) * double.Log(double.Sqrt(max_eigen));
                // center = c % 2 == 0 ? 1 : 0;
                //center = 0;
            }
            else
            {
                center.Value = 0;
            }
        });

        for (int i = 0; i < renderGrid.GridSize.X; i++)
        {
            renderGrid.AtCoords(new(i, 0)) = renderGrid.AtCoords(new(i, 1));
            renderGrid.AtCoords(new(i, renderGrid.GridSize.Y - 1)) = renderGrid.AtCoords(new(i, renderGrid.GridSize.Y - 2));
        }

        for (int j = 0; j < renderGrid.GridSize.Y; j++)
        {
            renderGrid.AtCoords(new(0, j)) = renderGrid.AtCoords(new(1, j));
            renderGrid.AtCoords(new(renderGrid.GridSize.X - 1, j)) = renderGrid.AtCoords(new(renderGrid.GridSize.X - 2, j));
        }
    }

    public void OnImGuiEdit(GridVisualizer vis)
    {
        var dat = vis.GetRequiredWorldService<DataService>()!;
        double period = dat.VectorField.Domain.RectBoundary.Size.Last;
        if (ImGuiHelpers.SliderFloat("T", ref T, -period * 1, period * 1))
            vis.MarkDirty = true;
    }
}