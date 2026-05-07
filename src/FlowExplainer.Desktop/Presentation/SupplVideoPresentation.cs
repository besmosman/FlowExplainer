namespace FlowExplainer;

public class SupplVideoPresentation : NewPresentation
{
    const string AssetsPath = "Assets/Images/Presentations/density-paper-cluster";
    private Texture michel0 { get; } = new ImageTexture(Path.Combine(AssetsPath, "michel0.png"));
    private Texture seed { get; } = new ImageTexture(Path.Combine(AssetsPath, "seed.png"));
    private Texture convection { get; } = new ImageTexture(Path.Combine(AssetsPath, "convection.png"));

    public double t;
    public override void Draw()
    {
        if (BeginSlide())
        {
            Presi.Text("Visualizing Lagrangian Heat Transport Paths and \r\n Density Structures in Unsteady Heat Transfer", 
                new Vec2(.5, .9), .034, true, Color.White);
            Presi.Text("Besm Osman, Andrei Jalba, Michel Speetjens and Anna Vilanova", new Vec2(.5, .65), .02, true, Color.White);
            DrawWorldPanel(new Vec2(0.5, .3), new Vec2(1, .5) * .7, zoom: 1,
                load: (world) =>
                {
                    var data = world.GetWorldService<DataService>();
                    data.SetDataset("Double Gyre EPS=0.1, Pe=100");
                    data.TimeMultiplier = 0f;
                    data.SimulationTime = 1;
                    data.currentSelectedScaler = "Convective Temperature";
                    data.currentSelectedVectorField = "Diffusion Flux";
                    var d = world.AddVisualisationService<DensityParticlesData>();
                    var s = world.AddVisualisationService<DensityPathStructuresSpaceTime>();
                    d.dFicticious = .1;
                    d.Particles.ResizeIfNeeded(10000);
                    d.SeedInterval = new Rect<Vec1>(0, 4);
                    s.Power = 0.3;
                    s.Tau = 3;
                    data.ColorGradient = Gradients.GetGradient("matlab_hot");
                    d.Reversed = true;

                }, "#m");

        }
        if (BeginSlide())
        {
            Title("Convective Heat Transfer");
            Presi.Image(convection, new Vec2(0.5, .43), .78);
        }
        if (BeginSlide())
        {
            var relCenterPos = new Vec2(.5, .5);
            var relSize = new Vec2(1, .5) * .9;
            if (IsFirstStep())
            {
                //relSize *= new Vec2(1.3, 1.3);
            }

            var view = DrawWorldPanel(relCenterPos, relSize, zoom: .8,
                load: (world) =>
                {
                    var data = world.GetWorldService<DataService>();
                    var axis = world.AddVisualisationService<AxisVisualizer>();
                    axis.DrawTitle = false;
                    axis.DrawGradient = true;
                    data.SetDataset("Double Gyre EPS=0.1, Pe=100");
                    data.TimeMultiplier = .2f;
                    data.currentSelectedScaler = "Total Temperature";
                    //world.AddVisualisationService<FlowDirectionVisualization>();
                }, "#w");

            //Presi.Text("u =  sin(πx)cos(πy)\r\nv = -cos(π(x)sin(πy)", new Vec2(0.5,0.14), .06f, true, Color.White);
            if (IsFirstStep())
            {
                Title("2D Domain");
            }

            var worldPanel = Presi.GetWidgetData("#w", 0);
            var grad = view.World.GetWorldService<DataService>().ColorGradient;
            var lb = WorldToScreenRel(worldPanel, new Vec2(0, 0.0));
            var rb = WorldToScreenRel(worldPanel, new Vec2(1, 0.0));
            var lt = WorldToScreenRel(worldPanel, new Vec2(0, 0.5));
            var rt = WorldToScreenRel(worldPanel, new Vec2(1, 0.5));

            void DrawWalls()
            {
                Gizmos2D.Line(Presi.View.Camera2D, lb, rb, grad.Get(1), 15f);
                Gizmos2D.Line(Presi.View.Camera2D, lt, rt, grad.Get(0.0f), 15f);
                Gizmos2D.AdvText(Presi.View.Camera2D, rb + new Vec2(40, -20), 50f, grad.Get(1), "Hot");
                Gizmos2D.AdvText(Presi.View.Camera2D, rt + new Vec2(40, -20), 50f, grad.Get(0.2f), "Cold");
            }

            if (BeginStep())
            {
                // Title("Temperature");
                //DrawWalls();
                Gizmos2D.Rect(Presi.View.Camera2D, lb, rt, grad.Get(.3f));
                Gizmos2D.AdvText(Presi.View.Camera2D, WorldToScreenRel(worldPanel, new Vec2(0.5, .25)) + new Vec2(0, -20), 40f, Color.White, "Constant Initial Temperature", centered: true);

            }
            if (BeginStep())
            {
                //Title("Hot/Cold Wall");
                Gizmos2D.Rect(Presi.View.Camera2D, lb, rt, grad.Get(.3f));
                Gizmos2D.AdvText(Presi.View.Camera2D, WorldToScreenRel(worldPanel, new Vec2(0.5, .25)) + new Vec2(0, -20), 40f, Color.White, "Constant Initial Temperature", centered: true);
                DrawWalls();
            }



            if (BeginStep())
            {
                Title("Heat Simulation");
                if (view.World.GetWorldService<GridVisualizer>() == null)
                {
                    var grid = view.World.AddVisualisationService<GridVisualizer>();
                    view.World.DataService.SimulationTime = 0;
                    view.World.DataService.currentSelectedScaler = "No Flow Temperature";
                    grid.SetGridDiagnostic(new ScalerGridDiagnostic());
                    grid.WaitForComputation();
                    grid.AutoScale = false;
                    grid.max = 2;
                    grid.min = 0.5;
                    view.World.DataService.TimeMultiplier = .1f;
                }
            }


            if (BeginStep())
            {
                Title("Velocity Field");
                if (view.World.GetWorldService<GridVisualizer>() != null)
                {
                    view.World.RemoveWorldService(view.World.GetWorldService<GridVisualizer>());
                    view.World.AddVisualisationService<ArrowVisualizer>();
                }
                Presi.Text("u =  sin(πL)cos(πy), v = -cos(πL)sin(πy)\r\n L = x - sin(2πt)", new Vec2(0.5, 0.05), .024f, true, Color.White);
            }

            if (BeginStep())
            {
                Title("Heat Simulation (with flow)");
                if (view.World.GetWorldService<GridVisualizer>() == null)
                {
                    view.World.RemoveWorldService(view.World.GetWorldService<ArrowVisualizer>());
                    var grid = view.World.AddVisualisationService<GridVisualizer>();
                    view.World.DataService.SimulationTime = 0;
                    view.World.DataService.currentSelectedScaler = "Total Temperature";
                    grid.SetGridDiagnostic(new ScalerGridDiagnostic());
                    grid.WaitForComputation();
                    grid.AutoScale = false;
                    grid.max = 2;
                    grid.min = 0.5;
                    view.World.DataService.TimeMultiplier = .23f;
                }
            }
        }

        if (BeginSlide())
        {

            var size = new Vec2(1, .5) / 1.77;
            var posY = .5;
            var offsetX = -.28;

            if (IsFirstStep())
            {
                Title("Capturing effect of fluid flow");
                DrawPanels();
            }
            else
            {
                Title("Lagrangian Formalism");

            }

            if (BeginStep())
            {
                size = new Vec2(1, .5) / 2.5;
                posY = .14;
                offsetX = -.23;
                Presi.Image(michel0, new Vec2(0.5, .6), .8);
                DrawPanels();
            }

            if (BeginStep())
            {
                Title("Lagrangian Formalism");
                size = new Vec2(1, .5) / 1.9;
                posY = .5;
                offsetX = -.26;
                DrawPanels();

            }

            if (BeginStep())
            {
                size = new Vec2(1, .5) / 1.96;
                posY = .66;
                offsetX = -.28;

                var lineWidth = .03;
                DrawPanels();
                Gizmos2D.Line(Presi.View.Camera2D, Presi.RelToSceen(new Vec2(.5 - lineWidth / 2 - .03, posY)), Presi.RelToSceen(new Vec2(.5 + lineWidth / 2, posY)), Color.White, 20f);


                var y1 = .18;
                var y2 = .24;
                Gizmos2D.Line(Presi.View.Camera2D, Presi.RelToSceen(new Vec2(.1, y1)), Presi.RelToSceen(new Vec2(.2, y1)), Color.White, 16f);
                Gizmos2D.Line(Presi.View.Camera2D, Presi.RelToSceen(new Vec2(.1, y2)), Presi.RelToSceen(new Vec2(.2, y2)), Color.White, 16f);

                var vR = DrawWorldPanel(new Vec2(0.5, .22), size, zoom: .78,
                    load: (world) =>
                    {
                        var data = world.GetWorldService<DataService>();
                        var axis = world.AddVisualisationService<AxisVisualizer>();
                        axis.DrawTitle = true;
                        axis.DrawGradient = false;
                        axis.Title = "Convective Temperature";
                        data.SetDataset("Double Gyre EPS=0.1, Pe=100");
                        data.TimeMultiplier = .2f;
                        data.currentSelectedScaler = "Convective Temperature";
                        world.AddVisualisationService<GridVisualizer>().SetGridDiagnostic(new ScalerGridDiagnostic());
                        world.AddVisualisationService<ArrowVisualizer>().Disable();

                    }, "#m");
                vR.World.DataService.SimulationTime = t;
                vR.World.GetWorldService<GridVisualizer>().Continous = true;

            }

            void DrawPanels()
            {

                var viewR = DrawWorldPanel(new Vec2(.5 - offsetX, posY), size, zoom: .78,
                    load: (world) =>
                    {
                        var data = world.GetWorldService<DataService>();
                        var axis = world.AddVisualisationService<AxisVisualizer>();
                        axis.DrawTitle = true;
                        axis.DrawGradient = false;
                        axis.Title = "Temperature without Flow";
                        data.SetDataset("Double Gyre EPS=0.1, Pe=100");
                        data.currentSelectedScaler = "No Flow Temperature";
                        world.AddVisualisationService<GridVisualizer>().SetGridDiagnostic(new ScalerGridDiagnostic());
                    }, "#r");

                var view = DrawWorldPanel(new Vec2(.5 + offsetX, posY), size, zoom: .78,
                    load: (world) =>
                    {
                        var data = world.GetWorldService<DataService>();
                        var axis = world.AddVisualisationService<AxisVisualizer>();
                        axis.DrawTitle = true;
                        axis.DrawGradient = false;
                        axis.Title = "Temperature with Flow";
                        data.SetDataset("Double Gyre EPS=0.1, Pe=100");
                        data.currentSelectedScaler = "Total Temperature";
                        world.AddVisualisationService<GridVisualizer>().SetGridDiagnostic(new ScalerGridDiagnostic());
                    }, "#l");



                t += Presi.View.World.FlowExplainer.DeltaTime / 2;
                if (t + 1 >= view.World.DataService.ScalerField.Domain.RectBoundary.Max.Z)
                {
                    t = 0.01f;
                }
                view.World.GetWorldService<GridVisualizer>().Continous = true;
                viewR.World.GetWorldService<GridVisualizer>().Continous = true;
                view.World.DataService.SimulationTime = t;
                viewR.World.DataService.SimulationTime = t;
            }
        }
        if (BeginSlide())
        {
            var relCenterPos = new Vec2(.5, .56);
            double d = .95;


            if (IsFirstStep())
            {
                var view = DrawMainView();
                Title("Computing Heat Flux");
                t = 1.2;
            }
            if (SlideEnter())
            {
                var view = DrawMainView();
                view.World.GetWorldService<AxisVisualizer>().DrawTitle = false;
            }
            if (BeginStep())
            {
                var view = DrawMainView();
                if (!view.World.GetWorldService<ArrowVisualizer>().IsEnabled)
                {
                    view.World.GetWorldService<ArrowVisualizer>().Enable();
                    view.World.GetWorldService<ArrowVisualizer>().colorByGradient = false;
                    //  view.World.GetWorldService<GridVisualizer>().Disable();
                }

                view.World.DataService.currentSelectedVectorField = "Convection Flux";
                Title("Convection Flux");
                Presi.LatexCentered("$\\boldsymbol{u}T$", new Vec2(.5, .08), .17);
            }

            if (BeginStep())
            {
                var view = DrawMainView();
                view.World.DataService.currentSelectedVectorField = "Diffusion Flux";
                Title("Diffusion Flux");
                Presi.LatexCentered("$$-\\frac{1}{Pe}\\nabla T'$$", new Vec2(.5, .08), .27);
            }

            if (BeginStep())
            {
                var view = DrawMainView();
                if (!view.World.GetWorldService<ArrowVisualizer>().IsEnabled)
                {
                    view.World.GetWorldService<ArrowVisualizer>().Enable();
                    view.World.GetWorldService<ArrowVisualizer>().colorByGradient = false;
                    //  view.World.GetWorldService<GridVisualizer>().Disable();
                }

                view.World.DataService.currentSelectedVectorField = "Total Flux";
                Title("Total Flux");
                Presi.LatexCentered("$$\\boldsymbol{u}T -\\frac{1}{Pe}\\nabla T'$$", new Vec2(.5, .08), .27);
            }

            if (BeginStep())
            {
                var view = DrawMainView();
                if (!view.World.GetWorldService<ArrowVisualizer>().IsEnabled)
                {
                    view.World.GetWorldService<ArrowVisualizer>().Enable();
                    view.World.GetWorldService<ArrowVisualizer>().colorByGradient = false;
                    //  view.World.GetWorldService<GridVisualizer>().Disable();
                }

                view.World.DataService.currentSelectedVectorField = "Total Flux";
                Title("Time-Dependent");
                Presi.Slider("t", ref t, 0.01, 5, new Vec2(0.5, .1), .8f);
            }

            View DrawMainView()
            {
                var view1 = DrawWorldPanel(relCenterPos, new Vec2(1, .5) * d, zoom: .78,
                    load: (world) =>
                    {
                        var data = world.GetWorldService<DataService>();
                        var axis = world.AddVisualisationService<AxisVisualizer>();
                        data.SetDataset("Double Gyre EPS=0.1, Pe=100");
                        data.currentSelectedScaler = "Total Temperature";
                        world.AddVisualisationService<GridVisualizer>().SetGridDiagnostic(new ScalerGridDiagnostic());
                        world.AddVisualisationService<ArrowVisualizer>().Disable();
                    }, "#m");
                view1.World.DataService.SimulationTime = t;
                return view1;
            }
        }
        if (BeginSlide())
        {
            Title("Visualization");
            Presi.MainParagraph(
                @"
Goal: Visualize transport paths and emerging coherent structures

Dataset is:
- @orange[Unsteady]
- @orange[Aperiodic]
- @orange[Divergent]
- @orange[Numerically unstable where convective temperature approaches zero]
");

            /*
            Presi.Text("Solution: @green[Visualizing transport paths and structures in the time-reparameterized] \r\n @green[spacetime formulation of thermal transport]", new Vec2(0.5,.15), .023, true, Color.White);
        */
        }

        if (BeginSlide())
        {
            Title("Proposed Visualization Method");
            Presi.MainParagraph(
                @"
Our solution:
- @green[Integrate particles over time-reparameterized spacetime]
- @green[Visualize density via accumulation along temporal spacetime slices]
- @green[Reveals path coherency, attracting (heating) and repelling (cooling) structures]
");
            Presi.Image(seed, new Vec2(0.5, .25), .6);
        }
        if (BeginSlide())
        {
            Presi.Text("Example Visualizations", new Vec2(0.5, .5), .04, true, Color.White);
        }
    }
}