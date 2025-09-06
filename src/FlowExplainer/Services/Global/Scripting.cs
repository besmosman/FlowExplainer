namespace FlowExplainer;

public static class Scripting
{
    public static void Startup(World world)
    {
        var gridVisualizer = world.GetWorldService<GridVisualizer>();
        var dataService = world.GetWorldService<DataService>();
        gridVisualizer.Enable();
        dataService.ColorGradient = Gradients.Grayscale;
        gridVisualizer.SetGridDiagnostic(new LICGridDiagnostic());
        
        
        
    }
}