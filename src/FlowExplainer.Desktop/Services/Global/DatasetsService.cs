using Newtonsoft.Json;

namespace FlowExplainer;

public class DatasetsService : GlobalService
{
    public Dictionary<string, Dataset> Datasets = new();

    public override void Initialize()
    {
        if (Directory.Exists("Datasets"))
            foreach (var datasetPath in Directory.GetDirectories("Datasets"))
            {
                string propertiesText = File.ReadAllText(Path.Combine(datasetPath, "properties.json"));
                var props = JsonConvert.DeserializeObject<Dictionary<string, string>>(propertiesText)!;
                props.TryAdd("Name", "?");
                props.TryGetValue("Loader", out string? loaderName);
                if (string.IsNullOrEmpty(loaderName))
                    loaderName = "Default";
                var dataset = new Dataset(props, DatasetLoaders.GetLoader(loaderName));
                dataset.Path = datasetPath;
                Datasets.Add(dataset.Name, dataset);
            }
    }

    public override void Draw()
    {
    }
}