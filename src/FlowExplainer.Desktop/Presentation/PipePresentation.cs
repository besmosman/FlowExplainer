namespace FlowExplainer;

public class PipePresentation : NewPresentation
{
    public override void Draw()
    {
        var datasets = Presi.FlowExplainer.GetGlobalService<DatasetsService>().Datasets;
        if (BeginSlide())
        {
            Presi.Text("Pipe Flow Case", new Vec2(.5, .5), .05, true, Color.White);
        }

        PipeSlides("PipeFlow Original", 0, 40);
        if (BeginSlide())
        {
            Presi.Text("Slower fluid for more conduction", new Vec2(.5, .56), .04, true, Color.White);
            Presi.Text("U_mean /= 0.01", new Vec2(.5, .42), .03, true, Color.Green);
            Presi.Text("step size *= 100", new Vec2(.5, .34), .03, true, Color.Green);
            Presi.Text("[0s..100s] -> [0s..10000s]", new Vec2(.5, .26), .03, true, Color.Green);
        }

        PipeSlides("PipeFlow Slow", 0, 40);

        if (BeginSlide())
        {
            Presi.Text("Boundary Condition", new Vec2(.5, .56), .04, true, Color.White);
            Presi.Text("Temperature BC instead of Inlet BC", new Vec2(.5, .42), .03, true, Color.Green);
            /*Presi.Text("step size *= 100", new Vec2(.5, .34), .03, true, Color.Green);
            Presi.Text("[0s..100s] -> [0s..10000s]", new Vec2(.5, .26), .03, true, Color.Green);*/
        }
        PipeSlides("PipeFlow Slow+BC", 0, 40);
        
        if (BeginSlide())
        {
            Presi.Text("Different Inlet Temperature", new Vec2(.5, .56), .04, true, Color.White);
            Presi.Text("T_inlet = T_in + waveform(-50,50)", new Vec2(.5, .42), .03, true, Color.Green);
            /*Presi.Text("step size *= 100", new Vec2(.5, .34), .03, true, Color.Green);
            Presi.Text("[0s..100s] -> [0s..10000s]", new Vec2(.5, .26), .03, true, Color.Green);*/
        }
        PipeSlides("PipeFlow Slow+Cool", -50, 50);
    }

    public double t = 0;

    public void PipeSlides(string datasetName, double min, double max)
    {
        var datasets = Presi.FlowExplainer.GetGlobalService<DatasetsService>().Datasets;
        var dataset = datasets[datasetName];
        if (!dataset.Loaded)
        {
            dataset.Load(dataset);
            dataset.Loaded = true;
        }
        var temp = dataset.GetVectorField<Vec3, double>("Temperature");
        var tempNoflow = dataset.GetVectorField<Vec3, double>("Temperature_NoFlow");
        var tempMinmax = FindMinMax(temp);
        var tempNoflowMinmax = FindMinMax(tempNoflow);

        if (BeginSlide())
        {
            var view = DrawWorldPanel(new Vec2(.5, .77), new Vec2(1, .3) * 1, zoom: 1.35, load: w =>
            {
                w.DataService.SetDataset(datasetName);
                w.DataService.ColorGradient = Gradients.GetGradient("Magma");
                var axis = w.AddVisualisationService<AxisVisualizer>();
                axis.AutoAxis();
                axis.VerticalAxis.Steps = 3;
                axis.HorizontalAxis.Steps = 6;
                axis.DrawTitle = false;
                var grid = w.AddVisualisationService<GridVisualizer>();
                var s = grid.SetGridDiagnostic<Scaler3DGridDiagnostic>();
                grid.AutoScale = false;
                grid.min = tempMinmax.Min;
                grid.max = tempMinmax.Max;
                s.scalerField = w.GetArtifact<IVectorField<Vec3, double>>("dataset/temperature");
                s.Time = 40;
            });

            var view2 = DrawWorldPanel(new Vec2(.5, .3), new Vec2(1, .3) * 1, zoom: 1.35, load: w =>
            {
                w.DataService.SetDataset(datasetName);
                w.DataService.ColorGradient = Gradients.GetGradient("Magma");
                var axis = w.AddVisualisationService<AxisVisualizer>();
                axis.AutoAxis();
                axis.VerticalAxis.Steps = 3;
                axis.HorizontalAxis.Steps = 6;
                axis.DrawTitle = false;
                var grid = w.AddVisualisationService<GridVisualizer>();
                grid.AutoScale = false;
                grid.max = max;
                grid.min = min;
                grid.min = tempNoflowMinmax.Min;
                grid.max = tempNoflowMinmax.Max;
                var s = grid.SetGridDiagnostic<Scaler3DGridDiagnostic>();

                s.scalerField = w.GetArtifact<IVectorField<Vec3, double>>("dataset/temperature_noflow");
                s.Time = 40;
            });

            view.World.GetWorldService<GridVisualizer>().Continous = true;
            view2.World.GetWorldService<GridVisualizer>().Continous = true;
            var scaler = (Scaler3DGridDiagnostic)view.World.GetWorldService<GridVisualizer>().diagnostic;
            var scaler2 = (Scaler3DGridDiagnostic)view2.World.GetWorldService<GridVisualizer>().diagnostic;
            scaler.Time = t;
            scaler2.Time = t;
            Presi.Slider("Time", ref t, 0, view.World.DataService.ScalerField.Domain.RectBoundary.Max.Z, new Vec2(.5, .05), .6);
            //Title(title);
            Presi.Text("Temperature", new Vec2(.5, .95), .02, true, Color.White);
            if(min < 0)
                Presi.Text("Temperature (reference)", new Vec2(.5, .45), .02, true, Color.White);
            else
            Presi.Text("Temperature (no flow)", new Vec2(.5, .45), .02, true, Color.White);
        }

        if (BeginSlide())
        {
            var view = DrawWorldPanel(new Vec2(.5, .77), new Vec2(1, .3) * 1, zoom: 1.35, load: w =>
            {
                w.DataService.SetDataset(datasetName);
                w.DataService.ColorGradient = Gradients.GetGradient("Magma");
                var ltf = w.AddVisualisationService<LagrangianTransportFormalismService>();
                ltf.RecomputePipeCaseLTF();
                var axis = w.AddVisualisationService<AxisVisualizer>();
                axis.AutoAxis();
                axis.VerticalAxis.Steps = 3;
                axis.HorizontalAxis.Steps = 6;
                axis.DrawTitle = false;

                var grid = w.AddVisualisationService<GridVisualizer>();
                grid.AutoScale = false;
                grid.max = max;
                grid.min = min;
                var s = grid.SetGridDiagnostic<Scaler3DGridDiagnostic>();

                s.scalerField = w.GetArtifact<IVectorField<Vec3, double>>("ltf/temperature/convective");
                s.Time = 40;
            });


            var view2 = DrawWorldPanel(new Vec2(.5, .3), new Vec2(1, .3) * 1, zoom: 1.35, load: w =>
            {
                w.DataService.SetDataset(datasetName);
                w.DataService.ColorGradient = Gradients.GetGradient("Parula");
                var ltf = w.AddVisualisationService<LagrangianTransportFormalismService>();
                ltf.RecomputePipeCaseLTF();
                var axis = w.AddVisualisationService<AxisVisualizer>();
                axis.AutoAxis();
                axis.VerticalAxis.Steps = 3;
                axis.HorizontalAxis.Steps = 6;
                axis.DrawTitle = false;
                var grid = w.AddVisualisationService<GridVisualizer>();
                grid.Bilinear = false;
                grid.min = min;
                grid.max = max;
                
                var slicer = w.AddVisualisationService<TimeSliceService>();
                var fluxSlice = (Artifact<IVectorField<Vec2, Vec2>>)slicer.RegisterSlicedArtifact(w.GetArtifact<IVectorField<Vec3, Vec2>>("ltf/flux/total"));
                var s = grid.SetGridDiagnostic<MagnitudeGridDiagnostic>();
                s.vectorfield = fluxSlice;

                var arrows = w.AddVisualisationService<ArrowVisualizer>();
                arrows.Vectorfield = fluxSlice;
                arrows.GridCells = 300;
                arrows.colorByGradient = false;
            });

            view2.World.GetWorldService<TimeSliceService>().sliceValue = t;
            var scaler = (Scaler3DGridDiagnostic)view.World.GetWorldService<GridVisualizer>().diagnostic;
            scaler.Time = t;

            Presi.Text("Convective Temperature", new Vec2(.5, .95), .02, true, Color.White);
            Presi.Text("Net Total Flux (direction / magnitude)", new Vec2(.5, .45), .02, true, Color.White);

            Presi.Slider("Time", ref t, 0, view.World.DataService.ScalerField.Domain.RectBoundary.Max.Z, new Vec2(.5, .05), .6);
        }
    }

    private Dictionary<IVectorField<Vec3, double>, Rect<Vec1>> cached = new();
    private Rect<Vec1> FindMinMax(IVectorField<Vec3, double> temp)
    {
        if (cached.ContainsKey(temp))
        {
           return cached[temp];
        }
        var vec = (RegularGridVectorField<Vec3, Vec3i, double>)temp;
        var r =  new Rect<Vec1>(vec.Grid.Data.Min(), vec.Grid.Data.Max());
        cached[temp] = r;
        return r;
    }
}