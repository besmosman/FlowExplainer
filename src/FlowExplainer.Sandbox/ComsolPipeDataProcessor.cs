using Newtonsoft.Json;

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

        var T = ParseTemperatureField(Path.Combine(comsolDataPath, "Temperature.txt"));
        var T_noflow = ParseTemperatureField(Path.Combine(comsolDataPath, "Temperature_NoFlow.txt"));
        T.Save(outputDatasetFolder, T.DisplayName);
        T_noflow.Save(outputDatasetFolder, T_noflow.DisplayName);
        
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
                        temperatureField.Grid.AtCoords(new Vec3i(z, r, tI)) = temps[r] - 273.15;
                    }

                    z++;
                }
            }
        }

        for (int i = 0; i < temperatureField.Grid.Data.Length; i++)
            if (double.IsNaN(temperatureField.Grid.Data[i]))
                throw new Exception();

        temperatureField.DisplayName = Path.GetFileNameWithoutExtension(temperaturePath);
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