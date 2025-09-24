namespace FlowExplainer;

public class HeatStructuresPresentation : Presentation
{
    public override Slide[] GetSlides()
    {
        return
        [
            new TitleSlide(),
            new HeatSubtractSlide(),
            new VectorFieldsSlide(),
            new HeatStructuresIntroSlide(),
            new HeatStructureSlide
            {
                Title = "Diffusion Sinks",
                ScalerField = "Diffusion Sinks",
                VectorField = "Diffusion Flux",
            },
            new HeatStructureSlide
            {
                Title = "Diffusion Sources",
                ScalerField = "Diffusion Sources",
                VectorField = "Diffusion Flux",
            },
            new HeatStructureSlide
            {
                Title = "Convection Sinks",
                ScalerField = "Convection Sinks",
                VectorField = "Convection Flux",
            },
            new HeatStructureSlide
            {
                Title = "Convection Sources",
                ScalerField = "Convection Sources",
                VectorField = "Convection Flux",
            },
        ];
    }

    public class HeatStructureSlide : Slide
    {
        public string ScalerField;
        public string VectorField;
        public string Title;

        public override void OnEnter()
        {
            w0.GetWorldService<DataService>().currentSelectedScaler = ScalerField;
            w0.GetWorldService<DataService>().currentSelectedVectorField = VectorField;
            w0.GetWorldService<GridVisualizer>().SetGridDiagnostic(new TemperatureGridDiagnostic());
            w0.GetWorldService<GridVisualizer>().Enable();
            w0.GetWorldService<GridVisualizer>().Continous = true;
            w0.GetWorldService<DataService>().TimeMultiplier = 0f;
            var flow = w0.GetWorldService<FlowDirectionVisualization>();
            flow.amount = 3000;
            if (!flow.IsEnabled)
            {
                flow.opacity = .3f;
                flow.speed = 2;
                flow.Initialize();
            }
            base.OnEnter();
        }
        public override void Draw()
        {
            LayoutMain();
            Title(Title);
            Presi.ViewPanel("v0", new Vec2(Presi.CanvasCenter.X, Presi.CanvasCenter.Y), new Vec2(1, .5f) * Presi.CanvasSize.X * .9f, .8f);
            ref float simulationTime = ref w0.GetWorldService<DataService>().SimulationTime;
            Presi.Slider($"time = {simulationTime:N2}", ref simulationTime, 0, 1, new Vec2(Presi.CanvasCenter.X, 100f), 500);
            var flow = w0.GetWorldService<FlowDirectionVisualization>();
            bool isEnabled = flow.IsEnabled;
            Presi.Checkbox("Show Flow", ref isEnabled, new Vec2(300, 100));

            if (flow.IsEnabled != isEnabled)
            {
                if (flow.IsEnabled)
                    flow.Disable();
                else
                    flow.Enable();
            }
            base.Draw();
        }
    }

    public class HeatStructuresIntroSlide : Slide
    {
        public override void Draw()
        {
            LayoutMain();
            Title("Heat Sources and Sinks");
            MainParagraph(
                @"
Heat Sinks/Sources:
- Instant
- Over timerange
");
            base.Draw();
        }
    }

    private class TitleSlide : Slide
    {
        public override void Draw()
        {
            LayoutTitle();
            TitleTitle("Progress", "Heat structures and LCS (26-09-2025)");
            base.Draw();
        }
    }

    private class VectorFieldsSlide : Slide
    {
        public override void OnLeave()
        {
            foreach (var w in worlds)
            {
                var gridVisualizer = w.GetWorldService<GridVisualizer>();
                w.GetWorldService<FlowDirectionVisualization>().Disable();
            }
            base.OnLeave();
        }
        public override void OnEnter()
        {
            foreach (var w in worlds)
            {
                var dat = w.GetWorldService<DataService>();
                dat.TimeMultiplier = 0f;
                dat.SimulationTime = .8f;
                var gridVisualizer = w.GetWorldService<GridVisualizer>();
                w.GetWorldService<FlowDirectionVisualization>().amount = 2000;
                w.GetWorldService<FlowDirectionVisualization>().speed = 3;
                w.GetWorldService<FlowDirectionVisualization>().thickness = .004f;
                w.GetWorldService<FlowDirectionVisualization>().opacity = .6f;
                w.GetWorldService<FlowDirectionVisualization>().Enable();
                w.GetWorldService<DataService>().currentSelectedScaler = "Convective Temperature";
                gridVisualizer.Disable();
                gridVisualizer.MarkDirty = true;
                gridVisualizer.SetGridDiagnostic(new TemperatureGridDiagnostic());
                gridVisualizer.Continous = false;
            }
            w0.GetWorldService<DataService>().currentSelectedVectorField = "Velocity";
            w1.GetWorldService<DataService>().currentSelectedVectorField = "Diffusion Flux";
            w2.GetWorldService<DataService>().currentSelectedVectorField = "Convection Flux";
            w3.GetWorldService<DataService>().currentSelectedVectorField = "Total Flux";
            base.OnEnter();
        }
        public override void Draw()
        {
            LayoutMain();
            Title("Vector Fields");
            Presi.ViewPanel("v0", new Vec2(Presi.CanvasCenter.X - 481, 780), new Vec2(1, .5f) * Presi.CanvasSize.X * .5f, .8f);
            Presi.ViewPanel("v1", new Vec2(Presi.CanvasCenter.X + 481, 780), new Vec2(1, .5f) * Presi.CanvasSize.X * .5f, .8f);
            Presi.ViewPanel("v2", new Vec2(Presi.CanvasCenter.X - 481, 260), new Vec2(1, .5f) * Presi.CanvasSize.X * .5f, .8f);
            Presi.ViewPanel("v3", new Vec2(Presi.CanvasCenter.X + 481, 260), new Vec2(1, .5f) * Presi.CanvasSize.X * .5f, .8f);
            Presi.Text("Velocity", new Vec2(Presi.CanvasCenter.X - 470, 1020), 64, true, Color.White);
            Presi.Text("Diffusion Flux", new Vec2(Presi.CanvasCenter.X + 470, 1020), 64, true, Color.White);
            Presi.Text("Convection Flux", new Vec2(Presi.CanvasCenter.X - 470, 490), 64, true, Color.White);
            Presi.Text("Total Flux", new Vec2(Presi.CanvasCenter.X + 470, 490), 64, true, Color.White);
            base.Draw();
        }
    }

    public class HeatSubtractSlide : Slide
    {
        public override void OnEnter()
        {
            Presi.GetView("v0").World.GetWorldService<AxisVisualizer>().DrawWalls = false;
            World[] ws = [w0, w1, w2];
            foreach (var w in ws)
            {
                var dat = w.GetWorldService<DataService>();
                dat.TimeMultiplier = .1f;
                dat.SimulationTime = .0000001f;
                dat.SimulationTime = .0000001f;
                var gridVisualizer = w.GetWorldService<GridVisualizer>();
                gridVisualizer.Enable();
                gridVisualizer.SetGridDiagnostic(new TemperatureGridDiagnostic());
                dat.currentSelectedVectorField = "Velocity";
            }

            w0.GetWorldService<FlowFieldVisualizer>().Disable();
            w1.GetWorldService<DataService>().currentSelectedScaler = "Total Temperature";
            w0.GetWorldService<DataService>().currentSelectedScaler = "Convective Temperature";
            w2.GetWorldService<DataService>().currentSelectedScaler = "No Flow Temperature";
            base.Load();
        }

        public override void Draw()
        {
            LayoutMain();
            Title("Temperature Fields");
            Presi.ViewPanel("v1", new Vec2(Presi.CanvasCenter.X - 481, 780), new Vec2(1, .5f) * Presi.CanvasSize.X * .5f, .8f);
            Presi.ViewPanel("v2", new Vec2(Presi.CanvasCenter.X + 481, 780), new Vec2(1, .5f) * Presi.CanvasSize.X * .5f, .8f);
            Presi.ViewPanel("v0", new Vec2(Presi.CanvasCenter.X, 260), new Vec2(1, .5f) * Presi.CanvasSize.X * .5f, .8f);
            Presi.Text("Temperature with Flow", new Vec2(Presi.CanvasCenter.X - 470, 1020), 64, true, Color.White);
            Presi.Text("Temperature without Flow", new Vec2(Presi.CanvasCenter.X + 470, 1020), 64, true, Color.White);
            Presi.Text("Convective Temperature", new Vec2(Presi.CanvasCenter.X, 490), 64, true, Color.White);
            Presi.Text("-", new Vec2(Presi.CanvasCenter.X - 30, 710), 204, true, Color.White);
            Presi.Text("=", new Vec2(Presi.CanvasCenter.X - 530, 250), 204, true, Color.White);
            //Presi.Text("u(x,y,t) = 0", new Vec2(Presi.CanvasCenter.X, 220), 80, true, Color.White);
            base.Draw();
        }
    }


    public override void Setup(FlowExplainer flowExplainer)
    {
        var manager = flowExplainer.GetGlobalService<WorldManagerService>();

        var w0 = manager.NewWorld();
        var w1 = manager.NewWorld();
        var w2 = manager.NewWorld();
        var w3 = manager.NewWorld();

        Scripting.SetGyreDataset(w0);
        Scripting.SetGyreDataset(w1);
        Scripting.SetGyreDataset(w2);
        Scripting.SetGyreDataset(w3);
        w0.GetWorldService<DataService>().ScalerFields.Add("Diffusion Sources", RegularGridVectorField<Vec3, Vec3i, float>.Load("diffusion-sources.field"));
        w0.GetWorldService<DataService>().ScalerFields.Add("Diffusion Sinks", RegularGridVectorField<Vec3, Vec3i, float>.Load("diffusion-sinks.field"));
        w0.GetWorldService<DataService>().ScalerFields.Add("Convection Sources", RegularGridVectorField<Vec3, Vec3i, float>.Load("convection-sources.field"));
        w0.GetWorldService<DataService>().ScalerFields.Add("Convection Sinks", RegularGridVectorField<Vec3, Vec3i, float>.Load("convection-sinks.field"));

        w0.GetWorldService<DataService>().currentSelectedVectorField = "Diffusion Flux";
        w0.GetWorldService<DataService>().currentSelectedVectorField = "Velocity";
        w1.GetWorldService<DataService>().currentSelectedScaler = "Total Temperature";


        var presentationService = flowExplainer.GetGlobalService<PresentationService>();
        presentationService.Presi.GetView("v0").World = w0;
        presentationService.Presi.GetView("v1").World = w1;
        presentationService.Presi.GetView("v2").World = w2;
        presentationService.Presi.GetView("v3").World = w3;
    }
}