namespace FlowExplainer;

public class FirstPresentation : Presentation
{
    public class ProgressSlide : Slide
    {
        public override void Draw()
        {
            LayoutMain();
            Title("3D visualizations");
            Presi.MainParagraph(
                @"
PhD Project (30%)
    - Reading
    - Experimenting
    - Double Gyre / Bickley Jet
    - Poincaré sections

    - 3D visualizations
    - Simple Heat Simulation
    - Diagnostics: FTLE / Velocity magnitude

Graduation Paper (70%)
    - IST abstract submitted/accepted
    - Modifications for new fetal dataset
    - Validation
");
            //Presi.ViewPanel("main", (Presi.CanvasSize - new Vec2(0, topbarHeight)) / 2, new Vec2(1920, 1000));
            base.Draw();
        }
    }


    public class RecapSlide : Slide
    {

        public override void Draw()
        {
            LayoutMain();
            Title("Recap");
            Presi.MainParagraph(
                @"
- Focus on graduation paper
- Replicate Speetjens 2012
- Reading
");
            base.Draw();
        }
    }

    public class DemoSlide : Slide
    {
        public override void Load()
        {
            /*var newWorld = Presi.View.World.FlowExplainer.GetGlobalService<WorldManagerService>().NewWorld();
            var newView = Presi.View.World.FlowExplainer.GetGlobalService<ViewsService>().NewView();
            Presi.GetView("second").World = newWorld;
            newView.World = newWorld;*/
            base.Load();
        }
        public override void Draw()
        {
            LayoutMain();
            Presi.ViewPanel("second", Presi.CanvasSize / 2, new Vec2(1,.5f) * Presi.CanvasSize.X*.9f, 1.3f);
            Title("Demo");
            base.Draw();
        }
    }
    
    
    public class Demo2Slide : Slide
    {
        public override void Load()
        {
            /*var newWorld = Presi.View.World.FlowExplainer.GetGlobalService<WorldManagerService>().NewWorld();
            var newView = Presi.View.World.FlowExplainer.GetGlobalService<ViewsService>().NewView();
            Presi.GetView("second").World = newWorld;
            newView.World = newWorld;*/
            base.Load();
        }

        public override void OnEnter()
        {
            base.OnEnter();
        }
        public override void Draw()
        {
            LayoutMain();
            Title("Demo 2");
            var width = 800;
            var spacing = 100f;
            Presi.ViewPanel("left", Presi.CanvasSize / 2 - new Vec2(width/2f + spacing/2f,00), new Vec2(1,.5f) * width, 1f);
            Presi.ViewPanel("right", Presi.CanvasSize / 2 + new Vec2(width/2f + spacing/2f,00), new Vec2(1,.5f) * width, 1f);
            base.Draw();
        }
    }


    public override Slide[] GetSlides()
    {
        return
        [
            new RecapSlide(),
            new DemoSlide(),
            new Demo2Slide(),
            new ProgressSlide(),
        ];
    }
    public override void Setup(FlowExplainer flowExplainer)
    {
    }
}