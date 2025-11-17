namespace FlowExplainer;

public class ProgressPresentation : Presentation
{

    public class SmearSlide : Slide
    {
        public ImageTexture Image = new ImageTexture("Assets/Images/presi/smear.png");

        public override void Draw()
        {
            LayoutMain();
            Title("Poincare Structures over time");
            Presi.Image(Image, Presi.CanvasCenter - new Vec2(0, 100), 1800);
            base.Draw();
        }
    }

    public class DensitySlide : Slide
    {
        public ImageTexture Image = new ImageTexture("Assets/Images/presi/density.png");

        public override void Draw()
        {
            LayoutMain();
            Title("Poincare Density Maps");
            Presi.Image(Image, Presi.CanvasCenter - new Vec2(0, 100), 1800);
            base.Draw();
        }
    }

    public class MapSlide : Slide
    {
        public ImageTexture Image = new ImageTexture("Assets/Images/presi/poincaremap.png");

        public override void Draw()
        {
            LayoutMain();
            Title("Max Density Map");
            Presi.Image(Image, Presi.CanvasCenter - new Vec2(0, 100), 1800);
            base.Draw();
        }
    }


    public class CoherentPointPhase3DSlide : Phase3DSlide
    {
        public override string Title => "Coherent trajectory";

        public override IEnumerable<Vec2> GetSeeds()
        {
            return [new Vec2(.27f, .46f)];
        }
    }

    public class ChaosPointPhase3DSlide : Phase3DSlide
    {
        public override string Title => "Chaotic trajectory";

        public override IEnumerable<Vec2> GetSeeds()
        {
            return [new Vec2(.45f, .45f)];
        }
    }


    public class GridPointPhase3DSlide : Phase3DSlide
    {
        public override string Title => "Lines";
        public override IEnumerable<Vec2> GetSeeds()
        {

            for (int j = 0; j < 20; j++)
            {
                yield return new Vec2(j / 20f, .25f);
            }


        }
    }


    public abstract class Phase3DSlide : Slide
    {
        public abstract string Title { get; }
        public abstract IEnumerable<Vec2> GetSeeds();
        public bool many = false;
        public override void OnEnter()
        {
            v0.Camera.Position = new Vec3(-0.09871601f, -0.1381484f, -9.768521f);
            v0.CameraYaw = -6.9f;
            v0.CameraZoom = 10;
            v0.CameraPitch = 1.14f;
            v0.Is3DCamera = true;
            var poincare3DVisualizer = w0.GetWorldService<Poincare3DVisualizer>();
            poincare3DVisualizer.Enable();
            poincare3DVisualizer.t = 0;
            poincare3DVisualizer.speed = 0;
            var enumerable = GetSeeds().ToArray();
            poincare3DVisualizer.periods = 10000;
            if (enumerable.Length >= 5)
            {
                poincare3DVisualizer.periods = 500;
                many = true;
            }
            poincare3DVisualizer.SetupTrajects(enumerable);
            base.OnEnter();
        }
        public override void OnLeave()
        {
            v0.Is3DCamera = false;
            w0.GetWorldService<Poincare3DVisualizer>().Disable();
            base.OnLeave();
        }
        public bool speedup = false;
        public override void Draw()
        {
            LayoutMain();
            Title($"Poincare Sections: {Title}");
            // v0.CameraZoom =20f;
            var maxT = 10;
            Presi.ViewPanel("v0", Presi.CanvasSize / 2 + new Vec2(0, 00), new Vec2(1, .5f) * Presi.CanvasSize.X * .9f, .8f);
            Presi.Slider("slice", ref w0.GetWorldService<Poincare3DVisualizer>().sliceT, 0, 1, new Vec2(700, 100), 400);
            Presi.Slider("speed", ref w0.GetWorldService<Poincare3DVisualizer>().speed, 0, maxT, new Vec2(Presi.CanvasCenter.X * (4 / 3f), 100), 400);
            bool old = speedup;
            Presi.Checkbox("speedup", ref speedup, new Vec2(Presi.CanvasCenter.X * (4 / 3f) + 350, 100), "speedup");
            bool tubeFactor = w0.GetWorldService<Poincare3DVisualizer>().tubeFactor > .5f;
            bool lastTube = tubeFactor;
            Presi.Checkbox("tube", ref tubeFactor, new Vec2(200, 100), "tube");
            if (lastTube != tubeFactor)
            {
                if (tubeFactor)
                {
                    w0.GetWorldService<Poincare3DVisualizer>().tubeFactor = 1;
                    w0.GetWorldService<Poincare3DVisualizer>().UpdateLines(1);
                }
                else
                {
                    w0.GetWorldService<Poincare3DVisualizer>().tubeFactor = 0;
                    w0.GetWorldService<Poincare3DVisualizer>().UpdateLines(1);
                }
            }
            if (old != speedup)
            {
                if (speedup)
                    w0.GetWorldService<Poincare3DVisualizer>().speed = 1000;
                else
                {
                    w0.GetWorldService<Poincare3DVisualizer>().speed = 1;
                }
            }
            if (speedup && w0.GetWorldService<Poincare3DVisualizer>().speed < 1000)
                speedup = false;
            base.Draw();
        }
    }

    public class MixingSlide : Slide
    {
        public override void OnEnter()
        {
            var heatSim = w0.GetWorldService<HeatSimulationService>();
            heatSim.Enable();
            heatSim.particleSpacing = .0025f;
            var vis = w0.GetWorldService<HeatSimulationVisualizer>();
            vis.Enable();
            vis.RenderRadius = .002f;
            w0.GetWorldService<DataService>().ColorGradient = Gradients.Parula;
            w0.GetWorldService<DataService>().SimulationTime = 0;
            w0.GetWorldService<DataService>().TimeMultiplier = 0;
            vis.Coloring = HeatSimulationVisualizer.Colorings.Tag;
            heatSim.Reset();
            base.OnEnter();
        }

        public override void Draw()
        {
            LayoutMain();
            Title("Flow Structures");
            Presi.ViewPanel("v0", Presi.CanvasSize / 2 + new Vec2(0, 00), new Vec2(1, .5f) * Presi.CanvasSize.X * .9f, .8f);
            Presi.Slider("speed", ref w0.GetWorldService<DataService>().TimeMultiplier, 0, 10, new Vec2(Presi.CanvasCenter.X, 100), 900);
            base.Draw();
        }
    }

    public override Slide[] GetSlides()
    {
        return
        [

            new MixingSlide(),
            new CoherentPointPhase3DSlide(),
            new ChaosPointPhase3DSlide(),
            new GridPointPhase3DSlide(),
            new DensitySlide(),
            new MapSlide(),
            new SmearSlide(),
        ];
    }

    public override void Setup(FlowExplainer flowExplainer)
    {
        var presentationService = flowExplainer.GetGlobalService<PresentationService>();
        var manager = flowExplainer.GetGlobalService<WorldManagerService>();
        var w0 = manager.NewWorld();
        Scripting.SetGyreDataset(w0);
        w0.GetWorldService<DataService>().currentSelectedVectorField = "Velocity";
        presentationService.Presi.GetView("v0").World = w0;
    }
}