using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class DatasetPresentation : Presentation
{
    public class VelocityField1Slide : Slide
    {
        public override void Draw()
        {
            LayoutMain();
            Title("Velocity Field");
            Presi.Text("u(x,y) =  sin(2πx)cos(2πy) \r\n v(x,y) = -cos(2πx)sin(2πy) \r\n x : [0, 1],  y : [0, 0.5]  ", new Vec2(Presi.CanvasCenter.X, 700), 96, true, Color.White);
            base.Draw();
        }
    }

    public class VelocityField2Slide : Slide
    {
        public override void Draw()
        {
            LayoutMain();
            Title("Velocity Field");
            Presi.ViewPanel("v0", Presi.CanvasSize / 2 + new Vec2(0, 60), new Vec2(1, .5f) * Presi.CanvasSize.X * .8f, .8f);
            Presi.Text("u(x,y) =  sin(2πx)cos(2πy)\r\nv(x,y) = -cos(2πx)sin(2πy)", new Vec2(Presi.CanvasCenter.X, 200), 80, true, Color.White);
            base.Draw();
        }
    }

    public class VelocityField3Slide : Slide
    {

        public override void OnEnter()
        {
            Presi.GetView("v0").World.GetWorldService<FlowFieldVisualizer>().Enable();
            Presi.GetView("v0").World.GetWorldService<FlowFieldVisualizer>().colorByGradient = true;
            Presi.GetView("v0").World.GetWorldService<DataService>().ColorGradient = Gradients.Parula;
            Presi.GetView("v0").World.GetWorldService<DataService>().TimeMultiplier = .1f;
            Presi.GetView("v0").World.GetWorldService<DataService>().SimulationTime = .0f;
            base.OnEnter();
        }
        public override void OnLeave()
        {
            Presi.GetView("v0").World.GetWorldService<DataService>().TimeMultiplier = 0f;
            Presi.GetView("v0").World.GetWorldService<DataService>().SimulationTime = .0f;
            base.OnLeave();
        }
        public override void Draw()
        {
            LayoutMain();
            Title("Velocity Field");
            Presi.ViewPanel("v0", Presi.CanvasSize / 2 + new Vec2(0, 60), new Vec2(1, .5f) * Presi.CanvasSize.X * .8f, .8f);
            Presi.GetView("v0").World.GetWorldService<DataService>().TimeMultiplier = .1f;
            Presi.Text("ε = 0.1, L = εsin(2πt) \r\n u(x,y) = sin(2π(x-L))cos(2πy)\r\n v(x,y) = -cos(2π(x-L))sin(2πy)", new Vec2(Presi.CanvasCenter.X, 220), 80, true, Color.White);
            base.Draw();
        }
    }


    public class HeatSimIntroSlide : Slide
    {
        public Texture image;

        public override void Load()
        {
            image = new ImageTexture("Assets/Images/presi/heatfunc.png")
            {
                TextureMagFilter = TextureMagFilter.Linear,
                TextureMinFilter = TextureMinFilter.Linear,
            };
            base.Load();
        }
        public override void OnEnter()
        {
            Presi.GetView("v0").World.GetWorldService<FlowFieldVisualizer>().Disable();
            //Presi.GetView("v0").World.GetWorldService<DataService>().ColorGradient = Gradients.GetGradient("BlueGrayRed");
            Presi.GetView("v0").World.GetWorldService<DataService>().SimulationTime = .0f;
            Presi.GetView("v0").World.GetWorldService<DataService>().TimeMultiplier = .0f;
            Presi.GetView("v0").World.GetWorldService<AxisVisualizer>().DrawWalls = true;
            base.OnEnter();
        }

        public override void Draw()
        {
            LayoutMain();
            Title("Heat Simulation");
            Presi.ViewPanel("v0", Presi.CanvasSize / 2 + new Vec2(0, 60), new Vec2(1, .5f) * Presi.CanvasSize.X * .8f, .8f);

            var colorHot = Presi.GetView("v0").World.GetWorldService<DataService>().ColorGradient.Get(1).ToHex();
            var colorCold = Presi.GetView("v0").World.GetWorldService<DataService>().ColorGradient.Get(.06f).ToHex();
            Presi.GetView("v0").World.GetWorldService<DataService>().TimeMultiplier = .1f;
            Presi.Text($"T(x, 0.5, t) = @#{colorCold}[T]\r\nT(x, 0, t)    = @#{colorHot}[T]", new Vec2(Presi.CanvasCenter.X - 400, 200), 80, true, Color.White);
            Presi.Text($"@#{colorHot}[hot]", new Vec2(Presi.CanvasCenter.X + 180 - 400, 100), 60, false, Color.White);
            Presi.Text($"@#{colorCold}[cold]", new Vec2(Presi.CanvasCenter.X + 180 - 400, 180), 60, false, Color.White);
            Presi.Text($"\r\n Pe = 100", new Vec2(Presi.CanvasCenter.X + 300, 200), 80, true, Color.White);

            Presi.Image(image, new Vec2(Presi.CanvasCenter.X + 400, 200), 500);

            base.Draw();
        }
    }



    public class HeatNoFlowSlide : Slide
    {
        public override void OnEnter()
        {
            var world = Presi.GetView("v0").World;
            var dat = world.GetWorldService<DataService>();
            dat.SimulationTime = .0000001f;
            dat.TimeMultiplier = .1f;
            var gridVisualizer = world.GetWorldService<GridVisualizer>();
            gridVisualizer.Enable();
            gridVisualizer.AutoScale = true;
            gridVisualizer.SetGridDiagnostic(new TemperatureGridDiagnostic());
            dat.VelocityField = IVectorField<Vec3, Vec2>.Constant(default, TempTot.Domain);
            dat.TempratureField = TempTotNoFlow;
            base.Load();
        }
        public override void Draw()
        {
            LayoutMain();
            Title("Heat Simulation u=0");
            Presi.ViewPanel("v0", Presi.CanvasSize / 2 + new Vec2(0, 60), new Vec2(1, .5f) * Presi.CanvasSize.X * .8f, .8f);
            Presi.Text("u(x,y,t) = 0", new Vec2(Presi.CanvasCenter.X, 220), 80, true, Color.White);
            base.Draw();
        }
    }


    public class HeatFlowSlide : Slide
    {
        public override void OnEnter()
        {
            Presi.GetView("v0").World.GetWorldService<AxisVisualizer>().DrawWalls = false;
            var world = Presi.GetView("v0").World;
            var dat = world.GetWorldService<DataService>();
            dat.SimulationTime = .0000001f;
            dat.TimeMultiplier = .1f;
            var gridVisualizer = world.GetWorldService<GridVisualizer>();
            gridVisualizer.Enable();
            gridVisualizer.SetGridDiagnostic(new TemperatureGridDiagnostic());
            dat.VelocityField = VelocityField;
            dat.TempratureField = TempTot;
            base.Load();
        }
        public override void Draw()
        {
            LayoutMain();
            Title("Spectral Simulation Result");
            Presi.ViewPanel("v0", Presi.CanvasSize / 2 + new Vec2(0, 60), new Vec2(1, .5f) * Presi.CanvasSize.X * .8f, .8f);
            //Presi.Text("u(x,y,t) = 0", new Vec2(Presi.CanvasCenter.X, 220), 80, true, Color.White);
            base.Draw();
        }
    }

    public class ConvectiveConvFluxSlide : Slide
    {
        public override void OnEnter()
        {
            Presi.GetView("v0").World.GetWorldService<AxisVisualizer>().DrawWalls = false;
            var world = Presi.GetView("v0").World;
            var dat = world.GetWorldService<DataService>();
            dat.SimulationTime = .9f;
            dat.TimeMultiplier = .0f;
            dat.VelocityField = ConvFluxField;
            dat.TempratureField = TempConvection;
            var flowFieldVisualizer = world.GetWorldService<FlowFieldVisualizer>();
            flowFieldVisualizer.Enable();
            flowFieldVisualizer.colorByGradient = false;
            base.Load();
        }

        public override void Draw()
        {
            LayoutMain();
            Title("Convective Temperature + Flux (t=0.9)");
            Presi.ViewPanel("v0", Presi.CanvasSize / 2 + new Vec2(0, 00), new Vec2(1, .5f) * Presi.CanvasSize.X * .9f, .8f);
            //Presi.Text("u(x,y,t) = 0", new Vec2(Presi.CanvasCenter.X, 220), 80, true, Color.White);
            base.Draw();
        }
    }

    public class ConvectiveDiffFluxSlide : Slide
    {
        public override void OnEnter()
        {
            Presi.GetView("v0").World.GetWorldService<AxisVisualizer>().DrawWalls = false;
            var world = Presi.GetView("v0").World;
            var dat = world.GetWorldService<DataService>();
            dat.SimulationTime = .9f;
            dat.TimeMultiplier = .0f;
            dat.VelocityField = DiffFluxField;
            dat.TempratureField = TempConvection;
            var flowFieldVisualizer = world.GetWorldService<FlowFieldVisualizer>();
            flowFieldVisualizer.Enable();
            flowFieldVisualizer.colorByGradient = false;
            base.Load();
        }

        public override void Draw()
        {
            LayoutMain();
            Title("Convective Temperature + diffusion flux (t=0.9)");
            Presi.ViewPanel("v0", Presi.CanvasSize / 2 + new Vec2(0, 00), new Vec2(1, .5f) * Presi.CanvasSize.X * .9f, .8f);
            //Presi.Text("u(x,y,t) = 0", new Vec2(Presi.CanvasCenter.X, 220), 80, true, Color.White);
            base.Draw();
        }
    }


    public class HeatSubtractSlide : Slide
    {
        public override void OnEnter()
        {
            Presi.GetView("v0").World.GetWorldService<AxisVisualizer>().DrawWalls = false;
            var w0 = Presi.GetView("v0").World;
            var w1 = Presi.GetView("v1").World;
            var w2 = Presi.GetView("v2").World;
            World[] worlds = [w0, w1, w2];
            foreach (var w in worlds)
            {
                var dat = w.GetWorldService<DataService>();
                dat.TimeMultiplier = .1f;
                dat.SimulationTime = .0000001f;
                dat.SimulationTime = .0000001f;
                var gridVisualizer = w.GetWorldService<GridVisualizer>();
                gridVisualizer.Enable();
                gridVisualizer.SetGridDiagnostic(new TemperatureGridDiagnostic());
                dat.VelocityField = VelocityField;
            }

            w1.GetWorldService<DataService>().TempratureField = TempTot;
            w2.GetWorldService<DataService>().TempratureField = TempTotNoFlow;
            w0.GetWorldService<DataService>().TempratureField = TempConvection;
            base.Load();
        }
        public override void Draw()
        {
            LayoutMain();
            Title("Convective temperature");
            Presi.ViewPanel("v1", new Vec2(Presi.CanvasCenter.X - 481, 740), new Vec2(1, .5f) * Presi.CanvasSize.X * .5f, .8f);
            Presi.ViewPanel("v2", new Vec2(Presi.CanvasCenter.X + 481, 740), new Vec2(1, .5f) * Presi.CanvasSize.X * .5f, .8f);
            Presi.ViewPanel("v0", new Vec2(Presi.CanvasCenter.X, 260), new Vec2(1, .5f) * Presi.CanvasSize.X * .5f, .8f);
            Presi.Text("Temprature with Flow", new Vec2(Presi.CanvasCenter.X - 470, 990), 64, true, Color.White);
            Presi.Text("Temprature without Flow", new Vec2(Presi.CanvasCenter.X + 470, 990), 64, true, Color.White);
            Presi.Text("-", new Vec2(Presi.CanvasCenter.X - 30, 710), 204, true, Color.White);
            Presi.Text("=", new Vec2(Presi.CanvasCenter.X - 530, 250), 204, true, Color.White);
            //Presi.Text("u(x,y,t) = 0", new Vec2(Presi.CanvasCenter.X, 220), 80, true, Color.White);
            base.Draw();
        }
    }



    public override Slide[] GetSlides()
    {

        return
        [
            new VelocityField1Slide(),
            new VelocityField2Slide(),
            new VelocityField3Slide(),
            new HeatSimIntroSlide(),
            new HeatFlowSlide(),
            new HeatNoFlowSlide(),
            new HeatSubtractSlide(),
            new ConvectiveConvFluxSlide(),
            new ConvectiveDiffFluxSlide(),
        ];
    }

    public static RegularGridVectorField<Vec3, Vec3i, Vec2> DiffFluxField;
    public static RegularGridVectorField<Vec3, Vec3i, Vec2> ConvFluxField;
    public static RegularGridVectorField<Vec3, Vec3i, float> TempConvection;
    public static RegularGridVectorField<Vec3, Vec3i, float> TempTot;
    public static RegularGridVectorField<Vec3, Vec3i, float> TempTotNoFlow;

    public static SpeetjensVelocityField VelocityField = new SpeetjensVelocityField()
    {
        epsilon = .1f
    };
    public override void Setup(FlowExplainer flowExplainer)
    {
        var presentationService = flowExplainer.GetGlobalService<PresentationService>();
        var manager = flowExplainer.GetGlobalService<WorldManagerService>();
        var w0 = manager.NewWorld();
        var w1 = manager.NewWorld();
        var w2 = manager.NewWorld();

        string fieldsFolder = "speetjens-computed-fields";
        //ComputeSpeetjensFields(dataService, fieldsFolder);

        DiffFluxField = RegularGridVectorField<Vec3, Vec3i, Vec2>.Load(Path.Combine(fieldsFolder, "diffFlux.field"));
        ConvFluxField = RegularGridVectorField<Vec3, Vec3i, Vec2>.Load(Path.Combine(fieldsFolder, "convectiveHeatFlux.field"));
        TempConvection = RegularGridVectorField<Vec3, Vec3i, float>.Load(Path.Combine(fieldsFolder, "tempConvection.field"));
        TempTot = RegularGridVectorField<Vec3, Vec3i, float>.Load(Path.Combine(fieldsFolder, "tempTot.field"));
        TempTotNoFlow = RegularGridVectorField<Vec3, Vec3i, float>.Load(Path.Combine(fieldsFolder, "tempNoFlow.field"));

        w0.GetWorldService<DataService>().VelocityField = DiffFluxField;
        w0.GetWorldService<DataService>().VelocityField = VelocityField;

        w0.GetWorldService<FlowFieldVisualizer>().Enable();

        w1.GetWorldService<DataService>().TempratureField = TempTot;
        presentationService.Presi.GetView("v0").World = w0;
        presentationService.Presi.GetView("v1").World = w1;
        presentationService.Presi.GetView("v2").World = w2;

    }
}