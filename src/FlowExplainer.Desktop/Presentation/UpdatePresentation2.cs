namespace FlowExplainer;

public class UpdatePresentation2 : NewPresentation
{
    private string periodicDataset = "(P) Double Gyre EPS=0.1, Pe=100";
    private string periodicDatasetHighPe = "(P) Double Gyre EPS=0.1, Pe=500";
    private string stableDataset = "Double Gyre EPS=0, Pe=100";

    private string nonperiodicDataset = "Double Gyre EPS=0.1, Pe=100";
    private double t0 = 0;
    public bool arrows;
    public bool map;

    public ImageTexture fig4 = new ImageTexture("Assets/Images/Presi/fig4.png");
    public ImageTexture fig6 = new ImageTexture("Assets/Images/Presi/fig6.png");
    public ImageTexture fig7 = new ImageTexture("Assets/Images/Presi/fig7.png");
    public ImageTexture fig13 = new ImageTexture("Assets/Images/Presi/fig13.png");
    public override void Draw()
    {
     

        var panelSize = new Vec2(.8, .4);
        var sliderPos = new Vec2(0.5, .12);
        if (true)
        {
            if (BeginSlide(""))
            {
                Presi.Text("Progress meeting 16/11/2025", new Vec2(0.5, 0.5), .05, true, Color.White);
            }

            if (BeginSlide("Stochastic R"))
            {
                var world = DrawWorldPanel(new Vec2(.5, .5), panelSize, load: w =>
                {
                    var stochastic = StochasticInstant(w);
                    var stochastic2 = w.AddVisualisationService<StochasticVisualization>();
                    stochastic2.mode = StochasticVisualization.Mode.Instantaneous;
                    stochastic2.dt = .13f;
                    stochastic2.ReseedChance = .7f;
                    stochastic2.alpha = .04f;
                    stochastic2.Count = 20000;
                    stochastic2.reverse = true;
                    stochastic.Color = new Color(.3, 1f, .3f, 1);
                    stochastic2.Color = new Color(1, .3f, .3f, 1);
                }).World;
                var rec = world.DataService.VectorField.Domain.RectBoundary;
                Title("Instantaneous Attracting/Repelling Structures (Convection Flux)");
                Presi.Slider("time", ref t0, rec.Min.Last, rec.Max.Last, sliderPos, .5);
                world.DataService.SimulationTime = t0;
            }

            if (BeginSlide(""))
            {
                TimeIntegration(periodicDataset, "Convection Flux", $"Periodic Attracting/Repelling Structures (Convection Flux)");
            }


            if (BeginSlide(""))
            {
                FuncCompare(fig6, (p) => new Vec2(2 + double.Tanh(p.Y), 0));
            }


            if (BeginSlide(""))
            {
                FuncCompare(fig4, (p) => new Vec2(p.X, -p.Y - double.Pow(p.Y, 3)));
            }

        }

        if (BeginSlide(""))
        {
            Presi.Image(fig4, new Vec2(0.24, .5), .5);
            Func<Vec2, Vec2> func = (p) => new Vec2(p.X, -p.Y - double.Pow(p.Y, 3));
            Presi.Text("(Source: G. Haller 2011)", new Vec2(0.04, .05), .03, false, Color.White);
            var view = DrawWorldPanel(new Vec2(.8, .55), new Vec2(1) / 2.5, zoom: .5f, load: w =>
            {
                map = false;
                w.DataService.SetDataset(nonperiodicDataset);
                var vec = new ArbitraryField<Vec2, Vec2>(new RectDomain<Vec2>(-Vec2.One, Vec2.One), func);
                var domainUp = new RectDomain<Vec3>(vec.Domain.RectBoundary.Min.Up(0), vec.Domain.RectBoundary.Max.Up(1));
                w.DataService.LoadedDataset.VectorFields["t"] = new ArbitraryField<Vec3, Vec2>(domainUp, p => vec.Evaluate(p.XY));
                w.DataService.currentSelectedVectorField = "t";

                var stoch = w.AddVisualisationService<StochasticConnectionVisualization>();
                stoch.DrawParticles = true;
                stoch.LifeTime = 1;
                stoch.Alpha = .4f;
                stoch.ReseedChance = .1;
                stoch.RenderRadius = .01;
                stoch.Count = 40000;
                stoch.Initialize();
                w.DataService.TimeMultiplier = 2f;
                w.DataService.SimulationTime = 0;
                ftle = true;
                var grid = w.AddVisualisationService<GridVisualizer>();
                grid.SetGridDiagnostic(new StochasticConnectionVisualization.GridDiagnostics()
                {
                    interpolationFactor = .04,
                    type = StochasticConnectionVisualization.GridDiagnostics.Type.Attracting,
                });
                if (!map)
                    grid.Disable();

                w.AddVisualisationService<ArrowVisualizer>().colorByGradient = false;
                w.GetWorldService<ArrowVisualizer>().AutoResize = false;
                w.GetWorldService<ArrowVisualizer>().Length = .08;
                w.GetWorldService<ArrowVisualizer>().GridCells = 100;
                w.GetWorldService<ArrowVisualizer>().Thickness = .01;
            });
            view.Camera2D.Position = Vec2.Zero;
            var gridDiagnostics = (StochasticConnectionVisualization.GridDiagnostics)view.World.GetWorldService<GridVisualizer>().diagnostic;
            Presi.DropdownEnum("Mode", ref gridDiagnostics.type, new Vec2(.8, .17));
            Presi.Checkbox("Map", ref map, new Vec2(.62, .17));
            if (map != view.World.GetWorldService<GridVisualizer>().IsEnabled)
            {
                if (view.World.GetWorldService<GridVisualizer>().IsEnabled)
                {
                    view.World.GetWorldService<GridVisualizer>().Disable();
                    // view.World.GetWorldService<ArrowVisualizer>().Enable();
                }
                else
                {
                    // view.World.GetWorldService<ArrowVisualizer>().Disable();
                    view.World.GetWorldService<GridVisualizer>().Enable();
                }

            }
        }


        if (BeginSlide(""))
        {
            KernelInterpolation((w) =>
            {
                w.DataService.SetDataset(periodicDataset);
                w.DataService.currentSelectedVectorField = "Convection Flux";
            });
        }

        if (BeginSlide(""))
        {
            KernelInterpolation((w) =>
            {
                w.DataService.SetDataset(periodicDataset);
                w.DataService.currentSelectedVectorField = "Diffusion Flux";
            });
            /*KernelInterpolation((w) =>
            {
                w.DataService.SetDataset(stableDataset);
                w.DataService.LoadedDataset.VectorFields["t"] = new ArbitraryField<Vec3, Vec2>
                    (new RectDomain<Vec3>(new Rect<Vec3>(Vec3.Zero, new Vec3(1, .5f, 1))), vec3 => new Vec2((vec3.X-.5f)*vec3.Y*vec3.X, 0));
                w.DataService.currentSelectedVectorField = "t";
            });*/
        }



        if (BeginSlide("Nice"))
        {
            var view = DrawWorldPanel(new Vec2(.5, .5), new Vec2(1, 1), load: w =>
            {
                w.DataService.SetDataset(periodicDatasetHighPe);
                w.DataService.currentSelectedVectorField = "Diffusion Flux";
                w.DataService.ColorGradient = Gradients.GetGradient("matlab_hot");
                var stoch = w.AddVisualisationService<StochasticConnectionVisualization>();
                stoch.DrawParticles = false;
                stoch.Alpha = .5f;
                stoch.ReseedChance = 0.1;
                stoch.RenderRadius = .03;
                stoch.kernelSizeM = .3;
                stoch.Count = 64_000;
                stoch.HighlightMouse = false;
                stoch.Initialize();
                w.DataService.TimeMultiplier = 3f;
                w.DataService.SimulationTime = 0;
                ftle = true;
                var grid = w.AddVisualisationService<GridVisualizer>();
                grid.TargetCellCount = 128_000;
                var gridDiagnostics = new StochasticConnectionVisualization.GridDiagnostics();
                grid.SetGridDiagnostic(gridDiagnostics);
                gridDiagnostics.interpolationFactor = .08;
                gridDiagnostics.type = StochasticConnectionVisualization.GridDiagnostics.Type.Repelling;

                //  w.AddVisualisationService<ArrowVisualizer>().colorByGradient = false;
            });
        }
        
        if (BeginSlide("Nice"))
        {
            var view = DrawWorldPanel(new Vec2(.5, .5), new Vec2(1, 1), load: w =>
            {
                w.DataService.SetDataset(stableDataset);
                w.DataService.currentSelectedVectorField = "Diffusion Flux";
                w.DataService.ColorGradient = Gradients.GetGradient("matlab_hot");
                var stoch = w.AddVisualisationService<StochasticConnectionVisualization>();
                stoch.DrawParticles = false;
                stoch.Alpha = .5f;
                stoch.ReseedChance = 0.8;
                stoch.RenderRadius = .03;
                stoch.kernelSizeM = .3;
                stoch.Count = 64_000;
                stoch.HighlightMouse = false;
                stoch.Initialize();
                w.DataService.TimeMultiplier = 3f;
                w.DataService.SimulationTime = 0;
                ftle = true;
                var grid = w.AddVisualisationService<GridVisualizer>();
                grid.TargetCellCount = 128_000;
                var gridDiagnostics = new StochasticConnectionVisualization.GridDiagnostics();
                grid.SetGridDiagnostic(gridDiagnostics);
                gridDiagnostics.interpolationFactor = .08;
                gridDiagnostics.type = StochasticConnectionVisualization.GridDiagnostics.Type.Repelling;

                //  w.AddVisualisationService<ArrowVisualizer>().colorByGradient = false;
            });
        }

        if (BeginSlide(""))
        {
            var view = DrawWorldPanel(new Vec2(.5, .5), new Vec2(1, 1), load: w =>
            {
                w.DataService.SetDataset(periodicDataset);
                w.DataService.currentSelectedVectorField = "Diffusion Flux";
                var gridDiagnostic = new UlamsGrid();
                w.AddVisualisationService<GridVisualizer>().SetGridDiagnostic(gridDiagnostic);
                gridDiagnostic.Recompute(w.GetWorldService<GridVisualizer>());
            });
            var ulam = (UlamsGrid)view.World.GetWorldService<GridVisualizer>().diagnostic;
            ulam.method.w = view.MousePosition;
            //Gizmos2D.Circle(view.Camera2D, view.MousePosition, Color.White, .01f);
        }
        void KernelInterpolation(Action<World> loaddat)
        {
            Title("Kernel Interpolation");
            var view = DrawWorldPanel(new Vec2(.5, .5), panelSize, load: w =>
            {
                loaddat(w);
                var stoch = w.AddVisualisationService<StochasticConnectionVisualization>();
                stoch.DrawParticles = true;
                stoch.LifeTime = 3;
                stoch.Alpha = .5f;
                stoch.ReseedChance = 0.8;
                stoch.RenderRadius = .002;
                stoch.Count = 30000;
                stoch.HighlightMouse = true;
                stoch.Initialize();
                w.DataService.TimeMultiplier = 1f;
                w.DataService.SimulationTime = 0;
                ftle = true;
                var grid = w.AddVisualisationService<GridVisualizer>();
                grid.TargetCellCount = 40000;
                grid.SetGridDiagnostic(new StochasticConnectionVisualization.GridDiagnostics()
                {
                    type = StochasticConnectionVisualization.GridDiagnostics.Type.Difference,
                });
                //     if (!ftle)
                grid.Disable();
                //  w.AddVisualisationService<ArrowVisualizer>().colorByGradient = false;
            }, filePath: loaddat.Method.GetHashCode().ToString());
            var world = view.World;
            var gridDiagnostics = (StochasticConnectionVisualization.GridDiagnostics)world.GetWorldService<GridVisualizer>().diagnostic;
            Presi.DropdownEnum("Mode", ref gridDiagnostics.type, new Vec2(.5, sliderPos.Y));
            Presi.Checkbox("Kernel Interpolation", ref map, new Vec2(.12, sliderPos.Y));
            Presi.Slider("dt", ref world.DataService.TimeMultiplier, 0, 1, new Vec2(.72, sliderPos.Y), .2f);
            Presi.Slider("Blending", ref gridDiagnostics.interpolationFactor, 0, 1, new Vec2(.72, sliderPos.Y - .08), .2f);

            if (world.GetWorldService<GridVisualizer>().IsEnabled != map)
            {
                if (world.GetWorldService<GridVisualizer>().IsEnabled)
                {
                    world.GetWorldService<GridVisualizer>().Disable();
                    world.GetWorldService<StochasticConnectionVisualization>().DrawParticles = true;
                }
                else
                {
                    world.GetWorldService<GridVisualizer>().Enable();
                    world.GetWorldService<StochasticConnectionVisualization>().DrawParticles = false;
                }
            }
        }

        void TimeIntegration(string dataset, string vectorfield, string title)
        {
            Title(title);
            var world = DrawWorldPanel(new Vec2(.5, .5), panelSize, load: w =>
            {
                w.DataService.SetDataset(dataset);
                w.DataService.currentSelectedVectorField = vectorfield;
                w.DataService.TimeMultiplier = .5f;
                var arrowVisualizer = w.AddVisualisationService<ArrowVisualizer>();
                if (!arrows)
                    arrowVisualizer.Disable();
                var stochastic = w.AddVisualisationService<StochasticVisualization>();
                arrowVisualizer.colorByGradient = true;

                stochastic.dt = .08f;
                stochastic.alpha = .04f;
                stochastic.ReseedChance = .7f;

                stochastic.Count = 20000;
                stochastic.mode = StochasticVisualization.Mode.TimeIntegration;
                var stochastic2 = w.AddVisualisationService<StochasticVisualization>();
                stochastic2.mode = StochasticVisualization.Mode.TimeIntegration;
                stochastic2.dt = .13f;
                stochastic2.ReseedChance = .7f;
                stochastic2.alpha = .07f;
                stochastic2.Count = 20000;
                stochastic2.reverse = true;
                stochastic.Color = new Color(.2, 1f, .2f, 1);
                stochastic2.Color = new Color(1, .3f, .3f, 1);

                if (vectorfield == "Diffusion Flux")
                {
                    stochastic.ReseedChance /= 10;
                    stochastic2.ReseedChance /= 10;

                }
            }, filePath: title).World;
            var rec = world.DataService.VectorField.Domain.RectBoundary;
            var t = world.DataService.SimulationTime % 1 + 5;
            Presi.Slider("time", ref t, 0, 6, sliderPos, .5);
            arrows = world.GetWorldService<ArrowVisualizer>().IsEnabled;

            Presi.Checkbox("Arrows", ref arrows, new Vec2(.12, sliderPos.Y));
            if (world.GetWorldService<ArrowVisualizer>().IsEnabled != arrows)
            {
                if (world.GetWorldService<ArrowVisualizer>().IsEnabled)
                    world.GetWorldService<ArrowVisualizer>().Disable();
                else
                    world.GetWorldService<ArrowVisualizer>().Enable();
            }
        }

    }
    private StochasticVisualization StochasticInstant(World w)
    {

        w.GetWorldService<DataService>().SetDataset(periodicDataset);
        w.GetWorldService<DataService>().currentSelectedVectorField = "Convection Flux";
        var arrowVisualizer = w.AddVisualisationService<ArrowVisualizer>();
        var stochastic = w.AddVisualisationService<StochasticVisualization>();
        arrowVisualizer.colorByGradient = true;
        stochastic.mode = StochasticVisualization.Mode.Instantaneous;
        stochastic.dt = .08f;
        stochastic.ReseedChance = .7f;
        stochastic.alpha = .04f;
        stochastic.Count = 20000;
        return stochastic;
    }

    private bool ftle;
    void FuncCompare(Texture image, Func<Vec2, Vec2> field)
    {

        Presi.Image(image, new Vec2(0.24, .5), .5);
        Presi.Text("(Source: G. Haller 2011)", new Vec2(0.04, .05), .03, false, Color.White);
        var view = DrawWorldPanel(new Vec2(.8, .55), new Vec2(1) / 2.5, zoom: .5f, load: w =>
        {
            w.DataService.SetDataset(nonperiodicDataset);
            var vec = new ArbitraryField<Vec2, Vec2>(new RectDomain<Vec2>(-Vec2.One, Vec2.One), field);
            var domainUp = new RectDomain<Vec3>(vec.Domain.RectBoundary.Min.Up(0), vec.Domain.RectBoundary.Max.Up(1));
            w.DataService.LoadedDataset.VectorFields["t"] = new ArbitraryField<Vec3, Vec2>(domainUp, p => vec.Evaluate(p.XY));
            w.DataService.currentSelectedVectorField = "t";

            var stochAttracting = w.AddVisualisationService<StochasticVisualization>();
            stochAttracting.mode = StochasticVisualization.Mode.Instantaneous;
            stochAttracting.Count = 40000;
            stochAttracting.dt = .1f;
            stochAttracting.RenderRadius = .01f;
            stochAttracting.alpha = .3f;

            var stochRepelling = w.AddVisualisationService<StochasticVisualization>();
            stochRepelling.mode = StochasticVisualization.Mode.Instantaneous;
            stochRepelling.Count = 40000;
            stochRepelling.dt = .1f;
            stochRepelling.RenderRadius = .01f;
            stochRepelling.alpha = .3f;
            stochAttracting.Color = new Color(.3, 1f, .3f, 1);
            stochRepelling.Color = new Color(1, .3f, .3f, 1);
            stochRepelling.reverse = true;
            w.DataService.TimeMultiplier = .5f;
            w.DataService.SimulationTime = 0;
            ftle = true;
            var grid = w.AddVisualisationService<GridVisualizer>();
            grid.SetGridDiagnostic(new FTLEGridDiagnostic());
            if (!ftle)
                grid.Disable();
            w.AddVisualisationService<ArrowVisualizer>().colorByGradient = false;

            w.GetWorldService<ArrowVisualizer>().AutoResize = false;
            w.GetWorldService<ArrowVisualizer>().Length = .08;
            w.GetWorldService<ArrowVisualizer>().GridCells = 100;
            w.GetWorldService<ArrowVisualizer>().Thickness = .01;


        }, filePath: image.GetHashCode().ToString());

        Presi.Checkbox("Show FTLE", ref ftle, new Vec2(.7, .1));
        if (ftle != view.World.GetWorldService<GridVisualizer>().IsEnabled)
        {
            if (view.World.GetWorldService<GridVisualizer>().IsEnabled)
                view.World.GetWorldService<GridVisualizer>().Disable();
            else
                view.World.GetWorldService<GridVisualizer>().Enable();
        }
        view.Camera2D.Position = Vec2.Zero;
    }
}