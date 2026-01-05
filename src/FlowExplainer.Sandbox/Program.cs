using System.Globalization;
using FlowExplainer;

DedicatedGraphics.InitializeDedicatedGraphics();
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
Config.Load("config.json");
var app = new FlowExplainer.FlowExplainer();
app.AddDefaultGlobalServices();
Scripting.Startup(app.GetGlobalService<WorldManagerService>().Worlds[0]);
app.Run();