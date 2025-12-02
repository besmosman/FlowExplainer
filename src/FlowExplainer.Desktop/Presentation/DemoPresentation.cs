namespace FlowExplainer;

public class DemoPresentation : NewPresentation
{
    public double d;
    public override void Draw()
    {
        CurrentLayout = null;
        if (BeginSlide("Title Slide"))
        {
            Title("Demo Presentation");
            Presi.Slider("test", ref d, 0, 1, new Vec2(.5f, .4f), .5f);
            // Presi.MainParagraph("Test");
        }

        if (BeginSlide("World Panel"))
        {
            var world = DrawWorldPanel(new Vec2(.5, .5), new Vec2(1, .8), zoom: .8,
                load: (world) =>
                {
                    var data = world.GetWorldService<DataService>();
                    var axis = world.AddVisualisationService<AxisVisualizer>();
                    axis.DrawTitle = false;
                    data.SetDataset("Double Gyre EPS=0.1, Pe=100");
                    data.TimeMultiplier = 1f;
                    data.currentSelectedScaler = "Total Temperature";
                    var grid = world.AddVisualisationService<GridVisualizer>();
                    grid.SetGridDiagnostic(new ScalerGridDiagnostic());
                    grid.WaitForComputation();
                });

            Title("Temprature");

            //   world.GetWorldService<DataService>().SimulationTime = .2f;
        }

        //CurrentLayout = LayoutMain;
        if (BeginSlide("Slide 2"))
        {
            Title("Multiple steps per slide");
            if (IsFirstStep())
            {
                Presi.MainParagraph("Step 0");
            }
            if (BeginStep())
            {
                Presi.MainParagraph("Step 1");
            }
            if (BeginStep())
            {
                Presi.MainParagraph("Step 2");
            }
        }

        if (BeginSlide("Slide 3"))
        {
            Title("Paragraphs");
            Presi.MainParagraph("First line.\r\nSecond line.\r\nThird line");
        }
    }
}