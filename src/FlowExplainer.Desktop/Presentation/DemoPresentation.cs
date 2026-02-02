namespace FlowExplainer;

public class DemoPresentation : NewPresentation
{
    public double sliderValue;

    public override void Draw()
    {
        CurrentLayout = null;
        if (BeginSlide("Latex Test"))
        {
            Title("Latex Test");
            Presi.MainParagraph(
                @"Description text, bla bla latex formula:




Next formula:
");
            Presi.LatexCentered(
                @"$$A(\mathbf{x}) \approx \sum_{i} F(\|\mathbf{x} - \mathbf{x}_i\|, \tau_i)$$",
                new Vec2(0.5, 0.63), .2);

            Presi.LatexCentered(
                @"$$\frac{d \mathbf{x}^{M}}{d t} = M_n\bigl(x(t), t\bigr)
\;\Rightarrow\;
x^{M}(t) = x_0 + \int_{0}^{t} M_n\bigl(x(\eta), \eta\bigr)\, d\eta
\;\equiv\;
\Phi_{t}^{M}(x_0).$$",
                new Vec2(0.5, 0.36), .15);
            /*Presi.Text("top", new Vec2(0.5, 0.1-.05), .01, true, Color.Red);
            Presi.Text("bot", new Vec2(0.5, 0.0-.05), .01, true, Color.Red);*/
            // Presi.MainParagraph("Test");
        }

        if (BeginSlide("Title Slide"))
        {
            Title("Demo Presentation");
            Presi.Slider("test", ref sliderValue, 0, 1, new Vec2(.5f, .4f), .5f);
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

            Title("Temperature");

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

            Presi.Text("Movement between steps", new Vec2(.2f + Presi.CurrentStep / 3f, .5f), .03f, true, Color.White);
        }

        if (BeginSlide("Slide 3"))
        {
            Title("Paragraphs");
            Presi.MainParagraph("First line.\r\nSecond line.\r\nThird line");
        }
    }
}