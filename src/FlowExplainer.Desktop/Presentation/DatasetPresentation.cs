using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class DatasetPresentation2 : Presentation
{
    public class IntroSlide : Slide
    {
        public override void Draw()
        {
            LayoutTitle();
            Presi.Text("Dataset Presentation", new Vec2(Presi.CanvasCenter.X, Presi.CanvasCenter.Y + 75), 100, true, Color.White);
            base.Draw();
        }
    }

    public class VelocityField1Slide : Slide
    {
        public override void Draw()
        {
            LayoutMain();
            Title("Velocity Field");
            Presi.Text("u(@blue[x],@red[y]) =  sin(2π@blue[x])cos(2π@red[y]) \r\n v(@blue[x],@red[y]) = -cos(2π@blue[x])sin(2π@red[y]) \r\n @blue[x] : [0, 1],  @red[y] : [0, 0.5]  ", new Vec2(Presi.CanvasCenter.X, 700), 96, true,
                Color.White);
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
            Presi.GetView("v0").World.GetWorldService<ArrowVisualizer>().Enable();
            Presi.GetView("v0").World.GetWorldService<ArrowVisualizer>().colorByGradient = true;
            Presi.GetView("v0").World.GetWorldService<DataService>().ColorGradient = Gradients.Parula;
            Presi.GetView("v0").World.GetWorldService<DataService>().TimeMultiplier = .4f;
            Presi.GetView("v0").World.GetWorldService<DataService>().SimulationTime = .0f;
            Presi.GetView("v0").World.GetWorldService<AxisVisualizer>().DrawWalls = false;
            base.OnEnter();
        }

        public override void OnLeave()
        {
            Presi.GetView("v0").World.GetWorldService<DataService>().TimeMultiplier =0.0;
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
            Presi.GetView("v0").World.GetWorldService<ArrowVisualizer>().Disable();
            //Presi.GetView("v0").World.GetWorldService<DataService>().ColorGradient = Gradients.GetGradient("BlueGrayRed");
            Presi.GetView("v0").World.GetWorldService<DataService>().SimulationTime = .0f;
            Presi.GetView("v0").World.GetWorldService<DataService>().TimeMultiplier = .0f;
            Presi.GetView("v0").World.GetWorldService<GridVisualizer>().Disable();
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
            gridVisualizer.SetGridDiagnostic(new ScalerGridDiagnostic());
            dat.currentSelectedScaler = "No Flow Temperature";
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
            gridVisualizer.SetGridDiagnostic(new ScalerGridDiagnostic());
            dat.currentSelectedVectorField = "Velocity";
            dat.currentSelectedScaler = "Total Temperature";
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
        public double t;

        public ConvectiveConvFluxSlide(double t)
        {
            this.t = t;
        }

        public override void OnEnter()
        {
            Presi.GetView("v0").World.GetWorldService<FlowDirectionVisualization>().amount = 4000;
            Presi.GetView("v0").World.GetWorldService<FlowDirectionVisualization>().Initialize();
            Presi.GetView("v0").World.GetWorldService<FlowDirectionVisualization>().Enable();
            var world = Presi.GetView("v0").World;
            var dat = world.GetWorldService<DataService>();
            dat.SimulationTime = t;
            dat.TimeMultiplier = .0f;
            dat.currentSelectedVectorField = "Convection Flux";
            dat.currentSelectedScaler = "Convective Temperature";
            var flowFieldVisualizer = world.GetWorldService<ArrowVisualizer>();
            flowFieldVisualizer.Enable();
            flowFieldVisualizer.colorByGradient = false;
            Presi.GetView("v0").World.GetWorldService<ArrowVisualizer>().Disable();
            base.Load();
        }

        public override void Draw()
        {
            LayoutMain();
            var w0 = Presi.GetView("v0").World;
            Title($"Convective Temperature + Convective Flux");
            Presi.ViewPanel("v0", Presi.CanvasSize / 2 + new Vec2(0, 00), new Vec2(1, .5f) * Presi.CanvasSize.X * .9f, .8f);
            Presi.Slider("time", ref w0.GetWorldService<DataService>().SimulationTime, 0, 1, new Vec2(Presi.CanvasCenter.X, 100), 200);
            //Presi.Text("u(x,y,t) = 0", new Vec2(Presi.CanvasCenter.X, 220), 80, true, Color.White);
            base.Draw();
        }
    }

    public class ConvectiveDiffFluxSlide : Slide
    {
        public double t;

        public ConvectiveDiffFluxSlide(double t)
        {
            this.t = t;
        }

        public override void OnEnter()
        {
            Presi.GetView("v0").World.GetWorldService<AxisVisualizer>().DrawWalls = false;
            var world = Presi.GetView("v0").World;
            var dat = world.GetWorldService<DataService>();
            Presi.GetView("v0").World.GetWorldService<FlowDirectionVisualization>().amount = 4001;
            Presi.GetView("v0").World.GetWorldService<FlowDirectionVisualization>().Initialize();
            Presi.GetView("v0").World.GetWorldService<FlowDirectionVisualization>().Enable();
            // dat.SimulationTime = t;
            dat.TimeMultiplier = .0f;
            dat.currentSelectedVectorField = "Diffusion Flux";
            dat.currentSelectedScaler = "Convective Temperature";
            var flowFieldVisualizer = world.GetWorldService<ArrowVisualizer>();
            flowFieldVisualizer.Disable();
            flowFieldVisualizer.colorByGradient = false;
            base.Load();
        }

        public override void Draw()
        {
            LayoutMain();
            Title($"Convective Temperature + Diffusion flux");
            Presi.ViewPanel("v0", Presi.CanvasSize / 2 + new Vec2(0, 00), new Vec2(1, .5f) * Presi.CanvasSize.X * .9f, .8f);
            Presi.Slider("time", ref w0.GetWorldService<DataService>().SimulationTime, 0, 1, new Vec2(Presi.CanvasCenter.X, 100), 400);
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
                gridVisualizer.SetGridDiagnostic(new ScalerGridDiagnostic());
                dat.currentSelectedVectorField = "Velocity";
            }

            w0.GetWorldService<ArrowVisualizer>().Disable();
            w1.GetWorldService<DataService>().currentSelectedScaler = "Total Temperature";
            w0.GetWorldService<DataService>().currentSelectedScaler = "Convective Temperature";
            w2.GetWorldService<DataService>().currentSelectedScaler = "No Flow Temperature";
            base.Load();
        }

        public override void Draw()
        {
            LayoutMain();
            Title("Convective temperature");
            Presi.ViewPanel("v1", new Vec2(Presi.CanvasCenter.X - 481, 740), new Vec2(1, .5f) * Presi.CanvasSize.X * .5f, .8f);
            Presi.ViewPanel("v2", new Vec2(Presi.CanvasCenter.X + 481, 740), new Vec2(1, .5f) * Presi.CanvasSize.X * .5f, .8f);
            Presi.ViewPanel("v0", new Vec2(Presi.CanvasCenter.X, 260), new Vec2(1, .5f) * Presi.CanvasSize.X * .5f, .8f);
            Presi.Text("Temperature with Flow", new Vec2(Presi.CanvasCenter.X - 470, 990), 64, true, Color.White);
            Presi.Text("Temperature without Flow", new Vec2(Presi.CanvasCenter.X + 470, 990), 64, true, Color.White);
            Presi.Text("-", new Vec2(Presi.CanvasCenter.X - 30, 710), 204, true, Color.White);
            Presi.Text("=", new Vec2(Presi.CanvasCenter.X - 530, 250), 204, true, Color.White);
            //Presi.Text("u(x,y,t) = 0", new Vec2(Presi.CanvasCenter.X, 220), 80, true, Color.White);
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
                dat.TimeMultiplier =0.0;
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
                gridVisualizer.SetGridDiagnostic(new ScalerGridDiagnostic());
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
            Presi.Slider("time", ref w0.GetWorldService<DataService>().SimulationTime, 0, 1, new Vec2(Presi.CanvasCenter.X, 0), 200);

            foreach (var w in worlds)
                w.GetWorldService<DataService>().SimulationTime = w0.GetWorldService<DataService>().SimulationTime;

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
            new ConvectiveConvFluxSlide(.9f),
            new ConvectiveDiffFluxSlide(.9f),
            new VectorFieldsSlide()
        ];
    }

    public static RegularGridVectorField<Vec3, Vec3i, Vec2> DiffFluxField;
    public static RegularGridVectorField<Vec3, Vec3i, Vec2> ConvFluxField;
    public static RegularGridVectorField<Vec3, Vec3i, double> TempConvection;
    public static RegularGridVectorField<Vec3, Vec3i, double> TempTot;
    public static RegularGridVectorField<Vec3, Vec3i, double> TempTotNoFlow;

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

        /*DiffFluxField = RegularGridVectorField<Vec3, Vec3i, Vec2>.Load(Path.Combine(fieldsFolder, "diffFlux.field"));
        ConvFluxField = RegularGridVectorField<Vec3, Vec3i, Vec2>.Load(Path.Combine(fieldsFolder, "convectiveHeatFlux.field"));
        TempConvection = RegularGridVectorField<Vec3, Vec3i, double>.Load(Path.Combine(fieldsFolder, "tempConvection.field"));
        TempTot = RegularGridVectorField<Vec3, Vec3i, double>.Load(Path.Combine(fieldsFolder, "tempTot.field"));
        TempTotNoFlow = RegularGridVectorField<Vec3, Vec3i, double>.Load(Path.Combine(fieldsFolder, "tempNoFlow.field"));*/
        Scripting.SetGyreDataset(w0);
        Scripting.SetGyreDataset(w1);
        Scripting.SetGyreDataset(w2);

        w0.GetWorldService<DataService>().currentSelectedVectorField = "Diffusion Flux";
        w0.GetWorldService<DataService>().currentSelectedVectorField = "Velocity";

        w0.GetWorldService<ArrowVisualizer>().Enable();

        w1.GetWorldService<DataService>().currentSelectedScaler = "Total Temperature";
        presentationService.Presi.GetView("v0").World = w0;
        presentationService.Presi.GetView("v1").World = w1;
        presentationService.Presi.GetView("v2").World = w2;
    }
}

public class DatasetPresentation : Presentation
{
    public class IntroSlide : Slide
    {
        public override void Draw()
        {
            LayoutTitle();
            Presi.Text("Dataset Presentation", new Vec2(Presi.CanvasCenter.X, Presi.CanvasCenter.Y + 75), 100, true, Color.White);
            base.Draw();
        }
    }

    public class VelocityField1Slide : Slide
    {
        public override void Draw()
        {
            LayoutMain();
            Title("Velocity Field");
            Presi.Text("u(@blue[x],@red[y]) =  sin(2π@blue[x])cos(2π@red[y]) \r\n v(@blue[x],@red[y]) = -cos(2π@blue[x])sin(2π@red[y]) \r\n @blue[x] : [0, 1],  @red[y] : [0, 0.5]  ", new Vec2(Presi.CanvasCenter.X, 700), 96, true,
                Color.White);
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
            Presi.GetView("v0").World.GetWorldService<ArrowVisualizer>().Enable();
            Presi.GetView("v0").World.GetWorldService<ArrowVisualizer>().colorByGradient = true;
            Presi.GetView("v0").World.GetWorldService<DataService>().ColorGradient = Gradients.Parula;
            Presi.GetView("v0").World.GetWorldService<DataService>().TimeMultiplier = .4f;
            Presi.GetView("v0").World.GetWorldService<DataService>().SimulationTime = .0f;
            Presi.GetView("v0").World.GetWorldService<AxisVisualizer>().DrawWalls = false;
            base.OnEnter();
        }

        public override void OnLeave()
        {
            Presi.GetView("v0").World.GetWorldService<DataService>().TimeMultiplier =0.0;
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
            Presi.GetView("v0").World.GetWorldService<ArrowVisualizer>().Disable();
            //Presi.GetView("v0").World.GetWorldService<DataService>().ColorGradient = Gradients.GetGradient("BlueGrayRed");
            Presi.GetView("v0").World.GetWorldService<DataService>().SimulationTime = .0f;
            Presi.GetView("v0").World.GetWorldService<DataService>().TimeMultiplier = .0f;
            Presi.GetView("v0").World.GetWorldService<GridVisualizer>().Disable();
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
            gridVisualizer.SetGridDiagnostic(new ScalerGridDiagnostic());
            dat.currentSelectedScaler = "No Flow Temperature";
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
            gridVisualizer.SetGridDiagnostic(new ScalerGridDiagnostic());
            dat.currentSelectedVectorField = "Velocity";
            dat.currentSelectedScaler = "Total Temperature";
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
        public double t;

        public ConvectiveConvFluxSlide(double t)
        {
            this.t = t;
        }

        public override void OnEnter()
        {
            Presi.GetView("v0").World.GetWorldService<AxisVisualizer>().DrawWalls = false;
            var world = Presi.GetView("v0").World;
            var dat = world.GetWorldService<DataService>();
            dat.SimulationTime = t;
            dat.TimeMultiplier = .0f;
            dat.currentSelectedVectorField = "Convection Flux";
            dat.currentSelectedScaler = "Convective Temperature";
            var flowFieldVisualizer = world.GetWorldService<ArrowVisualizer>();
            flowFieldVisualizer.Enable();
            flowFieldVisualizer.colorByGradient = false;
            base.Load();
        }

        public override void Draw()
        {
            LayoutMain();
            Title($"Convective Temperature + Convective Flux (t={t})");
            Presi.ViewPanel("v0", Presi.CanvasSize / 2 + new Vec2(0, 00), new Vec2(1, .5f) * Presi.CanvasSize.X * .9f, .8f);
            //Presi.Text("u(x,y,t) = 0", new Vec2(Presi.CanvasCenter.X, 220), 80, true, Color.White);
            base.Draw();
        }
    }

    public class ConvectiveDiffFluxSlide : Slide
    {
        public double t;

        public ConvectiveDiffFluxSlide(double t)
        {
            this.t = t;
        }

        public override void OnEnter()
        {
            Presi.GetView("v0").World.GetWorldService<AxisVisualizer>().DrawWalls = false;
            var world = Presi.GetView("v0").World;
            var dat = world.GetWorldService<DataService>();
            dat.SimulationTime = t;
            dat.TimeMultiplier = .0f;
            dat.currentSelectedVectorField = "Diffusion Flux";
            dat.currentSelectedScaler = "Convective Temperature";
            var flowFieldVisualizer = world.GetWorldService<ArrowVisualizer>();
            flowFieldVisualizer.Enable();
            flowFieldVisualizer.colorByGradient = false;
            base.Load();
        }

        public override void Draw()
        {
            LayoutMain();
            Title($"Convective Temperature + Diffusion flux (t={t})");
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
                gridVisualizer.SetGridDiagnostic(new ScalerGridDiagnostic());
                dat.currentSelectedVectorField = "Velocity";
            }

            w0.GetWorldService<ArrowVisualizer>().Disable();
            w1.GetWorldService<DataService>().currentSelectedScaler = "Total Temperature";
            w0.GetWorldService<DataService>().currentSelectedScaler = "Convective Temperature";
            w2.GetWorldService<DataService>().currentSelectedScaler = "No Flow Temperature";
            base.Load();
        }

        public override void Draw()
        {
            LayoutMain();
            Title("Convective temperature");
            Presi.ViewPanel("v1", new Vec2(Presi.CanvasCenter.X - 481, 740), new Vec2(1, .5f) * Presi.CanvasSize.X * .5f, .8f);
            Presi.ViewPanel("v2", new Vec2(Presi.CanvasCenter.X + 481, 740), new Vec2(1, .5f) * Presi.CanvasSize.X * .5f, .8f);
            Presi.ViewPanel("v0", new Vec2(Presi.CanvasCenter.X, 260), new Vec2(1, .5f) * Presi.CanvasSize.X * .5f, .8f);
            Presi.Text("Temperature with Flow", new Vec2(Presi.CanvasCenter.X - 470, 990), 64, true, Color.White);
            Presi.Text("Temperature without Flow", new Vec2(Presi.CanvasCenter.X + 470, 990), 64, true, Color.White);
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
            new IntroSlide(),
            new VelocityField1Slide(),
            new VelocityField2Slide(),
            new VelocityField3Slide(),
            new HeatSimIntroSlide(),
            new HeatFlowSlide(),
            new HeatNoFlowSlide(),
            new HeatSubtractSlide(),
            new ConvectiveConvFluxSlide(.9f),
            new ConvectiveDiffFluxSlide(.9f),
            new ConvectiveConvFluxSlide(.2f),
            new ConvectiveDiffFluxSlide(.2f),
        ];
    }

    public static RegularGridVectorField<Vec3, Vec3i, Vec2> DiffFluxField;
    public static RegularGridVectorField<Vec3, Vec3i, Vec2> ConvFluxField;
    public static RegularGridVectorField<Vec3, Vec3i, double> TempConvection;
    public static RegularGridVectorField<Vec3, Vec3i, double> TempTot;
    public static RegularGridVectorField<Vec3, Vec3i, double> TempTotNoFlow;

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

        /*DiffFluxField = RegularGridVectorField<Vec3, Vec3i, Vec2>.Load(Path.Combine(fieldsFolder, "diffFlux.field"));
        ConvFluxField = RegularGridVectorField<Vec3, Vec3i, Vec2>.Load(Path.Combine(fieldsFolder, "convectiveHeatFlux.field"));
        TempConvection = RegularGridVectorField<Vec3, Vec3i, double>.Load(Path.Combine(fieldsFolder, "tempConvection.field"));
        TempTot = RegularGridVectorField<Vec3, Vec3i, double>.Load(Path.Combine(fieldsFolder, "tempTot.field"));
        TempTotNoFlow = RegularGridVectorField<Vec3, Vec3i, double>.Load(Path.Combine(fieldsFolder, "tempNoFlow.field"));*/
        Scripting.SetGyreDataset(w0);
        Scripting.SetGyreDataset(w1);
        Scripting.SetGyreDataset(w2);

        w0.GetWorldService<DataService>().currentSelectedVectorField = "Diffusion Flux";
        w0.GetWorldService<DataService>().currentSelectedVectorField = "Velocity";

        w0.GetWorldService<ArrowVisualizer>().Enable();

        w1.GetWorldService<DataService>().currentSelectedScaler = "Total Temperature";
        presentationService.Presi.GetView("v0").World = w0;
        presentationService.Presi.GetView("v1").World = w1;
        presentationService.Presi.GetView("v2").World = w2;
    }
}