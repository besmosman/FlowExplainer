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
        public override void OnEnter()
        {
            Presi.GetView("v0").World.GetWorldService<FlowFieldVisualizer>().Disable();
            Presi.GetView("v0").World.GetWorldService<DataService>().ColorGradient = Gradients.GetGradient("BlueGrayRed");
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
    
            Presi.GetView("v0").World.GetWorldService<DataService>().TimeMultiplier = .1f;
            Presi.Text("T(x, 0.5, t) = @blue[T]\r\nT(x, 0, t)    = @red[T]", new Vec2(Presi.CanvasCenter.X, 220), 80, true, Color.White);
            Presi.Text("@red[hot]", new Vec2(Presi.CanvasCenter.X+180, 120), 60, false, Color.White);
            Presi.Text("@blue[cold]", new Vec2(Presi.CanvasCenter.X+180, 200), 60, false, Color.White);
            base.Draw();
        }
    }

    
    public class HeatIntro2Slide : Slide
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
        public override void Draw()
        {
            LayoutMain();
            Title("Heat Simulation");
            Presi.ViewPanel("v0", Presi.CanvasSize / 2 + new Vec2(0, 60), new Vec2(1, .5f) * Presi.CanvasSize.X * .8f, .8f);
            //Presi.Text("V2T ¼ Pe$VT", new Vec2(Presi.CanvasCenter.X, 220), 80, true, Color.White);
            Presi.Image(image, new Vec2(Presi.CanvasCenter.X, 200), 500);
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
            new HeatIntro2Slide(),
        ];
    }
    public override void Setup(FlowExplainer flowExplainer)
    {
        var presentationService = flowExplainer.GetGlobalService<PresentationService>();
        var manager = flowExplainer.GetGlobalService<WorldManagerService>();
        var w0 = manager.NewWorld();
        var w1 = manager.NewWorld();

        string fieldsFolder = "speetjens-computed-fields";
        //ComputeSpeetjensFields(dataService, fieldsFolder);

        var diffFlux = RegularGridVectorField<Vec3, Vec3i, Vec2>.Load(Path.Combine(fieldsFolder, "diffFlux.field"));
        var tempTot = RegularGridVectorField<Vec3, Vec3i, float>.Load(Path.Combine(fieldsFolder, "tempConvection.field"));
        w0.GetWorldService<DataService>().VelocityField = diffFlux;
        w1.GetWorldService<DataService>().VelocityField = diffFlux;

        w0.GetWorldService<DataService>().VelocityField = new SpeetjensVelocityField()
        {
            epsilon = .1f
        };
        w0.GetWorldService<FlowFieldVisualizer>().Enable();

        w1.GetWorldService<DataService>().TempratureField = tempTot;
        presentationService.Presi.GetView("v0").World = w0;

    }
}