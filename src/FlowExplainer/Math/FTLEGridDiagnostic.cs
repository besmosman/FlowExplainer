using System.Runtime.InteropServices;
using ImGuiNET;
using OpenTK.Mathematics;

namespace FlowExplainer;

/*
public class CustomGridDiagnostic : IGridDiagnostic
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FTLEData
    {
        public Vec2 StartPosition;
        public Vec2 FinalPosition;
        public Vec3 padding;
        public float FTLE;
    }

    public Type DataType => typeof(FTLEData);
    public float T = 1;

    public void UpdateGridData(GridVisualizer gridVisualizer)
    {
        var renderGrid = gridVisualizer.GetRenderGrid<FTLEData>();

        renderGrid.SetColorFunction(
            static (gl) => gl.ColorGradient(gl.Dat.FTLE));

        var dat = gridVisualizer.GetWorldService<DataService>()!;
        var domain = dat.VelocityField.Domain.Boundary;

        var t = dat.SimulationTime;
        var tau = dat.SimulationTime + T;

        Parallel.For(0, renderGrid.GridSize.X * renderGrid.GridSize.Y, c =>
        {
            var i = c % renderGrid.GridSize.X;
            var j = c / renderGrid.GridSize.X;
            var pos = new Vec2(i, j) / renderGrid.GridSize.ToVec2() * domain.Size.Down();

            var center = IFlowOperator<Vec2, Vec3>.Default.Compute(t, tau, pos, dat.VelocityField);

            renderGrid.AtCoords(new Vec2i(i, j)) = new FTLEData
            {
                FinalPosition = center.Entries.Last().XY,
                StartPosition = center.Entries.First().XY,
            };
        });

        Parallel.For(0, renderGrid.GridSize.X * renderGrid.GridSize.Y, c =>
        {
            var i = c % renderGrid.GridSize.X;
            var j = c / renderGrid.GridSize.X;

            ref var center = ref renderGrid.AtCoords(new Vec2i(i, j));
            if (i > 0 && j > 0 && i < renderGrid.GridSize.X - 1 && j < renderGrid.GridSize.Y - 1)
            {
                var end_left = renderGrid.AtCoords(new Vec2i(i - 1, j)).FinalPosition;
                var end_right = renderGrid.AtCoords(new Vec2i(i + 1, j)).FinalPosition;
                var end_up = renderGrid.AtCoords(new Vec2i(i, j - 1)).FinalPosition;
                var end_down = renderGrid.AtCoords(new Vec2i(i, j + 1)).FinalPosition;

                var start_right = renderGrid.AtCoords(new Vec2i(i - 1, j)).StartPosition;
                var start_left = renderGrid.AtCoords(new Vec2i(i + 1, j)).StartPosition;
                var start_down = renderGrid.AtCoords(new Vec2i(i, j + 1)).StartPosition;
                var start_up = renderGrid.AtCoords(new Vec2i(i, j - 1)).StartPosition;
                float dX = start_left.X - start_right.X;
                float dY = (start_down.Y - start_up.Y);

                {
                    float length = new Vec2(Vec2.Distance(end_left, end_right) / dX, Vec2.Distance(end_up, end_down) / dY).Length();
                    center.FTLE = 1f / float.Abs(tau - t) * length * .14f;
                }
            }
            else
            {
                center.FTLE = 0;
            }
        });

        for (int i = 0; i < renderGrid.GridSize.X; i++)
        {
            renderGrid.AtCoords(i, 0).FTLE = renderGrid.AtCoords(i, 1).FTLE;
            renderGrid.AtCoords(i, renderGrid.GridSize.Y - 1).FTLE = renderGrid.AtCoords(i, renderGrid.GridSize.Y - 2).FTLE;
        }

        for (int j = 0; j < renderGrid.GridSize.Y; j++)
        {
            renderGrid.AtCoords(0, j).FTLE = renderGrid.AtCoords(1, j).FTLE;
            renderGrid.AtCoords(renderGrid.GridSize.X - 1, j).FTLE = renderGrid.AtCoords(renderGrid.GridSize.X - 2, j).FTLE;
        }
    }

    public void OnImGuiEdit(GridVisualizer vis)
    {
        var dat = vis.GetRequiredWorldService<DataService>()!;
        ImGuiHelpers.SliderFloat("T", ref T, -dat.VelocityField.Domain.Boundary.Size.Last * 4, dat.VelocityField.Domain.Boundary.Size.Last * 4);
    }
}

public class FTLEvsCustomGridDiagnostic : IGridDiagnostic
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FTLEData
    {
        public Vec2 StartPosition;
        public Vec2 FinalPosition;
        public Vec3 padding;
        public float FTLE;
    }

    public Type DataType => typeof(FTLEData);
    public float T = 1;

    public void UpdateGridData(GridVisualizer gridVisualizer)
    {
        var renderGrid = gridVisualizer.GetRenderGrid<FTLEData>();

        renderGrid.SetColorFunction(
            static (gl) => gl.ColorGradient(gl.Dat.FTLE / 2));

        var dat = gridVisualizer.GetWorldService<DataService>()!;
        var domain = dat.VelocityField.Domain;

        var t = dat.SimulationTime;
        var tau = dat.SimulationTime + T;

        Parallel.For(0, renderGrid.GridSize.X * renderGrid.GridSize.Y, c =>
        {
            var i = c % renderGrid.GridSize.X;
            var j = c / renderGrid.GridSize.X;
            var pos = (new Vec2(i, j) / renderGrid.GridSize.ToVec2()) * domain.Boundary.Size.Down() + domain.Boundary.Min.Down();
            pos =  domain.Boundary.Reduce<Vec2>().Relative(new Vec2(i, j) / renderGrid.GridSize.ToVec2());

            var center = IFlowOperator<Vec2, Vec3>.Default.Compute(t, tau, pos, dat.VelocityField);

            renderGrid.AtCoords(new Vec2i(i, j)) = new FTLEData
            {
                FinalPosition = center.Entries.Last().XY,
                StartPosition = center.Entries.First().XY,
            };
        });

        Parallel.For(0, renderGrid.GridSize.X * renderGrid.GridSize.Y, c =>
        {
            var i = c % renderGrid.GridSize.X;
            var j = c / renderGrid.GridSize.X;

            ref var center = ref renderGrid.AtCoords(new Vec2i(i, j));
            if (i > 0 && j > 0 && i < renderGrid.GridSize.X - 1 && j < renderGrid.GridSize.Y - 1)
            {
                float xSlciec = renderGrid.GridSize.X / 2f + float.Sin((float)gridVisualizer.FlowExplainer.Time.TotalSeconds) * 0;
                if (i > xSlciec)
                {
                    var end_left = renderGrid.AtCoords(new Vec2i(i - 1, j)).FinalPosition;
                    var end_right = renderGrid.AtCoords(new Vec2i(i + 1, j)).FinalPosition;
                    var end_up = renderGrid.AtCoords(new Vec2i(i, j - 1)).FinalPosition;
                    var end_down = renderGrid.AtCoords(new Vec2i(i, j + 1)).FinalPosition;

                    var start_right = renderGrid.AtCoords(new Vec2i(i - 1, j)).StartPosition;
                    var start_left = renderGrid.AtCoords(new Vec2i(i + 1, j)).StartPosition;
                    var start_down = renderGrid.AtCoords(new Vec2i(i, j + 1)).StartPosition;
                    var start_up = renderGrid.AtCoords(new Vec2i(i, j - 1)).StartPosition;
                    float dX = start_left.X - start_right.X;
                    float dY = (start_down.Y - start_up.Y);

                    Matrix2 gradient = new Matrix2(
                        (end_left.X - end_right.X) / dX,
                        (end_down.X - end_up.X) / dY,
                        (end_left.Y - end_right.Y) / dX,
                        (end_down.Y - end_up.Y) / dY
                    );

                    var d = new Vec2(Vec2.DistanceSquared(end_left, end_right) / dX, Vec2.DistanceSquared(end_up, end_down) / dY);

                    var delta = gradient * gradient.Transposed();

                    var p_0 = (delta.Row0.X + delta.Row1.Y);
                    var p_1 = (delta.Row0.X + delta.Row1.Y) * .5f;
                    var p_2 = gradient.Row0;
                    var p_3 = gradient.Row1;

                    var m = (delta.Row0.X + delta.Row1.Y) * .5f; // delta.Trace * .5f;
                    var p = delta.Determinant;
                    var n = m * m - p;

                    if (n < 1e-05)
                        n = 0;

                    var right = float.Sqrt(n);
                    var max_eigen = float.Max(m + right, m - right);
                    center.FTLE = (1f / float.Abs(T)) * float.Log(d.Length() * 1f + 0f);
                    // center.FTLE = (1f / float.Abs(T)) * float.Log(float.Sqrt(max_eigen));
                }
                else
                {
                    var end_left = renderGrid.AtCoords(new Vec2i(i - 1, j)).FinalPosition;
                    var end_right = renderGrid.AtCoords(new Vec2i(i + 1, j)).FinalPosition;
                    var end_up = renderGrid.AtCoords(new Vec2i(i, j - 1)).FinalPosition;
                    var end_down = renderGrid.AtCoords(new Vec2i(i, j + 1)).FinalPosition;

                    var start_right = renderGrid.AtCoords(new Vec2i(i - 1, j)).StartPosition;
                    var start_left = renderGrid.AtCoords(new Vec2i(i + 1, j)).StartPosition;
                    var start_down = renderGrid.AtCoords(new Vec2i(i, j + 1)).StartPosition;
                    var start_up = renderGrid.AtCoords(new Vec2i(i, j - 1)).StartPosition;
                    float dX = start_left.X - start_right.X;
                    float dY = (start_down.Y - start_up.Y);

                    Matrix2 gradient = new Matrix2(
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

                    var right = float.Sqrt(n);
                    var max_eigen = float.Max(m + right, m - right);
                    center.FTLE = (1f / float.Abs(T)) * float.Log(float.Sqrt(max_eigen));
                }
            }
            else
            {
                center.FTLE = 0;
            }
        });

        for (int i = 0; i < renderGrid.GridSize.X; i++)
        {
            renderGrid.AtCoords(i, 0).FTLE = renderGrid.AtCoords(i, 1).FTLE;
            renderGrid.AtCoords(i, renderGrid.GridSize.Y - 1).FTLE = renderGrid.AtCoords(i, renderGrid.GridSize.Y - 2).FTLE;
        }

        for (int j = 0; j < renderGrid.GridSize.Y; j++)
        {
            renderGrid.AtCoords(0, j).FTLE = renderGrid.AtCoords(1, j).FTLE;
            renderGrid.AtCoords(renderGrid.GridSize.X - 1, j).FTLE = renderGrid.AtCoords(renderGrid.GridSize.X - 2, j).FTLE;
        }
    }

    public void OnImGuiEdit(GridVisualizer vis)
    {
        var dat = vis.GetRequiredWorldService<DataService>()!;
        float period = dat.VelocityField.Domain.Boundary.Size.Last;
        ImGuiHelpers.SliderFloat("T", ref T, -period * 1, period * 1);

    }
}
*/

public class LagrangianTemperatureGridDiagnostic : IGridDiagnostic
{
    public float T = 1;

    public void UpdateGridData(GridVisualizer gridVisualizer)
    {
        var renderGrid = gridVisualizer.RegularGrid.Grid;
        var dat = gridVisualizer.GetRequiredWorldService<DataService>();
        var tempratureField = dat.TempratureField;
        var spaceBounds = dat.VectorField.Domain.RectBoundary.Reduce<Vec2>();
        var t = dat.SimulationTime;
        var tau = dat.SimulationTime + T;
        Parallel.For(0, renderGrid.GridSize.X * renderGrid.GridSize.Y, c =>
        {
            var i = c % renderGrid.GridSize.X;
            var j = c / renderGrid.GridSize.X;
            var pos = (new Vec2(i, j) / renderGrid.GridSize.ToVec2()) * spaceBounds.Size + spaceBounds.Min;
            var center = IFlowOperator<Vec2, Vec3>.Default.Compute(t, tau, pos, dat.VectorField);
            //change in temprature compared to neighbros doe..
            var first = center.Entries.First();
            var last = center.Entries.Last();
            renderGrid.AtCoords(new Vec2i(i, j)).Value = (tempratureField.Evaluate(last) - tempratureField.Evaluate(first)) / (last - first).Z;
        });
        var average = renderGrid.Data.Average(d => d.Value);
        for (int i = 0; i < renderGrid.Data.Length; i++)
        {
            renderGrid.Data[i].Value -= average;
            renderGrid.Data[i].Value *= 10;

        }
    }
    public void OnImGuiEdit(GridVisualizer gridVisualizer)
    {
        var dat = gridVisualizer.GetRequiredWorldService<DataService>()!;
        float period = dat.VectorField.Domain.RectBoundary.Size.Last;
        ImGuiHelpers.SliderFloat("T", ref T, -period, period);
    }

}

public class FTLEGridDiagnostic : IGridDiagnostic
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FTLEData
    {
        public Vec2 StartPosition;
        public Vec2 FinalPosition;
        public Vec3 padding;
        public float FTLE;
    }

    public float T = 1;
    private FTLEData[] Data;

    public void UpdateGridData(GridVisualizer gridVisualizer)
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

        ParallelGrid.For(renderGrid.GridSize, (i, j) =>
        {
            var pos = spatialBounds.Relative(new Vec2(i, j) / renderGrid.GridSize.ToVec2());
            var center = flowOperator.Compute(t, tau, pos, vectorField);
            var index = renderGrid.GetCoordsIndex(new Vec2i(i, j));
            Data[index] = new FTLEData
            {
                FinalPosition = center.Entries.Last().XY,
                StartPosition = center.Entries.First().XY,
            };
        });

        ParallelGrid.For(renderGrid.GridSize, (i, j) =>
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
                float dX = start_left.X - start_right.X;
                float dY = (start_down.Y - start_up.Y);

                Matrix2 gradient = new Matrix2(
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

                var right = float.Sqrt(n);
                var max_eigen = float.Max(m + right, m - right);
                center.Value = (1f / float.Abs(T)) * float.Log(float.Sqrt(max_eigen));
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
        float period = dat.VectorField.Domain.RectBoundary.Size.Last;
        if (ImGuiHelpers.SliderFloat("T", ref T, -period * 1, period * 1))
            vis.MarkDirty = true;
    }
}