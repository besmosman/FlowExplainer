using System.Net.Http.Headers;
using ImGuiNET;

namespace FlowExplainer;

public class FunctionGridDiagnostic : IGridDiagnostic
{
    public float T = 1;
    public bool UseGradient;
    public bool StandardLCS = true;
    public int K = 10;
    
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
        
        float F_along(Vec3 last, Vec3 cur)
        {
            //return Vec2.Distance(trajectory.Entries.First().XY, trajectory.Entries.Last().XY);
            //return trajectory.AverageAlong((p, c) => (c - p).Down().Length());
            //return trajectory.Entries.Last().X/30f;
            if (!UseGradient)
                return (cur - last).Down().Length();
            return float.Sin((cur.X+cur.Y)*8);
        }

        if (StandardLCS)
        {
            
        //LCS standard
        ParallelGrid.For(renderGrid.GridSize, (i, j) =>
        {
            var pos = spatialBounds.Relative(new Vec2(i, j) / renderGrid.GridSize.ToVec2());
            var trajectory = flowOperator.Compute(t, tau, pos, vectorField);
            gridVisualizer.RegularGrid.Grid.AtCoords(new Vec2i(i, j)).Value = trajectory.AverageAlong(F_along);
        });
        }
        else
        {
            gridVisualizer.RegularGrid.Grid.Data.AsSpan().Fill(default);
            ParallelGrid.For(renderGrid.GridSize, (i, j) =>
            {
                for (int k = 0; k < K; k++)
                {
                    var pos = spatialBounds.Relative(new Vec2(i, j) / renderGrid.GridSize.ToVec2());
                   // pos = Utils.Random(spatialBounds);
                    var traj = flowOperator.Compute(t, tau, pos, vectorField);

                    for (int index = 2; index < traj.Entries.Length; index++)
                    {
                        var last = traj.Entries[index - 1];
                        var cur = traj.Entries[index];

                        var last_T = traj.Entries[0];
                        var cur_T = traj.Entries[1];
                        /*last_T.Z = traj.Entries[index - 2].Z;
                        cur_T.Z = traj.Entries[index - 1].Z;*/
                        var v = F_along(last, cur);

                        var targPos = cur.XY;
                        if (spatialBounds.Contains(targPos))
                        {
                            ref var targ = ref gridVisualizer.RegularGrid.AtPos(targPos);
                            var dis = (float)index / (traj.Entries.Length - 1f);
                            float weight = 1;
                            targ.Value += v * weight;
                            targ.Marked += weight;
                        }
                    }
                }
            });
       

        static float Kernel(float dis)
        {
            float sigma = 0.8f;
            return (float)Math.Exp(-(dis * dis) / (2 * sigma * sigma));
        }

        ParallelGrid.For(renderGrid.GridSize, (i, j) =>
        {
            ref var atCoords = ref renderGrid.AtCoords(new Vec2i(i, j));
            var pos = spatialBounds.Relative(new Vec2(i, j) / renderGrid.GridSize.ToVec2());
            var traj = flowOperator.Compute(t, tau, pos, vectorField);
            var last = traj.Entries[0];
            var cur = traj.Entries[^1];
            var v_start = F_along(last, cur);
            atCoords.Value /= atCoords.Marked;
            //atCoords.Value -= v_start;
            atCoords.Marked = 0;
        });
        }
        /*
        ParallelGrid.For(renderGrid.GridSize, (i, j) =>
        {
            var pos = spatialBounds.Relative(new Vec2(i, j) / renderGrid.GridSize.ToVec2());
            var trajBack = flowOperator.Compute(tau, t, pos, vectorFieldInverse);
            var traj = flowOperator.Compute(t, tau, trajBack.Entries.Last().Down(), vectorField);
            var trajt = flowOperator.Compute(t, tau, pos, vectorField);
            /*foreach (ref var f in traj.Entries.AsSpan())
            {
                f += new Vec3(offset, 0);
            }#1#
            var v = F(trajBack.Reverse());
            // if (pos.X < .5f)
            {
                var setPos = pos;
                if (spatialBounds.Contains(setPos))
                    gridVisualizer.RegularGrid.AtPos(setPos).Value = v;
            }
        });*/
        /*ParallelGrid.For(renderGrid.GridSize, (i, j) =>
    {
        var pos = spatialBounds.Relative(new Vec2(i, j) / renderGrid.GridSize.ToVec2());
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
        ImGui.Checkbox("use gradient", ref UseGradient);
        ImGui.Checkbox("LCS", ref StandardLCS);
    }
}