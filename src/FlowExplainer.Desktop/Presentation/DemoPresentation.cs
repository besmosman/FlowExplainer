using FlowExplainer.Msdf;

namespace FlowExplainer;

public class ResizableArray<T> where T : struct
{
    private T[] Entries;

    public ResizableArray(int c)
    {
        Entries = new T[c];
    }

    public ref T this[int index] => ref Entries[index];

    public int Length
    {
        get => Entries.Length;
    }
    
    public bool ResizeIfNeeded(int c, bool reset = false)
    {
        if (Entries.Length != c)
        {
            if (reset)
                Entries = new T[c];
            else
                Array.Resize(ref Entries, c);
            return true;
        }

        return false;
    }

    public Span<T> AsSpan()
    {
        return Entries.AsSpan();
    }
}

public class ClusterPresentation : NewPresentation
{
    public class StructureAccentuatingService : WorldService
    {
        
        public override void Initialize()
        {
        }

        public override void Draw(View view)
        {
        }
    }

    public override void Draw()
    {
        if (BeginSlide())
        {
            var tempRelSize = new Vec2(1, .5) / 1.6;
            var tempRelPos = new Vec2(.25, .75);
            var temp = DrawWorldPanel(tempRelPos, tempRelSize, zoom: .76,
                load: (world) =>
                {
                    var data = world.GetWorldService<DataService>();
                    data.SetDataset("Double Gyre EPS=0.1, Pe=100");
                    data.TimeMultiplier = .5f;
                    data.currentSelectedVectorField = "Total Flux";
                    var axis = world.AddVisualisationService<AxisVisualizer>();
                    var grid = world.AddVisualisationService<StructureAccentuatingService>();
                    axis.DrawTitle = false;
                });
        }
    }
}

public class DemoPresentation : NewPresentation
{
    public double sliderValue;

    public override void Draw()
    {
        CurrentLayout = null;
        if (false && BeginSlide("Latex Test"))
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
            Title($"{Presi.View.Camera2D.RenderTargetSize}");
            Presi.Slider("test", ref sliderValue, 0, 1, new Vec2(.5f, .4f), .5f);
            // Presi.MainParagraph("Test");
        }

        if (BeginSlide("World Panel"))
        {
            var flowRelSize = new Vec2(1, .5) / 1.6;
            var tempRelSize = new Vec2(1, .5) / 1.6;
            var tempRelPos = new Vec2(.25, .75);
            var flowRelPos = new Vec2(.25, .25);

            if (IsFirstStep())
            {
                tempRelPos = new Vec2(0.5, 0.5);
                tempRelSize = new Vec2(1, .5) / 1;
            }

            var temp = DrawWorldPanel(tempRelPos, tempRelSize, zoom: .76,
                load: (world) =>
                {
                    var data = world.GetWorldService<DataService>();
                    data.SetDataset("Double Gyre EPS=0.1, Pe=100");
                    data.TimeMultiplier = .5f;
                    data.currentSelectedVectorField = "Total Flux";
                    var grid = world.AddVisualisationService<ArrowVisualizer>();
                    var axis = world.AddVisualisationService<AxisVisualizer>();
                    axis.DrawTitle = false;
                });
            if (BeginStep())
            {
                var vel = DrawWorldPanel(flowRelPos, flowRelSize, zoom: .76,
                    load: (world) =>
                    {
                        var data = world.GetWorldService<DataService>();
                        data.SetDataset("Double Gyre EPS=0.1, Pe=100");
                        data.TimeMultiplier = .5f;
                        data.currentSelectedScaler = "Convective Temperature";
                        var grid = world.AddVisualisationService<GridVisualizer>();
                        grid.SetGridDiagnostic(new ScalerGridDiagnostic());
                        grid.WaitForComputation();
                        var axis = world.AddVisualisationService<AxisVisualizer>();
                        data.ColorGradient = Gradients.GetGradient("BlueGrayRed");
                        axis.DrawTitle = false;
                    });
                vel.World.GetWorldService<DataService>().SimulationTime = temp.World.DataService.SimulationTime;
            }


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