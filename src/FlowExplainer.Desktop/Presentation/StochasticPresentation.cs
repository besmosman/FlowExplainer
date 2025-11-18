using Microsoft.VisualBasic;

namespace FlowExplainer;

public class StochasticPresentation : Presentation
{

    public static string SteadyDatasetNameP = "(P) Double Gyre EPS=0, Pe=100";
    public static string UnsteadyDatasetNameP = "(P) Double Gyre EPS=0.1, Pe=100";
    public static string SteadyDatasetName = "Double Gyre EPS=0, Pe=100";
    public static string UnsteadyDatasetName = "Double Gyre EPS=0.1, Pe=100";

    /*
| Story
- Show steady vector field
- Divergence != attracting/repelling structures
- Stagnation
-
     */

    public static void MainPanel(Slide slide)
    {
        slide.Presi.ViewPanel("v0", slide.Presi.CanvasSize / 2 + new Vec2(0, 00), new Vec2(1, .5f) * slide.Presi.CanvasSize.X * .9f, .8f);
    }

    public class TotalFluxVectorfield : Slide
    {

        public override void OnLeave()
        {
            base.OnLeave();
        }
        public override void OnEnter()
        {

            w0.DataService.currentSelectedVectorField = "Total Flux";
            base.OnEnter();
        }
        public override void Draw()
        {
            LayoutMain();
            Title("Total Convection Flux");
            MainPanel(this);
            base.Draw();
        }
    }

    public class DiffusionVectorfield : Slide
    {

        public override void OnLeave()
        {
            base.OnLeave();
        }
        public override void OnEnter()
        {
            w0.GetWorldService<FlowDirectionVisualization>().amount = 5000;
            w0.GetWorldService<FlowDirectionVisualization>().Enable();
            w0.GetWorldService<FlowDirectionVisualization>().opacity = 1;
            w0.DataService.currentSelectedVectorField = "Diffusion Flux";
            base.OnEnter();
        }

        public override void Draw()
        {
            LayoutMain();
            Title("Diffusion Flux");
            MainPanel(this);
            base.Draw();
        }
    }

    public class DivergenceSlide : Slide
    {

        public override void OnLeave()
        {
            w0.GetWorldService<GridVisualizer>().Disable();

            base.OnLeave();
        }
        public override void OnEnter()
        {
            w0.GetWorldService<GridVisualizer>().Enable();
            w0.GetWorldService<GridVisualizer>().TargetCellCount = 1000;
            w0.GetWorldService<GridVisualizer>().SetGridDiagnostic(new DivergenceGridDiagnostic());
            base.OnEnter();
        }

        public override void Draw()
        {
            w0.GetWorldService<GridVisualizer>().Continous = false;
            LayoutMain();
            Title("Divergence (Total Convection Flux)");
            MainPanel(this);
            base.Draw();
        }
    }

    public class MagnitudeSlide : Slide
    {
        public override void OnEnter()
        {
            w0.GetWorldService<GridVisualizer>().Enable();
            w0.GetWorldService<GridVisualizer>().SetGridDiagnostic(new StagnationGridDiagnostic());
            base.OnEnter();
        }

        public override void Draw()
        {
            w0.GetWorldService<GridVisualizer>().Continous = false;
            LayoutMain();
            Title("Magnitude (Total Convection Flux)");
            MainPanel(this);
            base.Draw();
        }
    }

    public class StochasticSlide : Slide
    {
        public string VectorField;
        public bool paused;

        public override void OnLeave()
        {
            w0.GetWorldService<StochasticVisualization>().Disable();
            base.OnLeave();
        }
        public override void OnEnter()
        {
            w0.DataService.currentSelectedVectorField = VectorField;
            w0.GetWorldService<GridVisualizer>().Disable();
            w0.GetWorldService<FlowDirectionVisualization>().Disable();
            var stochasticPoincare = w0.GetWorldService<StochasticVisualization>();
            stochasticPoincare.Enable();
            stochasticPoincare.RespawnChance = 0;
            stochasticPoincare.Count = 5000;
            stochasticPoincare.dt = .01;
            if (paused)
                stochasticPoincare.dt = 0;
                if (VectorField.StartsWith("Diffusion"))
                stochasticPoincare.dt *= 2;
            stochasticPoincare.alpha = 1;
            stochasticPoincare.additiveBlending = false;
            //stochasticPoincare.fadeIn = false;
            stochasticPoincare.RenderRadius = .004f;
            stochasticPoincare.ColorByGradient = false;
            stochasticPoincare.Initialize();
            base.OnEnter();
        }

        public static bool respawnChance = false;
        public static bool showArrows = true;
        public static bool fadeIn = false;
        
        public StochasticSlide(string vectorField)
        {
            VectorField = vectorField;
        }
        public override void Draw()
        {
            LayoutMain();
            if (showArrows != w0.GetWorldService<FlowArrowVisualizer>().IsEnabled)
            {
                if (showArrows)
                    w0.GetWorldService<FlowArrowVisualizer>().Enable();
                else
                    w0.GetWorldService<FlowArrowVisualizer>().Disable();
            }
            var stochasticPoincare = w0.GetWorldService<StochasticVisualization>();
            Title("Stochastic " + (stochasticPoincare.reverse ? "Repelling Regions" : "Attracting Regions") + " (" + w0.DataService.currentSelectedVectorField + ")");
            stochasticPoincare.RespawnChance = respawnChance ? .0015f : 0f;
            stochasticPoincare.fadeIn = fadeIn;
            stochasticPoincare.additiveBlending = fadeIn;
            stochasticPoincare.alpha = fadeIn ? .3f : 1f;
            float y = 40;
            float y1 = 130;
            Presi.Checkbox("Respawn chance", ref respawnChance, new Vec2(600, y));
            Presi.Checkbox("Show Arrows", ref showArrows, new Vec2(600, y1));
            Presi.Checkbox("Reverse", ref stochasticPoincare.reverse, new Vec2(1100, y1));
            Presi.Checkbox("Fade In & additive blending", ref fadeIn, new Vec2(1100, y));
            MainPanel(this);
            base.Draw();
        }
    }

    public class SteadySlide : Slide
    {
        public override void OnEnter()
        {
            w0.GetWorldService<GridVisualizer>().Enable();
            w0.GetWorldService<FlowArrowVisualizer>().Disable();
            w0.GetWorldService<GridVisualizer>().SetGridDiagnostic(new ScalerGridDiagnostic());
            w0.GetWorldService<GridVisualizer>().MarkDirty = true;
            w0.GetWorldService<StochasticVisualization>().Disable();
            w0.DataService.SetDataset(SteadyDatasetName);

            w0.DataService.SimulationTime = 0;
            base.OnEnter();
        }
        public override void Draw()
        {
            w0.DataService.TimeMultiplier = .3f;
            w0.GetWorldService<GridVisualizer>().Continous = true;
            LayoutMain();
            Title("Steady Flow (epsilon=0)");
            MainPanel(this);
            Presi.Slider("time", ref w0.DataService.SimulationTime, 0, w0.DataService.VectorField.Domain.RectBoundary.Max.Z, new Vec2(Presi.CanvasCenter.X, 100), 900);

            base.Draw();
        }
    }

    public class UnsteadySlide : Slide
    {
        public override void OnEnter()
        {
            w0.DataService.SimulationTime = 0;
            w0.DataService.TimeMultiplier = .5f;
            w0.GetWorldService<GridVisualizer>().Enable();
            w0.GetWorldService<StochasticVisualization>().Disable();
            w0.GetWorldService<GridVisualizer>().MarkDirty = true;
            w0.DataService.SetDataset(UnsteadyDatasetName);
            base.OnEnter();
        }
        public override void Draw()
        {
            w0.GetWorldService<GridVisualizer>().Continous = true;
            LayoutMain();
            Title("Unsteady Flow (epsilon=0.1)");
            Presi.Slider("time", ref w0.DataService.SimulationTime, 0, w0.DataService.VectorField.Domain.RectBoundary.Max.Z, new Vec2(Presi.CanvasCenter.X, 100), 900);
            MainPanel(this);
            base.Draw();
        }
    }

    public class UnsteadyPeriodicSlide : Slide
    {
        public override void OnEnter()
        {
            w0.DataService.SimulationTime = 0;
            w0.DataService.TimeMultiplier = .5f;
            w0.DataService.currentSelectedVectorField = "Total Flux";
            w0.GetWorldService<GridVisualizer>().Enable();
            w0.GetWorldService<StochasticVisualization>().Disable();
            w0.GetWorldService<GridVisualizer>().MarkDirty = true;
            w0.DataService.SetDataset(UnsteadyDatasetNameP);
            base.OnEnter();
        }
        public override void Draw()
        {
            w0.GetWorldService<GridVisualizer>().Continous = true;
            LayoutMain();
            Title("Unsteady Periodic Flow t = [3..4]");
            var time2 = w0.DataService.SimulationTime % 1f + 3;
            Presi.Slider("time", ref time2, 0, 5, new Vec2(Presi.CanvasCenter.X, 100), 900);
            MainPanel(this);
            base.Draw();
        }
    }

    public class PeriodicArrowSlide : Slide
    {
        public override void OnEnter()
        {
            w0.DataService.TimeMultiplier = .3f;
            w0.GetWorldService<GridVisualizer>().Disable();
            w0.GetWorldService<FlowArrowVisualizer>().Enable();
            w0.GetWorldService<StochasticVisualization>().Disable();
            base.OnEnter();
        }

        public override void Draw()
        {
            LayoutMain();
            Title("Periodic Total Convection Flux");
            MainPanel(this);
            base.Draw();
        }
    }

    public class PeriodicStochasticSlide : Slide
    {
        public string vectorFieldName;

        public PeriodicStochasticSlide(string vectorFieldName)
        {
            this.vectorFieldName = vectorFieldName;
        }

        public override void OnEnter()
        {
            w0.GetWorldService<FlowArrowVisualizer>().Disable();
            w0.DataService.currentSelectedVectorField = vectorFieldName;
            var stochasticPoincare = w0.GetWorldService<StochasticVisualization>();
            stochasticPoincare.Enable();
            stochasticPoincare.reverse = false;
            w0.DataService.SimulationTime = 0;
            stochasticPoincare.Count = 20000;
            stochasticPoincare.RespawnChance = .01f;
            stochasticPoincare.dt = .1;
            stochasticPoincare.alpha = .5f;
            stochasticPoincare.fadeIn =true;
            stochasticPoincare.FixedT = true;
            stochasticPoincare.Initialize();
            base.OnEnter();
        }
        public bool animate = false;
        public bool showArrows;
        public override void Draw()
        {
            var stochasticPoincare = w0.GetWorldService<StochasticVisualization>();
            LayoutMain();
            w0.DataService.TimeMultiplier = animate ? .2f : 0f;
            
            if (showArrows != w0.GetWorldService<FlowArrowVisualizer>().IsEnabled)
            {
                if (showArrows)
                    w0.GetWorldService<FlowArrowVisualizer>().Enable();
                else
                    w0.GetWorldService<FlowArrowVisualizer>().Disable();
            }
            
            w0.DataService.SimulationTime = w0.DataService.SimulationTime % 1;
            Title($"Stochastic {(stochasticPoincare.reverse ? "Repellers" : "Attractors")} ({vectorFieldName})");
            if (!animate)
                Presi.Slider("time", ref w0.DataService.SimulationTime, 0, 1, new Vec2(Presi.CanvasCenter.X + 200, 100), 500);
            MainPanel(this);
            Presi.Checkbox("Reverse", ref stochasticPoincare.reverse, new Vec2(200, 100));
            Presi.Checkbox("Show Arrows", ref showArrows, new Vec2(500, 100));
            Presi.Checkbox("animate", ref animate, new Vec2(1600, 100));
            base.Draw();
        }
    }

    public class TimeSlide : Slide
    {
        public override void OnLeave()
        {
            var stochasticPoincare = w0.GetWorldService<StochasticVisualization>();
            stochasticPoincare.FixedT = true;
            stochasticPoincare.ColorByGradient = false;
            base.OnLeave();
        }

        public override void OnEnter()
        {
            var stochasticPoincare = w0.GetWorldService<StochasticVisualization>();
            stochasticPoincare.FixedT = false;
            stochasticPoincare.ColorByGradient = false;
            stochasticPoincare.RenderRadius = .002f;
            stochasticPoincare.dt = .02f;
            stochasticPoincare.RespawnChance = .002f;
            stochasticPoincare.alpha = 1;
            base.OnEnter();
        }
        public override void Draw()
        {
            var stochasticPoincare = w0.GetWorldService<StochasticVisualization>();
            stochasticPoincare.alpha = 1f;
            stochasticPoincare.fadeIn = true;
            LayoutMain();
            MainPanel(this);
            Title("Particles with varying time");
            Presi.Checkbox("Reverse", ref stochasticPoincare.reverse, new Vec2(200, 100));
            Presi.Checkbox("Use colormap", ref stochasticPoincare.ColorByGradient, new Vec2(1500, 100));
            base.Draw();
        }
    }

    public class StagnationPointSlide : Slide
    {
        public override void OnEnter()
        {
            var stochasticPoincare = w0.GetWorldService<StochasticVisualization>();
            stochasticPoincare.RenderRadius = .002f;
            stochasticPoincare.RespawnChance = 0f;
            stochasticPoincare.alpha = 0.1f;
            stochasticPoincare.ColorByGradient = true;
            stochasticPoincare.FixedT = false;
            stochasticPoincare.dt = .1f;
            base.OnEnter();
        }

        public override void Draw()
        {
            var stochasticPoincare = w0.GetWorldService<StochasticVisualization>();
            stochasticPoincare.fadeIn = true;
            LayoutMain();
            MainPanel(this);
            Title("Stagnation Path (Diffusion Flux)");
            Presi.Checkbox("Reverse", ref stochasticPoincare.reverse, new Vec2(200, 100));
            Presi.Checkbox("Use colormap", ref stochasticPoincare.ColorByGradient, new Vec2(1600, 100));
            base.Draw();
            
        }
    }

    public override Slide[] GetSlides()
    {
        return
        [
            new DiffusionVectorfield(),
            new TotalFluxVectorfield(),
            new DivergenceSlide(),
            new MagnitudeSlide(),
            new StochasticSlide("Total Flux") {paused = true},
            new StochasticSlide("Total Flux"),
            new StochasticSlide("Diffusion Flux"),
            new SteadySlide(),
            new UnsteadySlide(),
            new UnsteadyPeriodicSlide(),
            new PeriodicArrowSlide(),
            new PeriodicStochasticSlide("Total Flux"),
            new PeriodicStochasticSlide("Diffusion Flux"),
            new TimeSlide(),
            new StagnationPointSlide(),
        ];
    }

    public override void Setup(FlowExplainer flowExplainer)
    {
        var presentationService = flowExplainer.GetGlobalService<PresentationService>();
        var manager = flowExplainer.GetGlobalService<WorldManagerService>();
        var w0 = manager.NewWorld();
        presentationService.Presi.GetView("v0").World = w0;
        w0.GetWorldService<DataService>().SetDataset(SteadyDatasetNameP);
    }
}