using MathNet.Numerics.Data.Matlab;

namespace FlowExplainer;

public class PipeScene : Scene
{
    public override void Load(FlowExplainer flowExplainer)
    {

        var properties = new Dictionary<string, string>();
        properties.Add("Name", "Pipe Flow");
        var dataset = new Dataset(properties, (d) =>
        {

            var path = @"C:\Users\20183493\Downloads\ScalarTransportCFDCaseStudies\Data\Temperature1a_NoLabels.mat";
            var mats = MatlabReader.List(path);
            var R = MatlabReader.Read<double>(path, "R");
            var Z = MatlabReader.Read<double>(path, "Z");
            var T = MatlabReader.Read<double>(path, "T");
            var dt = MatlabReader.Read<double>(path, "dt");
            if (dt[0, 0] != 1)
                throw new Exception();

            var Zs = Z.Enumerate().Distinct().Order().ToArray();
            var Rs = R.Enumerate().Distinct().Order().ToArray();
            var min = new Vec2(Z.Enumerate().Min(), R.Enumerate().Min());
            var max = new Vec2(Z.Enumerate().Max(), R.Enumerate().Max());
            var dim = new Vec3i(Zs.Length, Rs.Length, T.ColumnCount);
            var dom = new RectDomain<Vec3>(min.Up(0), max.Up(T.ColumnCount - 1));


            //matlab script gives:
            var d_s_out = 60.3e-3; // outer diameter steel pipe
            var delta_s = 2.9e-3; // thickness steel pipe
            var d_c_out = 125e-3; // outer diameter casing
            var delta_c = 3e-3; // thickness casing
            var rw = d_s_out / 2 - delta_s;



            RegularGridVectorField<Vec3, Vec3i, double> Temp = new RegularGridVectorField<Vec3, Vec3i, double>(dim, dom);

            
            Dictionary<double, int> ZtoIndex = new();
            for (int i = 0; i < Zs.Length; i++)
                ZtoIndex.Add(Zs[i], i);

            Dictionary<double, int> RtoIndex = new();
            for (int i = 0; i < Rs.Length; i++)
                RtoIndex.Add(Rs[i], i);


            for (int t = 0; t < T.ColumnCount; t++)
            {
                for (int i = 0; i < T.RowCount; i++)
                {
                    var temp = T[i, t];
                    Temp.AtCoords(new Vec3i(ZtoIndex[Z[i, 0]], RtoIndex[R[i, 0]], t)) = temp;
                }
            }
            d.ScalerFields.Add("T", Temp);
            

            var U = 1e-2;
            d.VectorFields.Add("Velocity", new ArbitraryField<Vec3, Vec2>(Temp.Domain,
                (x) =>
                {
               
                    var r = x.Y;

                    if (r < 0 || r > rw)
                        return new Vec2(0, 0);

                    double vZ = 2 * U * (1 - (r * r) / (rw * rw));

                    return new Vec2(vZ, 0);
                }));
            //d.VectorFields.Add("Velocity", new AnalyticalEvolvingVelocityField());
        });
        flowExplainer.GetGlobalService<DatasetsService>().Datasets.Add("Pipe Flow", dataset);
        var world = flowExplainer.GetGlobalService<WorldManagerService>().Worlds[0];
        world.GetWorldService<DataService>().SetDataset(dataset);
    }
}