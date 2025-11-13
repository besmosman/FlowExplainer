using ImGuiNET;
using MemoryPack;

namespace FlowExplainer;

public class Knn<Vec, Veci>
    where Vec : IVec<Vec>, IVecIntegerEquivalent<Veci>
    where Veci : IVec<Veci, int>, IVecDoubleEquivalent<Vec>
{
    public void BuildRented(IEnumerable<Vec> vecs)
    {
        //RegularGrid<Veci, > 

    }
}

public class PoincareSmearGridDiagnostic : IGridDiagnostic
{
    public bool UseCustomColoring => !Average;
    public bool Average = false;

    public void UpdateGridData(GridVisualizer gridVisualizer)
    {
        int periods = 100;
        int stepsPerPeriod = 64;
        var dat = gridVisualizer.GetRequiredWorldService<DataService>();
        var rect = dat.VectorField.Domain.RectBoundary;
        double period = rect.Size.Z;
        var Integrator = IIntegrator<Vec3, Vec2>.Rk4;

        if (false)
        {
            var gridSize = new Vec2i(20, 10) * 3;
            var densityGrid = new RegularGridVectorField<Vec2, Vec2i, double>(Rental<double>.Rent(gridSize.X * gridSize.Y), gridSize, rect.Min.XY, rect.Max.XY);
            densityGrid.Grid.Data.AsSpan().Fill(0);
            var pos = new Vec2(.3f,.4f);
            double dt = period / stepsPerPeriod;
            for (int p = 0; p < periods; p++)
            {
                for (int i = 0; i < stepsPerPeriod; i++)
                {
                    double t = (p * stepsPerPeriod + i) * dt;
                    pos = Integrator.Integrate(dat.VectorField, pos.Up(t), dt);
                    pos = dat.VectorField.Domain.Bounding.Bound(pos.Up(t)).XY;
                }
                if (p > period / 5)
                    densityGrid.AtPos(pos)++;
            }
            
            ParallelGrid.For(gridVisualizer.RegularGrid.GridSize,
                (i, j) =>
                {
                    var rel = new Vec2(i, j) / gridVisualizer.RegularGrid.GridSize.ToVec2() * new Vec2(1,.5f);
                    gridVisualizer.RegularGrid.AtCoords(new Vec2i(i, j)).Value =EstimateIfChaos(rel, 0)  > 21.5f ? 1 : 0;
                });
            Rental<double>.Return(densityGrid.Grid.Data);
        return;
        }
        
        ParallelGrid.For(gridVisualizer.RegularGrid.GridSize,
            (i, j) =>
            {
                int steps = 64;
                gridVisualizer.RegularGrid.AtCoords(new Vec2i(i, j)).Value = 0;
                ref var atCoords = ref gridVisualizer.RegularGrid.AtCoords(new Vec2i(i, j));
                var pos = gridVisualizer.RegularGrid.ToWorldPos(new Vec2(i + .5f, j + .5f));
                double totWeight =0.0;
                for (int k = 0; k < steps; k++)
                {
                    double t = (k) / (double)steps;
                    bool coherent = EstimateIfChaos(pos, (k + 1) / (double)steps) > 22.5f;
                    if (coherent)
                    {
                        if (Average)
                        {
                            atCoords.Value += 1f / steps;
                        }
                        else
                        {
                            double weight = GetWeight(1f - t);
                            weight = 1;
                            atCoords.Color = dat.ColorGradient.Get(t) * (float)weight;
                            totWeight += weight;
                        }
                    }
                    //atCoords.Value = EstimateIfChaos(pos, (k + 1) / (double)steps);

                }
                //atCoords.Color /= totWeight;
                if (totWeight == 0)
                    atCoords.Color = Color.Black;

            });

        double GetWeight(double dis)
        {
            double sigma = 0.3f;
            return (double)Math.Exp(-(dis * dis) / (2 * sigma * sigma));
        }

        /*bool EstimateIfChaos(Vec2 x, double startPhase)
        {
            var gridSize = new Vec2(20, 10)*3;
            var hashset = new HashSet<Vec2i>();
            var pos = x;
            double dt = period / stepsPerPeriod;

            int maxPerdiods = periods;
            double chanceChaos =0.0;
            for (int p = 0; p < maxPerdiods; p++)
            {
                for (int i = 0; i < stepsPerPeriod; i++)
                {
                    double t = (p * stepsPerPeriod + i) * dt + startPhase;
                    pos = Integrator.Integrate(dat.VectorField, pos.Up(t), dt);
                    pos = dat.VectorField.Boundary.Wrap(pos.Up(t)).XY;
                }
                hashset.Add((pos * new Vec2(2, 1) * gridSize).Floor());
                var filled = (hashset.Count / ((double)p+1) * (gridSize.X * gridSize.Y)) / (gridSize.X * gridSize.Y);
                chanceChaos = Utils.Lerp(chanceChaos, filled, .1f);
                if (chanceChaos > .7f)
                    return true;
            }
            return false;
        }*/

        double EstimateIfChaos(Vec2 x, double startPhase)
        {
            var gridSize = new Vec2i(20, 10) / 2;
            var densityGrid = new RegularGridVectorField<Vec2, Vec2i, double>(Rental<double>.Rent(gridSize.X * gridSize.Y), gridSize, rect.Min.XY, rect.Max.XY);
            densityGrid.Grid.Data.AsSpan().Fill(0);
            var pos = x;
            double dt = period / stepsPerPeriod;
            for (int p = 0; p < periods; p++)
            {
                for (int i = 0; i < stepsPerPeriod; i++)
                {
                    double t = (p * stepsPerPeriod + i) * dt + startPhase;
                    pos = Integrator.Integrate(dat.VectorField, pos.Up(t), dt);
                    pos = dat.VectorField.Domain.Bounding.Bound(pos.Up(t)).XY;
                }
                if (p > period / 5)
                    densityGrid.AtPos(pos)++;
            }
            densityGrid.Grid.Data.Sort();

            var sum = 0.0;
            var c = 0;

            for (int i = densityGrid.Grid.Data.Length - 1; i >= densityGrid.Grid.Data.Length -3; i--)
            {
                sum += densityGrid.Grid.Data[i];
                c++;
            }
            var f = sum / c;

            Rental<double>.Return(densityGrid.Grid.Data);
            return f;
        }
    }


    public void OnImGuiEdit(GridVisualizer gridVisualizer)
    {
        ImGui.Checkbox("Average", ref Average);
    }
}