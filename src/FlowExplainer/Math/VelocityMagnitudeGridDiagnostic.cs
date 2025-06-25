using System.Runtime.InteropServices;
using ImGuiNET;

namespace FlowExplainer;

public class VelocityMagnitudeGridDiagnostic : IGridDiagnostic
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VelData
    {
        public float AverageVelocityMagnitude;
    }

    public float T = 1;

    public Type DataType => typeof(VelData);

    public void UpdateGridData(GridVisualizer vis)
    {
        var renderGrid = vis.GetRenderGrid<VelData>();

        var dat = vis.GetRequiredWorldService<DataService>()!;
        var domain = dat.VelocityField.Domain;

        var t = dat.SimulationTime;
        var tau = dat.SimulationTime + T;

        renderGrid.SetColorFunction(
            (gl) => gl.ColorGradient(gl.Dat.AverageVelocityMagnitude));

        Parallel.For(0, renderGrid.GridSize.X * renderGrid.GridSize.Y, c =>
        {
            var i = c % renderGrid.GridSize.X;
            var j = c / renderGrid.GridSize.X;
            var pos = new Vec2(i, j) / renderGrid.GridSize.ToVec2() * domain.Size;
            var center = IFlowOperator<Vec2, Vec3>.Default.Compute(t, tau, pos, dat.VelocityField);
            renderGrid.AtCoords(new Vec2i(i, j)).AverageVelocityMagnitude = center.AverageAlong((prev, cur) => ((prev.XY - cur.XY) / (cur.Z - prev.Z)).Length());
        });
    }

    public void OnImGuiEdit(GridVisualizer vis)
    {
        var dat = vis.GetRequiredWorldService<DataService>()!;
        ImGuiHelpers.SliderFloat("T", ref T, -dat.VelocityField.Period * 4, dat.VelocityField.Period * 4);
    }
}