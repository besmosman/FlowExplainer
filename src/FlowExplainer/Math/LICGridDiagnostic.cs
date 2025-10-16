using System.Numerics;
using FlowExplainer;
using ImGuiNET;

namespace FlowExplainer;

public class GaussianKernel : IKernel
{
    public float GetWeight(float dis)
    {
        float sigma = 0.3f;
        return (float)Math.Exp(-(dis * dis) / (2 * sigma * sigma));
    }
}

public class ConstantKernel : IKernel
{
    public float GetWeight(float dis)
    {
        return 1;
    }
}

public interface IKernel
{
    float GetWeight(float f);
}

public class LICGridDiagnostic : IGridDiagnostic
{
    public float arcLength = 1;
    public bool UseCustomColoring => true;
    public bool UseUnsteady;

    private RegularGridVectorField<Vec2, Vec2i, float> licField = new(Vec2i.One, default, default);
    private IVectorField<Vec2, float> NoiseField = new NoiseField();
    public bool modulateByTemp = false;


    public void UpdateGridData(GridVisualizer gridVisualizer)
    {
        var renderGrid = gridVisualizer.RegularGrid;

        var dat = gridVisualizer.GetRequiredWorldService<DataService>()!;
        var domain = dat.VectorField.Domain;



        var t = dat.SimulationTime;
        var tau = dat.SimulationTime + arcLength;

        if (licField.GridSize != gridVisualizer.RegularGrid.GridSize)
            licField.Resize(gridVisualizer.RegularGrid.GridSize, gridVisualizer.RegularGrid.RectDomain);


        if (UseUnsteady)
            LIC.ComputeASteady(NoiseField, dat.VectorField, licField, t, 0.4f, arcLength);
        else
            LIC.ComputeSteady(NoiseField, dat.VectorField, licField, t, arcLength);

        var licMin = licField.Grid.Data.Min();
        var licMax = licField.Grid.Data.Max();
        ParallelGrid.For(renderGrid.GridSize, (i, j) =>
        {
            /*if(i > 2 || j >2)
                return;*/

            var lic = licField.Grid[new Vec2i(i, j)];
            ref var cell = ref renderGrid.Grid.AtCoords(new Vec2i(i, j));
            var pos = renderGrid.ToWorldPos(new Vec2(i + .5f, j + .5f));
            var temp = dat.TempratureField.Evaluate(pos.Up(t));
            var v = gridVisualizer.ScaleScaler(temp);
            var licMulti = (lic - licMin) / (licMax - licMin);
            if (modulateByTemp)
            {
                cell.Value = temp;
                licMulti += .5f;
                cell.Color = dat.ColorGradient.GetCached(v) * new Color(licMulti, licMulti, licMulti, 1);
            }
            else
            {
                cell.Value = licMulti;
                cell.Color = new Color(licMulti, licMulti, licMulti, 1);
            }
        });
        /*for (int i = 0; i < licField.Grid.Data.Length; i++)
        {
            var lic = licField.Grid.Data[i];
            renderGrid.Grid.Data[i].Value = dat.TempratureField.Evaluate();
            var v = gridVisualizer.ScaleScaler(renderGrid.Grid.Data[i].Value);
            var heat = dat.ColorGradient.Get(v);
            renderGrid.Grid.Data[i].Color = new Color(float.Abs(renderGrid.Grid.Data[i].Value / 1), 0, 0);
        }*/
    }

    float Kernel(float dis)
    {
        float sigma = 0.3f;
        return (float)Math.Exp(-(dis * dis) / (2 * sigma * sigma));
    }

    public void OnImGuiEdit(GridVisualizer vis)
    {
        var dat = vis.GetRequiredWorldService<DataService>()!;
        float period = dat.VectorField.Domain.RectBoundary.Size.Last;
        ImGuiHelpers.SliderFloat("arc length", ref arcLength, 0, dat.VectorField.Domain.RectBoundary.Size.X / 5);
        ImGui.Checkbox("modulate by temp", ref modulateByTemp);
    }
}