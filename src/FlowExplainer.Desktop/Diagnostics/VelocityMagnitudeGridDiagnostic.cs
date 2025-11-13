using System.Runtime.InteropServices;

namespace FlowExplainer;

/*
public class VelocityMagnitudeGridDiagnostic : IGridDiagnostic
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VelData
    {
        public double Magnitude;
    }
    
    public Type DataType => typeof(VelData);

    public void UpdateGridData(GridVisualizer vis)
    {
        var renderGrid = vis.GetRenderGrid<VelData>();

        var dat = vis.GetRequiredWorldService<DataService>()!;
        var domain = dat.VelocityField.Domain;
        renderGrid.SetColorFunction(
            (gl) => gl.ColorGradient(gl.Dat.Magnitude));
        
        Parallel.For(0, renderGrid.GridSize.X * renderGrid.GridSize.Y, c =>
        {
            var i = c % renderGrid.GridSize.X;
            var j = c / renderGrid.GridSize.X;
            var pos =  domain.Boundary.Reduce<Vec2>().Relative(new Vec2(i, j) / renderGrid.GridSize.ToVec2());
            renderGrid.AtCoords(new Vec2i(i, j)).Magnitude = dat.VelocityField.Evaluate(pos.Up(dat.SimulationTime)).Length()/10;
        });
    }

    public void OnImGuiEdit(GridVisualizer vis)
    {
    }
}*/