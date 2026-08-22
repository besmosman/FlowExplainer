using System.Data;
using System.Reflection;
using System.Text;
using ImGuiNET;
using MathNet.Numerics.Data.Matlab;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FlowExplainer;

public static class Scripting
{
    public static void Startup(World world)
    {
        /*new PipeDataProcesser().ProcessDatasets(
            "C:\\Users\\osman\\Downloads\\ScalarTransportCFDCaseStudies\\Data",
            "Datasets");*/

        world.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
        world.DataService.currentSelectedVectorField = "Velocity";
        world.DataService.currentSelectedScaler = "Convective Temperature";

        //LoadScene(world, new HeatSimScene());
        var op = new OperatorSplittedPullbackTestService();
        op.Initialize();
        //op.Save(5, .01);
        world.FlowExplainer.GetGlobalService<PresentationService>().LoadPresentation(new PullbackPresentation());
        world.FlowExplainer.GetGlobalService<PresentationService>().StartPresenting();
    }

    private static void LoadScene(World world, Scene scene)
    {
        world.FlowExplainer.GetGlobalService<SceneManager>().LoadScene(scene);
    }
}