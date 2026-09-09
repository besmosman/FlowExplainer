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

        /*new ComsolPipeDataProcessor().ProcessDataset(
            "C:\\Users\\osman\\Documents\\ComsolResults\\PipeFlow Original", 1);*/

        /*new ComsolPipeDataProcessor().ProcessDataset(
            "C:\\Users\\osman\\Documents\\ComsolResults\\PipeFlow Slow", .01);*/

        /*new ComsolPipeDataProcessor().ProcessDataset(
     "C:\\Users\\osman\\Documents\\ComsolResults\\PipeFlow Slow+BC", .01);*/

        /*new ComsolPipeDataProcessor().ProcessDataset(
   "C:\\Users\\osman\\Documents\\ComsolResults\\PipeFlow Slow+Cool", .01);*/

        world.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
        world.DataService.currentSelectedVectorField = "Velocity";
        world.DataService.currentSelectedScaler = "Convective Temperature";

        //var op = new OperatorSplittedPullbackTestService();
        //op.Initialize();
        //op.Save(5, .01);
        if (false)
        {
            LoadScene(world, new PipeFlowSpacetimeDensityStructureScene());
        }
        else
        {
            LoadScene(world, new PipeFlowSpacetimeDensityStructureScene());
            world.FlowExplainer.GetGlobalService<PresentationService>().LoadPresentation(new PipePresentation());
            world.FlowExplainer.GetGlobalService<PresentationService>().StartPresenting();
        }
    }

    private static void LoadScene(World world, Scene scene)
    {
        world.FlowExplainer.GetGlobalService<SceneManager>().LoadScene(scene);
    }
}