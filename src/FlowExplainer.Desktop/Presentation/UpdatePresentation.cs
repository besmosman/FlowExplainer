namespace FlowExplainer;

public class UpdatePresentation : Presentation
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
    */

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
    }

    public override Slide[] GetSlides()
    {
        return
        [
            //new AULICDiffusionSlide(),
            new LICVelocitySlide(),
            new LICDiffusionSlide(),
            new LICConvectionSlide(),
            new LICTotalSlide(),
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

}