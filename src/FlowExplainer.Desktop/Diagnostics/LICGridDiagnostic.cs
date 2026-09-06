using System.Numerics;
using FlowExplainer;
using ImGuiNET;

namespace FlowExplainer;

public class GaussianKernel : IKernel
{
    public double GetWeight(double dis)
    {
        double sigma = 0.3f;
        return (double)Math.Exp(-(dis * dis) / (2 * sigma * sigma));
    }
}

public class ConstantKernel : IKernel
{
    public double GetWeight(double dis)
    {
        return 1;
    }
}

public interface IKernel
{
    double GetWeight(double f);
}

public class LICGridDiagnostic : IGridDiagnostic
{
    public double arcLength = .1;
    public double t = 20;

    public bool UseCustomColoring => true;
    public bool UseUnsteady;

    private RegularGridVectorField<Vec2, Vec2i, double> licField = new(Vec2i.One, default, default);
    private IVectorField<Vec2, double> NoiseField = new NoiseField();
    public Artifact<IVectorField<Vec3, Vec2>>? VectorField;
    public bool modulateByTemp = false;


    public void UpdateGridData(GridVisualizer gridVisualizer, CancellationToken token)
    {
        if(VectorField == null)
            return;
        
        var renderGrid = gridVisualizer.RegularGrid;
        var domain = VectorField.Value.Domain;

        if (licField.GridSize != gridVisualizer.RegularGrid.GridSize)
            licField.Resize(gridVisualizer.RegularGrid.GridSize, gridVisualizer.RegularGrid.RectDomain);


        if (UseUnsteady)
            LIC.ComputeASteady(NoiseField, VectorField.Value, licField, t, 0.4f, arcLength, token);
        else
            LIC.ComputeSteady(NoiseField, VectorField.Value, licField, t, arcLength, token);

        var licMin = licField.Grid.Data.Min();
        var licMax = licField.Grid.Data.Max();
        ParallelGrid.For(renderGrid.GridSize, token, (i, j) =>
        {
            /*if(i > 2 || j >2)
                return;*/

            var lic = licField.Grid[new Vec2i(i, j)];
            ref var cell = ref renderGrid.Grid.AtCoords(new Vec2i(i, j));
            var pos = renderGrid.ToWorldPos(new Vec2(i + .5f, j + .5f));
            //  var temp = gridVisualizer.DataService.ScalerField.Evaluate(pos.Up(t));
            // var v = gridVisualizer.ScaleScaler(temp);
            var licMulti = (lic - licMin) / (licMax - licMin);
            /*if (modulateByTemp)
            {
                cell.Value = temp;
                licMulti += .5f;
                cell.Color = gridVisualizer.DataService.ColorGradient.GetCached(v) * new Color(licMulti, licMulti, licMulti, 1);
            }*/
            //  else
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
            renderGrid.Grid.Data[i].Color = new Color(double.Abs(renderGrid.Grid.Data[i].Value / 1), 0, 0);
        }*/
    }

    double Kernel(double dis)
    {
        double sigma = 0.3f;
        return (double)Math.Exp(-(dis * dis) / (2 * sigma * sigma));
    }

    public void OnImGuiEdit(GridVisualizer vis)
    {
        //double period = VectorField.Value.Domain.RectBoundary.Size.Last;
        //ImGuiHelpers.Slider("arc length", ref arcLength, 0, VectorField.Value.Domain.RectBoundary.Size.X / 5);
        ImGui.Checkbox("modulate by temp", ref modulateByTemp);
        OptionsManager.ArtifactSelector(vis.World, ref VectorField);
    }
}