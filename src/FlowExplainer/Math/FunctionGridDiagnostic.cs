using System.Net.Http.Headers;

namespace FlowExplainer;

public class FunctionGridDiagnostic : IGridDiagnostic
{
    public float T = 950;

    public void UpdateGridData(GridVisualizer gridVisualizer)
    {
        var renderGrid = gridVisualizer.RegularGrid.Grid;
        var dat = gridVisualizer.GetWorldService<DataService>()!;
        var vectorField = dat.VectorField;
        var vectorFieldInverse = new ArbitraryField<Vec3, Vec2>(dat.VectorField.Domain, (p) => -dat.VectorField.Evaluate(p));
        var domain = vectorField.Domain;
        var spatialBounds = domain.Boundary.Reduce<Vec2>();
        var flowOperator = IFlowOperator<Vec2, Vec3>.Default;

        float t = dat.SimulationTime;
        float tau = t + T;
        gridVisualizer.RegularGrid.Grid.Data.AsSpan().Fill(default);


        float F(Trajectory<Vec3> trajectory)
        {
            var p = trajectory.Entries.Last();
            var rel = spatialBounds.Relative(p.Down());
            //return Vec2.Distance(trajectory.Entries.First().XY, trajectory.Entries.Last().XY);
            //return trajectory.AverageAlong((p, c) => (c - p).Down().Length());
            return trajectory.AverageAlong((p, c) => (c - p).Down().Length());
            //return trajectory.Entries.Last().X/30f;
            //return float.Sin((rel.X - rel.Y * 90) / 550);
        }

        ParallelGrid.For(renderGrid.GridSize, (i, j) =>
        {
            var pos = spatialBounds.Relative(new Vec2(i, j) / renderGrid.GridSize.ToVec2());
            var traj = flowOperator.Compute(t, tau, pos, vectorField);
            var offset = -(traj.Entries.Last() - traj.Entries.First()).Down();
            /*foreach (ref var f in traj.Entries.AsSpan())
            {
                f += new Vec3(offset, 0);
            }*/
            var v = F(traj);
            if (spatialBounds.Contains(traj.Entries.Last().Down()))
                gridVisualizer.RegularGrid.AtPos(traj.Entries.Last().Down()).Value = 0;
        });

        ParallelGrid.For(renderGrid.GridSize, (i, j) =>
        {
            var pos = spatialBounds.Relative(new Vec2(i, j) / renderGrid.GridSize.ToVec2());
            var trajBack = flowOperator.Compute(tau, t, pos, vectorFieldInverse);
            var traj = flowOperator.Compute(t, tau, trajBack.Entries.Last().Down(), vectorField);
            var trajt = flowOperator.Compute(t, tau, pos, vectorField);
            /*foreach (ref var f in traj.Entries.AsSpan())
            {
                f += new Vec3(offset, 0);
            }*/
            var v = F(trajBack.Reverse());
            if (pos.X < 10)
            {
                var setPos = pos;
                if (spatialBounds.Contains(setPos))
                    gridVisualizer.RegularGrid.AtPos(setPos).Value = v;
            }
        });
        /*ParallelGrid.For(renderGrid.GridSize, (i, j) =>
        {
            var pos = spatialBounds.Relative(new Vec2(i, j) / renderGrid.GridSize.ToVec2());
            if (pos.X < 10)
                gridVisualizer.RegularGrid.Grid.AtCoords(new Vec2i(i, j)).Value = F(flowOperator.Compute(t, tau, pos, vectorField));
        });*/
        /*
        ParallelGrid.For(renderGrid.GridSize, (i, j) =>
        {
            int s = 1;
            for (int k = 0; k < s; k++)
            {
                var pos = spatialBounds.Relative(new Vec2(i, j) / renderGrid.GridSize.ToVec2());
                var trajBack = flowOperator.Compute(t, tau * (k + 1) * (s), pos, vectorField);
                var length = trajBack.AverageAlong((p, c) => (c - p).Down().Length());
                var end = trajBack.Entries.Last().Down();
                //end = pos;
                var rel = spatialBounds.Relative(end);
                var v = float.Sin((rel.X - rel.Y * 90) / 550);
                v = length;
                //if (spatialBounds.IsWithin(end))
                //    gridVisualizer.RegularGrid.AtPos(pos).Value = v;
                renderGrid[new Vec2i(i, j)].Marked = v;
                renderGrid[new Vec2i(i, j)].Padding = (trajBack.Entries.Last() - trajBack.Entries.First()).Down();
            }
        });

        ParallelGrid.For(renderGrid.GridSize, (i, j) =>
        {
            var pos = spatialBounds.Relative(new Vec2(i, j) / renderGrid.GridSize.ToVec2());
            pos += renderGrid[new Vec2i(i, j)].Padding;
            if (spatialBounds.IsWithin(pos))
                gridVisualizer.RegularGrid.AtPos(pos).Value = renderGrid[new Vec2i(i, j)].Marked;

            /*
            renderGrid[new Vec2i(i, j)].Marked = 0;
            #1#
        });


        ParallelGrid.For(renderGrid.GridSize, (i, j) =>
        {
            //renderGrid[new Vec2i(i, j)].Marked = 0;
            renderGrid[new Vec2i(i, j)].Value = renderGrid[new Vec2i(i, j)].Marked;
        });*/
    }

    public void OnImGuiEdit(GridVisualizer gridVisualizer)
    {
        ImGuiHelpers.SliderFloat("T", ref T, 0, 1000);
    }
}