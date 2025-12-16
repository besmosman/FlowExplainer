namespace FlowExplainer;

public class PoincareSmear2GridDiagnostic : IGridDiagnostic
{
    public bool UseCustomColoring => true;
    public double DisplayT;

    RegularGrid<Vec2i, Trajectory<Vec3>> trajectories = new(Vec2i.One);
    public void UpdateGridData(GridVisualizer gridVisualizer, CancellationToken token)
    {
        var dat = gridVisualizer.GetRequiredWorldService<DataService>();
        var rect = dat.VectorField.Domain.RectBoundary;
        double period = rect.Size.Z;
        int periods = 300;
        int stepsPerPeriod = 100;
        var flowOperator = new IFlowOperator<Vec2, Vec3>.DefaultFlowOperator(stepsPerPeriod * periods);
        if (gridVisualizer.RegularGrid.GridSize != trajectories.GridSize)
        {
            trajectories.Resize(gridVisualizer.RegularGrid.GridSize);
            ParallelGrid.For(gridVisualizer.RegularGrid.GridSize, token,
                (i, j) =>
                {
                    var pos = gridVisualizer.RegularGrid.ToWorldPos(new Vec2(i + .5f, j + .5f));
                    trajectories.AtCoords(new Vec2i(i, j)) = flowOperator.ComputeTrajectory(0, periods * period, pos, dat.VectorField);
                });
        }
        
        ParallelGrid.For(gridVisualizer.RegularGrid.GridSize, token,
            (i, j) => { trajectories.AtCoords(new Vec2i(i, j)).AtTimeBilinear(DisplayT); });


        var Integrator = IIntegrator<Vec3, Vec2>.Rk4;
        ParallelGrid.For(gridVisualizer.RegularGrid.GridSize, token,
            (i, j) =>
            {
                int steps = 20;
                gridVisualizer.RegularGrid.AtCoords(new Vec2i(i, j)).Value = 0;
                ref var atCoords = ref gridVisualizer.RegularGrid.AtCoords(new Vec2i(i, j));
                var pos = gridVisualizer.RegularGrid.ToWorldPos(new Vec2(i + .5f, j + .5f));
                for (int k = 0; k < steps; k++)
                {
                    double t = k / (double)steps;
                    int coherent = EstimateChaos(pos, t) > 145 ? 1 : 0;
                    atCoords.Value += coherent * (1f / (double)steps);
                    if (coherent > 0)
                    {
                        atCoords.Color = Gradients.Parula.GetCached(t) * (float)t;
                    }
                }
            });

        double GetWeight(double dis)
        {
            double sigma = 0.3f;
            return (double)Math.Exp(-(dis * dis) / (2 * sigma * sigma));
        }

        double EstimateChaos(Vec2 x, double startPhase)
        {
            var gridSize = new Vec2i(20, 10) * 2;
            var densityGrid = new RegularGridVectorField<Vec2, Vec2i, double>(Rental<double>.Rent(gridSize.X * gridSize.Y), gridSize, rect.Min.XY, rect.Max.XY);
            densityGrid.Grid.Data.AsSpan().Fill(0);
            var pos = x;
            double dt = period / stepsPerPeriod;
            for (int p = 0; p < periods; p++)
            {
                for (int i = 0; i < stepsPerPeriod; i++)
                {
                    double t = (p * stepsPerPeriod + i) * dt + startPhase;
                    pos = Integrator.Integrate(dat.VectorField, pos.Up(t), dt).XY;
                    pos = dat.VectorField.Domain.Bounding.Bound(pos.Up(t)).XY;
                }
                densityGrid.AtPos(pos)++;
            }
            var f = densityGrid.Grid.Data.Max();
            Rental<double>.Return(densityGrid.Grid.Data);
            return f;
        }
    }


    public void OnImGuiEdit(GridVisualizer gridVisualizer)
    {

    }
}