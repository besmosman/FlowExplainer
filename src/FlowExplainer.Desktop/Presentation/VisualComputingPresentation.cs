using Microsoft.VisualBasic;
using OpenTK.Graphics.ES11;

namespace FlowExplainer;

public class VisualComputingPresentation : NewPresentation
{
    public double last_T;
    public override void Draw()
    {
        if (BeginSlide())
        {
            Presi.Image(vcp, new Vec2(0.5,0.4), .75f);
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

            if (BeginStep())
            {
                Title("Hot/Cold Wall");
                var worldPanel = Presi.GetWidgetData("#w", 0);
                var grad = view.World.GetWorldService<DataService>().ColorGradient;
                var lb = WorldToScreenRel(worldPanel, new Vec2(0, 0.0));
                var rb = WorldToScreenRel(worldPanel, new Vec2(1, 0.0));
                var lt = WorldToScreenRel(worldPanel, new Vec2(0, 0.5));
                var rt = WorldToScreenRel(worldPanel, new Vec2(1, 0.5));
                //var rect= new Rect<Vec2>(worldPanel.RenderMin, worldPanel.RenderMax);
                //Gizmos2D.Circle(Presi.View.Camera2D, WorldToScreenRel(worldPanel, new Vec2(0.5,0.25)), Color.White,10f);
                //  if(isFirst)
                //  Gizmos2D.Rect(Presi.View.Camera2D, lb, rt, grad.Get(0.2f));
                Gizmos2D.Line(Presi.View.Camera2D, lb, rb, grad.Get(1), 15f);
                Gizmos2D.Line(Presi.View.Camera2D, lt, rt, grad.Get(0.1f), 15f);
                Gizmos2D.AdvText(Presi.View.Camera2D, rb + new Vec2(50,0), 100f, grad.Get(1), "Hot");
                Gizmos2D.AdvText(Presi.View.Camera2D, rt + new Vec2(50,0), 100f, grad.Get(0.1f), "Cold");
            }

            if (BeginStep())
            {
                Title("Heat Simulation (no flow)");
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
                Presi.Text("u =  sin(πx)cos(πy) v = -cos(π(x)sin(πy)", new Vec2(0.5, 0.05), .05f, true, Color.White);
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

            if (BeginStep())
            {
                Title("Flux");
                if (view.World.GetWorldService<ArrowVisualizer>() == null)
                {
                    view.World.AddVisualisationService<ArrowVisualizer>().colorByGradient = false;
                    view.World.DataService.currentSelectedVectorField = "Diffusion Flux";
                    view.World.DataService.TimeMultiplier = .0f;
                }
                ref var t = ref view.World.DataService.SimulationTime;
                Presi.Slider("time", ref t, 0, 4, new Vec2(0.5, 0.1f), .6);
                last_T = t;
            }
        }
        if (BeginSlide())
        {
            var relCenterPos = new Vec2(.5, .5);
            var relSize = new Vec2(1, .5) * .9;
           
            Title("Flux");

            var view = DrawWorldPanel(relCenterPos, relSize, zoom: .8,
                load: (world) =>
                {
                    var data = world.GetWorldService<DataService>();
                    var axis = world.AddVisualisationService<AxisVisualizer>();
                    axis.DrawTitle = false;
                    axis.DrawGradient = true;
                    data.SetDataset("Double Gyre EPS=0.1, Pe=100");
                    data.TimeMultiplier = .0f;
                    data.SimulationTime = last_T;
                    data.currentSelectedScaler = "Total Temperature";
                    data.currentSelectedVectorField = "Convection Flux";
                    data.currentSelectedVectorField = "Diffusion Flux";
                    //world.AddVisualisationService<ArrowVisualizer>();
                    var f = world.AddVisualisationService<FlowDirectionVisualization>();
                    f.amount = 6000;
                    f.opacity *= 1.4f;
                    f.speed *= -2;
                });
            ref var t = ref view.World.DataService.SimulationTime;

           
            /*if (BeginStep())
            {
                if (view.World.GetWorldService<FlowDirectionVisualization>() != null)
                {
                    view.World.RemoveWorldService(view.World.GetWorldService<ArrowVisualizer>());
                }
            }*/
            Presi.Slider("time", ref t, 0, 4, new Vec2(0.5, 0.1f), .6);

        }
        if (BeginSlide())
        {
            var relCenterPos = new Vec2(.5, .5);
            var relSize = new Vec2(.8, .4);
            
            var sliderPos = new Vec2(0.5, 0.1f);
            double sliderW = .6;
            if (IsFirstStep())
            {
            Title("Evolving Structures Over Time");
            }
            else
            {
                //Title("2D Structures => 3D Spacetime Visualization");
                relCenterPos.X = .2f;
                relSize = new Vec2(0.5f, 0.25f);

                sliderPos = new Vec2(0.2f, 0.1f);
                sliderW = 0.4f;
                Presi.Image(_3d, new Vec2(0.7,0.5), .45f);

            }
            var view = DrawWorldPanel(relCenterPos, relSize, load: w =>
            {
                w.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
                w.DataService.currentSelectedVectorField = "Diffusion Flux";
                var stoch = w.AddVisualisationService<StochasticVisualization>();
                w.DataService.SimulationTime = 1.5f;
                stoch.dt = .3f;
                stoch.reverse = true;
                stoch.ReseedChance = .1f;
                //  w.AddVisualisationService<ArrowVisualizer>().colorByGradient = false;
            });
            ref var t = ref view.World.DataService.SimulationTime;

            if (IsFirstStep())
            {
                view.World.DataService.TimeMultiplier = 0;
                Presi.Slider("time", ref t, 0, 4, sliderPos, sliderW);

            }
            else
            {
                view.World.DataService.TimeMultiplier = .4f;
                if (view.World.DataService.SimulationTime > 3)
                    view.World.DataService.SimulationTime = 0;
                Presi.Text("Evolving 2D Structures", new Vec2(0.25,0.8), 0.04, true, Color.White);
                Presi.Text("3D Spacetime Visualization", new Vec2(0.75,0.8), 0.04, true, Color.White);
            }
            last_T = t;
           
            
            if (BeginStep())
            {
                
            }
        }

        if (BeginSlide())
        {
            Presi.Text("Simple Example Pipeline", new Vec2(0.5,0.56), .08f, true, Color.White);
            Presi.Text("(one of many options)", new Vec2(0.5,0.45), .05f, true, Color.Grey(0.8f));
        }
        if (BeginSlide())
        {
            Title("Finding Stagnation Structures");
            var view = DrawWorldPanel(new Vec2(.5, .5), new Vec2(.8, .4), load: w =>
            {
                w.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
                w.DataService.currentSelectedVectorField = "Diffusion Flux";
                w.DataService.currentSelectedScaler = "Convective Temperature";
                w.DataService.SimulationTime = last_T;
                var axisVisualizer = w.AddVisualisationService<AxisVisualizer>();
                axisVisualizer.DrawGradient = true;
                axisVisualizer.DrawTitle = false;
                var stoch = w.AddVisualisationService<GridVisualizer>();
                stoch.DataService.ColorGradient = Gradients.GetGradient("BlueGrayRed");
                stoch.SetGridDiagnostic(new ScalerGridDiagnostic());
                //  w.AddVisualisationService<ArrowVisualizer>().colorByGradient = false;
            }, zoom: .8f);
            ref var t = ref view.World.DataService.SimulationTime;
            Presi.Slider("time", ref t, 0, 4, new Vec2(0.5, 0.1f), .6);

            if (BeginStep())
            {
                if (view.World.GetWorldService<Vis2D>() == null)
                {
                    view.World.AddVisualisationService<Vis2D>();
                }
            }

            if (BeginStep())
            {
                view.World.GetWorldService<Vis2D>().move = true;
            }
            if (BeginStep())
            {
                if (view.World.GetWorldService<GridVisualizer>().IsEnabled)
                    view.World.GetWorldService<GridVisualizer>().Disable();
            }
        }

        if (BeginSlide())
        {
            Title("Spacetime View");
            var view = DrawWorldPanel(new Vec2(.5, .5), new Vec2(1, .8), load: w =>
            {
                w.DataService.SetDataset("(P) Double Gyre EPS=0.1, Pe=100");
                w.DataService.currentSelectedVectorField = "Diffusion Flux";
                w.DataService.currentSelectedScaler = "Convective Temperature";
                var axisVisualizer = w.AddVisualisationService<Axis3D>();
                var s = w.AddVisualisationService<SpaceTimeSurfaceStructureExtractor2>();
                s.ParticleCount = 200000;
                s.Initialize();
                s.Radius = 0.004f;
                //  w.AddVisualisationService<ArrowVisualizer>().colorByGradient = false;
            }, zoom: .8f);
            if (!view.Is3DCamera)
            {
                view.Is3DCamera = true;
                view.CameraZoom = 26;
                view.CameraOffset = -new Vec3(0.5f, 0.25, .5f);
            }
            ref var t = ref view.World.GetWorldService<SpaceTimeSurfaceStructureExtractor2>().TargetValue;
            GL.Disable(EnableCap.DepthTest);
            Presi.Slider("target", ref t, -1, 1, new Vec2(0.5, 0.1), .5);

            if (BeginStep())
            {
                if (view.World.DataService.LoadedDataset.Name != "(P) Double Gyre EPS=0.1, Pe=500")
                {
                    view.World.DataService.SetDataset("(P) Double Gyre EPS=0.1, Pe=500");
                    view.World.GetWorldService<SpaceTimeSurfaceStructureExtractor2>().Initialize();
                }
            }
        }
        if (BeginSlide())
        {
            var image = challenge0;
            if (!IsFirstStep())
                image = challenge1;
            Presi.Image(image, new Vec2(0.5, 0.5), 1);
            if (BeginStep())
            {

            }
        }

        if (BeginSlide())
        {
            var view = DrawWorldPanel(new Vec2(.5, .5), new Vec2(1, .5), load: w =>
            {
                w.DataService.SetDataset("(P) Double Gyre EPS=0, Pe=100");
                w.DataService.currentSelectedVectorField = "Diffusion Flux";
                var stoch = w.AddVisualisationService<DensityPathStructures>();
                stoch.InfluenceRadius = 0.003;
                stoch.AccumelationFactor = .4f;
                stoch.reseedRate = .009;
                stoch.Decay = 0.06;
                stoch.ParticleCount = 10000;
                stoch.Initialize();
                w.DataService.TimeMultiplier = -3f;
                w.DataService.ColorGradient = Gradients.GetGradient("matlab_hot");
                //  w.AddVisualisationService<ArrowVisualizer>().colorByGradient = false;
            }, zoom:1.1f);
        }
    }

    public Texture challenge0 = new ImageTexture("Assets/Images/presi/challenge-0.png");
    public Texture challenge1 = new ImageTexture("Assets/Images/presi/challenge-1.png");
    public Texture vcp = new ImageTexture("Assets/Images/presi/vcp.png");
    public Texture _3d = new ImageTexture("Assets/Images/presi/3d.png");

    public class Vis2D : WorldService
    {

        private Vec2[] particles;
        public bool move;
        public override void Initialize()
        {
            particles = new Vec2[500];
            foreach (ref var p in particles.AsSpan())
            {
                p = Utils.Random(new Rect<Vec2>(Vec2.Zero, new Vec2(1, .5f)));
            }
        }
        public override void Draw(View view)
        {
            if (move)
            {
                var ScalerField = view.World.GetSelectableVectorFields<Vec2, double>().First().VectorField;
                double t = view.World.DataService.SimulationTime;
                foreach (ref var p in particles.AsSpan())
                {
                    var grad = ScalerField.FiniteDifferenceGradient(p, .0001f).NormalizedSafe();
                    var dis = (ScalerField.Evaluate(p) - 0);
                    p += grad * dis * -.01f;
                }
            }
            foreach (ref var p in particles.AsSpan())
            {
                if (p.Y > .01f)
                    Gizmos2D.Instanced.RegisterCircle(p, .01f, Color.Green);
            }
            Gizmos2D.Instanced.RenderCircles(view.Camera2D);
        }
    }
}