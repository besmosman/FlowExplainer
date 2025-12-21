using System.Net.Mime;
using ImGuiNET;

namespace FlowExplainer;

public class UpdatePresentation : NewPresentation
{
    private string periodicDataset = "(P) Double Gyre EPS=0.1, Pe=100";
    private string periodicDatasetHighPe = "(P) Double Gyre EPS=0.1, Pe=500";

    private string nonperiodicDataset = "Double Gyre EPS=0.1, Pe=100";
    private double t0 = 0;
    public bool arrows;

    public ImageTexture fig4 = new ImageTexture("Assets/Images/Presi/fig4.png");
    public ImageTexture fig6 = new ImageTexture("Assets/Images/Presi/fig6.png");
    public ImageTexture fig7 = new ImageTexture("Assets/Images/Presi/fig7.png");
    public ImageTexture fig13 = new ImageTexture("Assets/Images/Presi/fig13.png");
    public override void Draw()
    {
        var panelSize = new Vec2(.8, .4);
        var sliderPos = new Vec2(0.5, .07);
        if (true)
        {
            if (BeginSlide(""))
            {
                Presi.Text("Progress meeting 02/12/2025", new Vec2(0.5, 0.5), .05, true, Color.White);
            }
            if (BeginSlide("Stochastic"))
            {
                var world = DrawWorldPanel(new Vec2(.5, .5), panelSize, load: w =>
                {
                    var stochastic = StochasticInstant(w);
                    //stochastic.Color = new Color(1, .3f, .3f, 1);
                    stochastic.reverse = false;
                }).World;
                var rec = world.DataService.VectorField.Domain.RectBoundary;
                Title("Attracting Structures (Convection Flux)");
                Presi.Slider("time", ref t0, rec.Min.Last, rec.Max.Last, sliderPos, .5);
                world.DataService.SimulationTime = t0;
            }

            if (BeginSlide("Stochastic R"))
            {
                var world = DrawWorldPanel(new Vec2(.5, .5), panelSize, load: w =>
                {
                    var stochastic = StochasticInstant(w);
                    //stochastic.Color = new Color(1, .3f, .3f, 1);
                    stochastic.reverse = true;
                }).World;
                var rec = world.DataService.VectorField.Domain.RectBoundary;
                Title("Repelling Structures (Convection Flux)");
                Presi.Slider("time", ref t0, rec.Min.Last, rec.Max.Last, sliderPos, .5);
                world.DataService.SimulationTime = t0;
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
                Title("Attracting/Repelling Structures (Convection Flux)");
                Presi.Slider("time", ref t0, rec.Min.Last, rec.Max.Last, sliderPos, .5);
                world.DataService.SimulationTime = t0;
            }

            if (BeginSlide("Time"))
            {
                var world = DrawWorldPanel(new Vec2(.5, .5), panelSize, load: w =>
                {
                    w.DataService.SetDataset(nonperiodicDataset);
                    w.DataService.currentSelectedVectorField = "Total Flux";
                    w.DataService.currentSelectedScaler = "Convective Temperature";
                    var g = w.AddVisualisationService<GridVisualizer>();
                    var a = w.AddVisualisationService<ArrowVisualizer>();
                    g.SetGridDiagnostic(new ScalerGridDiagnostic());
                    w.DataService.TimeMultiplier = .5f;
                    w.DataService.SimulationTime = 0;
                }).World;
                var rec = world.DataService.VectorField.Domain.RectBoundary;
                Title("Convective Temperature");
                //if (IsFirstStep())
                {
                    Presi.Slider("time", ref world.DataService.SimulationTime, rec.Min.Last, rec.Max.Last, sliderPos, .5);
                }
                if (BeginStep())
                {
                    world.DataService.SimulationTime = world.DataService.SimulationTime % 1 + 5;
                }
            }
            if (BeginSlide(""))
            {
                TimeIntegration(periodicDataset, "Convection Flux", $"Periodic Attracting/Repelling Structures (Convection Flux)");
            }

            if (BeginSlide(""))
            {
                TimeIntegration(periodicDataset, "Diffusion Flux", $"Attracting/Repelling Structures (Diffusion Flux)");
            }
            if (BeginSlide(""))
            {
                TimeIntegration(periodicDatasetHighPe, "Diffusion Flux", $"Higher Péclet (Diffusion Flux, Pe=500)");
            }
            if (BeginSlide(""))
            {
               // Title("Lagrangian Structures");
                Presi.MainParagraph(
@"
What are these structures?
How does it compare to other methods?

Read:
- A variational theory of hyperbolic Lagrangian Coherent Structures (G. Haller 2011)
- Material barriers to diffusive and stochastic transport (G. Haller et al. 2018)
");
            }
            if (BeginSlide(""))
            {
                FuncCompare(fig4, (p) => new Vec2(p.X, -p.Y - double.Pow(p.Y, 3)));
            }
            if (BeginSlide(""))
            {
                FuncCompare(fig6, (p) => new Vec2(2 + double.Tanh(p.Y), 0));
            }

            if (BeginSlide(""))
            {
                FuncCompare(fig7, (p) => new Vec2(1 + double.Tanh(p.X) * double.Tanh(p.X), -p.Y));
            }
            if (BeginSlide(""))
            {
                FuncCompare(fig13, (p) => new Vec2(p.X, -p.Y));
            }


            if (BeginSlide(""))
            {
                FuncCompare(fig4, (p) => new Vec2(p.X, -p.Y - double.Pow(p.Y, 3)));
            }
        }

        if (BeginSlide(""))
        {
            var view = DrawWorldPanel(new Vec2(.5, .5), new Vec2(1, .6), load: w =>
            {
                w.DataService.SetDataset(periodicDataset);
                w.DataService.currentSelectedVectorField = "Convection Flux";
                w.DataService.currentSelectedScaler = "Convective Temperature";
                w.AddVisualisationService<Axis3D>();
                var v = w.AddVisualisationService<StochasticVisualization3D>();
                v.Count = 100000;
                v.maxAlpha = .1;
                v.dt = .05;
                v.ReseedChance = .05;
            });
            Title("3D version (Convection Flux)");
            if (SlideEnter())
            {
                view.CameraOffset = -new Vec3(.5, .25, .5);
                view.CameraZoom = 10;
                view.Is3DCamera = true;
            }
        }

        if (BeginSlide(""))
        {
            var view = DrawWorldPanel(new Vec2(.5, .5), new Vec2(1, .6), load: w =>
            {
                w.DataService.SetDataset(periodicDataset);
                w.DataService.currentSelectedVectorField = "Convection Flux";
                w.DataService.currentSelectedScaler = "Convective Temperature";
                w.AddVisualisationService<Axis3D>();
                var v = w.AddVisualisationService<StochasticVisualization3D>();
                v.Count = 100000;
                v.maxAlpha = 0;
                v.mode = StochasticVisualization3D.Mode.Backwords;
                v.dt = .1;
                v.lerpFactor = .3f;
                v.threshold = .38;
                v.VolumeRender = true;
                v.ReseedChance = .2;
                v.depthScaling = 90;
            });
            Title("Volume Rendering (Convection Flux)");
            if (SlideEnter())
            {
                view.CameraOffset = -new Vec3(.5, .25, .5);
                view.CameraZoom = 10;
                view.Is3DCamera = true;
            }
        }
        if (BeginSlide("s"))
        {
            Title("Summary");
            Presi.MainParagraph(
                @"
Last 2 weeks:
- Time-dependent version for finding attracting/repelling structures
- Comparing different LCS definitions
- Tried simple 3D version with volume rendering

Misc:
- Tool functionalities
- Randomized 2D/3D vectorfield
");
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


/*public class UpdatePresentation : Presentation
{
    public class LICVelocitySlide : PrecomputedSlide
    {
        public override string Title => "LIC (Velocity)";

        public override void SetupDatasource(World world)
        {
            var data = world.GetWorldService<DataService>();
            data.currentSelectedVectorField = "Velocity";
        }
    }

    public class LICDiffusionSlide : PrecomputedSlide
    {
        public override string Title => "LIC (Diffusion Flux)";

        public override void SetupDatasource(World world)
        {
            var data = world.GetWorldService<DataService>();
            data.currentSelectedVectorField = "Diffusion Flux";
        }
    }
    public class LICConvectionSlide : PrecomputedSlide
    {
        public override string Title => "LIC (Convection Flux)";

        public override void SetupDatasource(World world)
        {
            var data = world.GetWorldService<DataService>();
            data.currentSelectedVectorField = "Convection Flux";
        }
    }

    public class LICTotalSlide : PrecomputedSlide
    {
        public override string Title => "LIC (Total Flux)";

        public override void SetupDatasource(World world)
        {
            var data = world.GetWorldService<DataService>();
            data.currentSelectedVectorField = "Total Flux";
        }
    }


    /*
    public class ULICDiffusionSlide : PrecomputedSlide
    {
        public override string Title => "Pathline LIC (Diffusion Flux)";
        public override double T => .3f;

        public override void SetupDatasource(World world)
        {
            var gridVisualizer = world.GetWorldService<GridVisualizer>();
            var data = world.GetWorldService<DataService>();
            data.currentSelectedVectorField = "Diffusion Flux";
            gridVisualizer.SetGridDiagnostic(new LICGridDiagnostic
            {
                UseUnsteady = true,
                arcLength = T,
            });
        }
    }


    public class AULICDiffusionSlide : PrecomputedSlide
    {
        public override string Title => "AULIC (Velocity)";

        public override void SetupDatasource(World world)
        {
            var gridVisualizer = world.GetWorldService<GridVisualizer>();
            var data = world.GetWorldService<DataService>();
            data.currentSelectedVectorField = "Velocity";
            gridVisualizer.SetGridDiagnostic(new UFLIC
            {
                dt = .04f,
                startTime = 0,
                expected_lifetime = .2f,
                auto = true,
            });
        }
    }
    #1#

    /*
    public abstract class PrecomputedSlide : Slide
    {
        public abstract void SetupDatasource(World world);
        public abstract string Title { get; }

        public bool pathlineVersion;
        public double T = .4f;
        public override void Prepare(FlowExplainer flowExplainer)
        {
            var world = flowExplainer.GetGlobalService<WorldManagerService>().Worlds[0];
            var gridVisualizer = world.GetWorldService<GridVisualizer>();
            gridVisualizer.Enable();

            gridVisualizer.TargetCellCount = 100000;
            SetupDatasource(world);
            var path = Title.Replace(" ", "_");
            gridVisualizer.SetGridDiagnostic(new LICGridDiagnostic
            {
                arcLength = .1f,
            });
            gridVisualizer.Save(path + "-streamline", 0, 1, 50);
            gridVisualizer.SetGridDiagnostic(new LICGridDiagnostic
            {
                UseUnsteady = true,
                arcLength = T,
            });
            gridVisualizer.Save(path + "-pathline", 0, 1, 50);
            base.Prepare(flowExplainer);
        }

        public override void Load()
        {
            var dataService = w0.GetWorldService<DataService>();
            var path = Title.Replace(" ", "_");

            dataService.LoadScalerField(Title+"-streamline", path+"-streamline");
            dataService.LoadScalerField(Title+"-pathline", path+"-pathline");
            dataService.currentSelectedScaler = Title+"-streamline";

            var size = ((RegularGridVectorField<Vec3, Vec3i, double>)dataService.ScalerFields[dataService.currentSelectedScaler]).GridSize;
            base.Load();
            w0.GetWorldService<GridVisualizer>().TargetCellCount = size.X * size.Y;
        }

        public override void OnEnter()
        {
            var gridVisualizer = w0.GetWorldService<GridVisualizer>();
            gridVisualizer.Enable();
            gridVisualizer.Continous = true;
            gridVisualizer.SetGridDiagnostic(new ScalerGridDiagnostic());
            var dataService = w0.GetWorldService<DataService>();
            base.OnEnter();
        }
        public override void Draw()
        {
            var dataService = w0.GetWorldService<DataService>();

            if (pathlineVersion)
                dataService.currentSelectedScaler = Title + "-pathline";
            else
                dataService.currentSelectedScaler = Title + "-streamline";

            LayoutMain();
            Title(Title);
            w0.GetWorldService<GridVisualizer>().RegularGrid.Interpolate = false;
            Presi.ViewPanel("v0", Presi.CanvasSize / 2 + new Vec2(0, 00), new Vec2(1, .5f) * Presi.CanvasSize.X * .9f, .8f);
            Presi.Checkbox("Pathline", ref pathlineVersion, new Vec2(Presi.CanvasCenter.X - 700, 100));

            if (!pathlineVersion)
            {
                Presi.Slider("time", ref w0.GetWorldService<DataService>().SimulationTime, 0, 1, new Vec2(Presi.CanvasCenter.X, 100), 500);
            }
            else
            {
                var t = w0.GetWorldService<DataService>().SimulationTime;
                var title = $"time = [{t:N2}..{(t + T):N2}]";
                Presi.SliderCustomTitle(title, ref w0.GetWorldService<DataService>().SimulationTime, 0, 1, new Vec2(Presi.CanvasCenter.X, 100), 500);
            }
            base.Draw();
        }
    }#1#

    public override Slide[] GetSlides()
    {
        return
        [
            //new AULICDiffusionSlide(),
            /*new LICVelocitySlide(),
            new LICDiffusionSlide(),
            new LICConvectionSlide(),
            new LICTotalSlide(),#1#
            //new ULICDiffusionSlide()
        ];
    }
    public override void Setup(FlowExplainer flowExplainer)
    {
        var presentationService = flowExplainer.GetGlobalService<PresentationService>()!;

        var manager = flowExplainer.GetGlobalService<WorldManagerService>();
        var w0 = manager.NewWorld();
        w0.GetWorldService<DataService>().ColorGradient = Gradients.Grayscale;
        Scripting.SetGyreDataset(w0);
        presentationService.Presi.GetView("v0").World = w0;
    }

    public override void Prepare(FlowExplainer flowExplainer)
    {
        base.Prepare(flowExplainer);
    }

}*/