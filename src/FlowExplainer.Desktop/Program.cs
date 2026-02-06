using System.Globalization;
using FlowExplainer;

DedicatedGraphics.InitializeDedicatedGraphics();
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
Config.Load("config.json");
var app = new FlowExplainer.FlowExplainer();
app.AddDefaultGlobalServices();
var world = app.GetGlobalService<WorldManagerService>().Worlds.First();

var datasets = app.GetGlobalService<DatasetsService>().Datasets;
if (datasets.Count == 0)
    AddExampleDataset(app);
else
    world.DataService.SetDataset(datasets.First().Key);

world.AddVisualisationService<AxisVisualizer>();
app.Run();


void AddExampleDataset(FlowExplainer.FlowExplainer flowExplainer)
{

    var dataset = new Dataset(new()
        {
            {
                "Name", "Test Data"
            },
        },
        (d) =>
        {
            d.VectorFields.Add("Double Gyre", new SpeetjensVelocityField()
            {
                epsilon = 0.1
            });
            d.ScalerFields.Add("Scaler Field", new ArbitraryField<Vec3, double>(new RectDomain<Vec3>(Vec3.Zero, new Vec3(1, .5, 1)),
                (x) => Vec2.Distance(x.XY, new Vec2(.5f + x.Z, .25))));
        });

    flowExplainer.GetGlobalService<DatasetsService>().Datasets.Add(dataset.Name, dataset);
    var world = flowExplainer.GetGlobalService<WorldManagerService>().Worlds[0];
    world.DataService.SetDataset(dataset.Name);
    world.DataService.currentSelectedVectorField = "Double Gyre";
    world.DataService.currentSelectedScaler = "Scaler Field";
}