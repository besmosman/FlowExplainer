using System.Xml.Linq;
using MathNet.Numerics.Data.Matlab;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowExplainer;

public class ComsolPipeDataProcessor
{
    public void ProcessDataset(string comsolDataPath, string outputDatasetFolder)
    {
        if (Directory.Exists(outputDatasetFolder))
            Directory.Delete(outputDatasetFolder, true);
        Directory.CreateDirectory(outputDatasetFolder);

        Dictionary<string, string> props = new Dictionary<string, string>();
        props.Add("Name", $"PipeFlowCase1");
        props.Add("Loader", $"PipeFlow");
        var ser = JsonConvert.SerializeObject(props, Formatting.Indented);
        File.WriteAllText(Path.Combine(outputDatasetFolder, "properties.json"), ser);

        var temperaturePath = Path.Combine(comsolDataPath, "Temperature.txt");
        var T = ParseTemperatureField(temperaturePath);
        T.Save(outputDatasetFolder, T.DisplayName);
    }


    private RegularGridVectorField<Vec3, Vec3i, double> ParseTemperatureField(string temperaturePath)
    {
        var lines = File.ReadAllLines(temperaturePath);
        var gridStart = lines.Index().First(l => l.Item.StartsWith("% Grid")).Index + 1;

        var rLine = SplitAndParse(lines[gridStart]);
        var zLine = SplitAndParse(lines[gridStart + 1]);

        var min = new Vec2(zLine.Min(), rLine.Min());
        var max = new Vec2(zLine.Max(), rLine.Max());

        var minTime = GetTimeFromLine(lines.First(l => l.StartsWith("% T (K) @ t=")));
        var maxTime = GetTimeFromLine(lines.Reverse().First(l => l.StartsWith("% T (K) @ t=")));

        var timesteps = lines.Reverse().Count(l => l.StartsWith("% T (K) @ t="));
        var gridSize = new Vec3i(zLine.Length, rLine.Length, timesteps);

        var rectDomain = new RectDomain<Vec3>(min.Up(minTime), max.Up(maxTime));
        var temperatureField = new RegularGridVectorField<Vec3, Vec3i, double>(gridSize, rectDomain);
        var dt = (maxTime - minTime) / (timesteps - 1);
        Array.Fill(temperatureField.Grid.Data, double.NaN);
        for (int l = 0; l < lines.Length; l++)
        {
            if (lines[l].StartsWith("% T (K) @ t="))
            {
                var t = GetTimeFromLine(lines[l]);
                var tI = (int)double.Round(t / dt);
                var startL = l + 1;
                int z = 0;
                while (lines.Length > startL + z && !lines[startL + z].StartsWith("%"))
                {
                    var temps = SplitAndParse(lines[startL + z]);
                    for (int r = 0; r < temps.Length; r++)
                    {
                        temperatureField.Grid.AtCoords(new Vec3i(z, r, tI)) = temps[r];
                    }

                    z++;
                }
            }
        }

        for (int i = 0; i < temperatureField.Grid.Data.Length; i++)
            if (double.IsNaN(temperatureField.Grid.Data[i]))
                throw new Exception();

        temperatureField.DisplayName = Path.GetFileNameWithoutExtension("Temperature");
        return temperatureField;
    }

    public double GetTimeFromLine(string s)
    {
        return double.Parse(s.Replace("% T (K) @ t=", ""));
    }

    public double[] SplitAndParse(string s)
    {
        return s.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(double.Parse).ToArray();
    }
}

public class PipeDataProcesser
{
    public void ProcessDatasets(string path, string outputFolder)
    {
        foreach (var p in Directory.EnumerateFiles(path).Take(1))
        {
            var caseName = Path.GetFileNameWithoutExtension(p).TrimStart("Temperature").TrimEnd("_NoLabels");
            ProcessDataset(p, Path.Combine(outputFolder, $"PipeSegmentCase{caseName}"));
        }
    }

    private static void ProcessDataset(string path, string outputFolder)
    {
        if (Directory.Exists(outputFolder))
            Directory.Delete(outputFolder, true);
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
        temperature.SaveToPath(Path.Combine(outputFolder, "T.vec3_vec1_field"));
        BinarySerializer.Save(Path.Combine(outputFolder, "u.vec3_vec2_field_analytical"), new AnalyticalVectorFieldSave
        {
            DisplayName = "u",
            TypeName = nameof(PoiseuilleFlowPipe),
        });

        Dictionary<string, string> props = new Dictionary<string, string>();
        var caseName = Path.GetFileNameWithoutExtension(path).TrimStart("Temperature").TrimEnd("_NoLabels");
        props.Add("Name", $"PipeSegment case {caseName}");
        var ser = JsonConvert.SerializeObject(props, Formatting.Indented);
        File.WriteAllText(Path.Combine(outputFolder, "properties.json"), ser);
    }
}