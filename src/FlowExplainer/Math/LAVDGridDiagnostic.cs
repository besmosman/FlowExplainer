using System.Runtime.InteropServices;

namespace FlowExplainer;

public class LAVDGridDiagnostic : IGridDiagnostic
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VortData
    {
        public Vec2 StartPosition;
        public Vec2 FinalPosition;
        public float Vorticity;
    }

    public float T = 1;
    private VortData[] Data;

    public void UpdateGridData(GridVisualizer gridVisualizer)
    {
        //var renderGrid = Data;

        var renderGrid = gridVisualizer.RegularGrid.Grid;
        var dat = gridVisualizer.GetWorldService<DataService>()!;
        var domain = dat.VectorField.Domain;

        var t = dat.SimulationTime;
        var tau = dat.SimulationTime + T;
        if (Data == null || Data.Length != renderGrid.Data.Length)
            Data = new VortData[renderGrid.Data.Length];

        Parallel.For(0, renderGrid.GridSize.X * renderGrid.GridSize.Y, c =>
        {
            var i = c % renderGrid.GridSize.X;
            var j = c / renderGrid.GridSize.X;
            ref var center = ref renderGrid.AtCoords(new Vec2i(i, j));

            float dx = domain.Boundary.Size.X / renderGrid.GridSize.X;
            float dy = domain.Boundary.Size.Y / renderGrid.GridSize.Y;

            // Neighbor indices with clamping
            int ip = Math.Min(i + 1, renderGrid.GridSize.X - 1);
            int im = Math.Max(i - 1, 0);
            int jp = Math.Min(j + 1, renderGrid.GridSize.Y - 1);
            int jm = Math.Max(j - 1, 0);

            var v_right = dat.VectorField.Evaluate(gridVisualizer.RegularGrid.ToWorldPos(new Vec2(ip, j)).Up(t));
            var v_left  = dat.VectorField.Evaluate(gridVisualizer.RegularGrid.ToWorldPos(new Vec2(im, j)).Up(t));
            var v_up    = dat.VectorField.Evaluate(gridVisualizer.RegularGrid.ToWorldPos(new Vec2(i, jp)).Up(t));
            var v_down  = dat.VectorField.Evaluate(gridVisualizer.RegularGrid.ToWorldPos(new Vec2(i, jm)).Up(t));

            var du_dy = (v_up.X - v_down.X) / (2 * dy);
            var dv_dx = (v_right.Y - v_left.Y) / (2 * dx);

            var vorticity = dv_dx - du_dy;
            Data[c].Vorticity = vorticity;
            center.Value = float.Abs(vorticity/10f);
        });

        
        var averageVorticity = Data.Average(c => c.Vorticity);
        for (int i = 0; i < Data.Length; i++)
            Data[i].Vorticity -= averageVorticity;
            

        /*Parallel.For(0, renderGrid.GridSize.X * renderGrid.GridSize.Y, c =>
        {
            var i = c % renderGrid.GridSize.X;
            var j = c / renderGrid.GridSize.X;
            renderGrid.AtCoords(new Vec2i(i,j)).Value = Data[c].Vorticity*100;
        });*/
    }

    public void OnImGuiEdit(GridVisualizer vis)
    {
        var dat = vis.GetRequiredWorldService<DataService>()!;
        float period = dat.VectorField.Domain.Boundary.Size.Last;
        if (ImGuiHelpers.SliderFloat("T", ref T, -period * 1, period * 1))
            vis.MarkDirty = true;
    }
}