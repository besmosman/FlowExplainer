using System.Runtime.CompilerServices;

namespace FlowExplainer;

public class LICS : IGridDiagnostic
{

    private IVectorField<Vec3, Vec2> VectorField;
    public Func<Trajectory<Vec3>, float, float> Metric;
    public float T = .2f;
    public float Multi = 1f;
    public IKernel Kernel = new ConstantKernel();

    private static IVectorField<Vec2, float> NoiseField = new NoiseField();


    public void UpdateGridData(GridVisualizer gridVisualizer)
    {
        Metric = static (trajectory, c) => NoiseField.Evaluate(trajectory.AtC(c).XY);
        var vec = gridVisualizer.GetRequiredWorldService<DataService>().VectorField;
        VectorField = new ArbitraryField<Vec3, Vec2>(vec.Domain, p => vec.Evaluate(p) * Multi);
        var flowop = IFlowOperator<Vec2, Vec3>.Default;
        var t_start = gridVisualizer.GetRequiredWorldService<DataService>().SimulationTime;
        var t_end = t_start + T;
        ParallelGrid.RunMainThread(gridVisualizer.RegularGrid.GridSize, (i, j) =>
        {
            ref var atCoords = ref gridVisualizer.RegularGrid.AtCoords(new Vec2i(i, j));
            var pos = gridVisualizer.RegularGrid.ToWorldPos(new Vec2(i + .5f, j + .5f));
            var traj = flowop.Compute(t_start, t_end, pos, VectorField);
            var totWeight = 0f;
            var sum = 0f;
            foreach (var pair in traj.Enumerate())
            {
                var weight = Kernel.GetWeight(pair.c);
                if (weight != 0)
                {
                    totWeight += weight;
                    sum += Metric(traj, pair.c) * weight;
                }
            }
            var value = sum / totWeight;
            atCoords.Value = value;
        });
    }
    public void OnImGuiEdit(GridVisualizer gridVisualizer)
    {
        ImGuiHelpers.SliderFloat("T", ref T, 0, 2);
        ImGuiHelpers.SliderFloat("Mutli", ref Multi, 0, 10);
    }
}