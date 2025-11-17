using ImGuiNET;

namespace FlowExplainer;


public class CriticalPointDiagnostic : IGridDiagnostic
{
    public void UpdateGridData(GridVisualizer gridVisualizer, CancellationToken token)
    {
        var renderGrid = gridVisualizer.RegularGrid.Grid;
        var dat = gridVisualizer.GetRequiredWorldService<DataService>();
        var vectorField = dat.VectorField;
        var spaceBounds = dat.VectorField.Domain.RectBoundary.Reduce<Vec2>();

        var t = dat.SimulationTime;
        var d = vectorField.Domain.RectBoundary.Size.XY / (renderGrid.GridSize.ToVec2() - Vec2.One);

        ParallelGrid.For(renderGrid.GridSize, token,  (i,j) =>
        {
            var pos = spaceBounds.Relative(new Vec2(i, j) / (renderGrid.GridSize.ToVec2() - Vec2.One));
            renderGrid.AtCoords(new Vec2i(i, j)).Padding = vectorField.Evaluate(pos.Up(t));
        });

        ParallelGrid.For(renderGrid.GridSize, token,  (i,j) =>
        {
            renderGrid.AtCoords(new Vec2i(i, j)).Value = 0;
            var pos = (new Vec2(i, j) / renderGrid.GridSize.ToVec2()) * spaceBounds.Size + spaceBounds.Min;
            if (i > 0 && j > 0 && i < renderGrid.GridSize.X - 1 && j < renderGrid.GridSize.Y - 1)
            {
                var left = renderGrid.AtCoords(new Vec2i(i - 1, j)).Padding;
                var right = renderGrid.AtCoords(new Vec2i(i + 1, j)).Padding;
                var up = renderGrid.AtCoords(new Vec2i(i, j + 1)).Padding;
                var down = renderGrid.AtCoords(new Vec2i(i, j - 1)).Padding;


                renderGrid.AtCoords(new Vec2i(i, j)).Value = 0;
                if ((left.X * right.X < 0 || up.X * down.X < 0) &&
                    (left.Y * right.Y < 0 || up.Y * down.Y < 0))
                {
                    renderGrid.AtCoords(new Vec2i(i, j)).Value = 1;
                }
            }
        });
    }

    public void OnImGuiEdit(GridVisualizer gridVisualizer)
    {
        
    }
}

public class StagnationGridDiagnostic : IGridDiagnostic
{
    public void UpdateGridData(GridVisualizer gridVisualizer, CancellationToken token)
    {
        var renderGrid = gridVisualizer.RegularGrid.Grid;
        var dat = gridVisualizer.GetRequiredWorldService<DataService>();
        var vectorField = dat.VectorField;
        var spaceBounds = dat.VectorField.Domain.RectBoundary.Reduce<Vec2>();

        var t = dat.SimulationTime;
        var d = vectorField.Domain.RectBoundary.Size.XY / (renderGrid.GridSize.ToVec2() - Vec2.One);
        ParallelGrid.For(renderGrid.GridSize, token,  (i,j) =>
        {
            var pos = spaceBounds.Relative(new Vec2(i, j) / (renderGrid.GridSize.ToVec2() - Vec2.One));
            renderGrid.AtCoords(new Vec2i(i, j)).Value = vectorField.Evaluate(pos.Up(t)).Length();
            /*if( vectorField.Evaluate(pos.Up(t)).Length() < 0.04f)
            renderGrid.AtCoords(new Vec2i(i, j)).Value = 1;*/
        });
    }

    public void OnImGuiEdit(GridVisualizer gridVisualizer)
    {

    }
}

public class DivergenceGridDiagnostic : IGridDiagnostic
{
    public void UpdateGridData(GridVisualizer gridVisualizer, CancellationToken token)
    {
        var renderGrid = gridVisualizer.RegularGrid.Grid;
        var dat = gridVisualizer.GetRequiredWorldService<DataService>();
        var vectorField = dat.VectorField;
        var spaceBounds = dat.VectorField.Domain.RectBoundary.Reduce<Vec2>();

        var t = dat.SimulationTime;
        var d = vectorField.Domain.RectBoundary.Size.XY / (renderGrid.GridSize.ToVec2() - Vec2.One);
        ParallelGrid.For(renderGrid.GridSize, token,  (i,j) =>
        {
            var pos = spaceBounds.Relative(new Vec2(i, j) / (renderGrid.GridSize.ToVec2() - Vec2.One));
            renderGrid.AtCoords(new Vec2i(i, j)).Padding = vectorField.Evaluate(pos.Up(t));
        });

        ParallelGrid.For(renderGrid.GridSize, token,  (i,j) =>
        {
            renderGrid.AtCoords(new Vec2i(i, j)).Value = 0;
            var pos = (new Vec2(i, j) / renderGrid.GridSize.ToVec2()) * spaceBounds.Size + spaceBounds.Min;
            if (i > 0 && j > 0 && i < renderGrid.GridSize.X - 1 && j < renderGrid.GridSize.Y - 1)
            {
                var left = renderGrid.AtCoords(new Vec2i(i - 1, j)).Padding;
                var right = renderGrid.AtCoords(new Vec2i(i + 1, j)).Padding;
                var up = renderGrid.AtCoords(new Vec2i(i, j + 1)).Padding;
                var down = renderGrid.AtCoords(new Vec2i(i, j - 1)).Padding;
                renderGrid.AtCoords(new Vec2i(i, j)).Value = FD.Divergence(left, right, down, up, d);
                    
                    /*
                    FD.Derivative(left.X, right.X, d.X) + FD.Derivative(up.Y, down.Y, d.Y);
                    (right.X - left.X) / (2 * d.X) +
                    (up.Y - down.Y) / (2 * d.Y);*/
            }
        });
    }
    public void OnImGuiEdit(GridVisualizer gridVisualizer)
    {

    }
}