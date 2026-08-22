using MathNet.Numerics.Data.Matlab;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowExplainer;

public class PipeDataProcesser
{
    public void ProcessDatasets(string path, string outputFolder)
    {
        foreach (var p in Directory.EnumerateFiles(path).Take(1))
        {
            var caseName = Path.GetFileNameWithoutExtension(p).TrimStart("Temperature").TrimEnd("_NoLabels");
            ProcessDataset(p, Path.Combine(outputFolder,$"PipeSegmentCase{caseName}"));
        }
    }

    private static void ProcessDataset(string path, string outputFolder)
    {
        if(Directory.Exists(outputFolder))
            Directory.Delete(outputFolder,true);
        Directory.CreateDirectory(outputFolder);
        var mats = MatlabReader.List(path);
        var R = MatlabReader.Read<double>(path, "R");
        var Z = MatlabReader.Read<double>(path, "Z");
        var T = MatlabReader.Read<double>(path, "T");
        var dt = MatlabReader.Read<double>(path, "dt");

        if (Math.Abs(dt[0, 0] - 1) > .0001)
            throw new NotImplementedException();

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


        RegularGridVectorField<Vec3, Vec3i, double> temperature = new RegularGridVectorField<Vec3, Vec3i, double>(dim, dom);

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
                temperature.AtCoords(new Vec3i(ZtoIndex[Z[i, 0]], RtoIndex[R[i, 0]], t)) = temp;
            }
        }

        temperature.DisplayName = "T";
        temperature.Save(Path.Combine(outputFolder, "T.vec3_vec1_field"));
        BinarySerializer.Save(Path.Combine(outputFolder, "u.vec3_vec2_field_analytical"), new AnalyticalVectorFieldSave
        {
            DisplayName = "u",
            TypeName = nameof(PipeFlow),
        });

        Dictionary<string, string> props = new Dictionary<string, string>();
        var caseName = Path.GetFileNameWithoutExtension(path).TrimStart("Temperature").TrimEnd("_NoLabels");
        props.Add("Name", $"PipeSegment case {caseName}");
        var ser = JsonConvert.SerializeObject(props, Formatting.Indented);
        File.WriteAllText(Path.Combine(outputFolder, "properties.json"), ser);
    }
}