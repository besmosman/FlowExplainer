using System.Globalization;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class IntroductionPresentation : Presentation
{

    public override Slide[] GetSlides()
    {
        return
        [
            
            new HeatSubtractSlide(),
            new VectorFieldsSlide(),
        ];
    }
    
    public override void Setup(FlowExplainer flowExplainer)
    {
        var manager = flowExplainer.GetGlobalService<WorldManagerService>();

        var w0 = manager.NewWorld();
        var w1 = manager.NewWorld();
        var w2 = manager.NewWorld();
        var w3 = manager.NewWorld();

        Scripting.SetGyreDataset(w0);
        Scripting.SetGyreDataset(w1);
        Scripting.SetGyreDataset(w2);
        Scripting.SetGyreDataset(w3);


        float[] ts = [0.01f, 0.3f];

        int timeSteps = 100;
        foreach (float t in ts)
        {
            var title = t.ToString(CultureInfo.InvariantCulture);
            w0.GetWorldService<DataService>().ScalerFields.Add($"Diffusion Sources (T={title})", RegularGridVectorField<Vec3, Vec3i, float>.Load($"diffusion-sources-T={title}.field"));
            w0.GetWorldService<DataService>().ScalerFields.Add($"Diffusion Sinks (T={title})", RegularGridVectorField<Vec3, Vec3i, float>.Load($"diffusion-sinks-T={title}.field"));
            w0.GetWorldService<DataService>().ScalerFields.Add($"Convection Sources (T={title})", RegularGridVectorField<Vec3, Vec3i, float>.Load($"convection-sources-T={title}.field"));
            w0.GetWorldService<DataService>().ScalerFields.Add($"Convection Sinks (T={title})", RegularGridVectorField<Vec3, Vec3i, float>.Load($"convection-sinks-T={title}.field"));
        }

        w0.GetWorldService<DataService>().currentSelectedVectorField = "Diffusion Flux";
        w0.GetWorldService<DataService>().currentSelectedVectorField = "Velocity";
        w1.GetWorldService<DataService>().currentSelectedScaler = "Total Temperature";


        var presentationService = flowExplainer.GetGlobalService<PresentationService>();
        presentationService.Presi.GetView("v0").World = w0;
        presentationService.Presi.GetView("v1").World = w1;
        presentationService.Presi.GetView("v2").World = w2;
        presentationService.Presi.GetView("v3").World = w3;
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
                dat.TimeMultiplier = 0f;
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

    public class HeatSubtractSlide : Slide
    {
        public override void OnEnter()
        {
            Presi.GetView("v0").World.GetWorldService<AxisVisualizer>().DrawWalls = false;
            World[] ws = [w0, w1, w2];
            foreach (var w in ws)
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

            w0.GetWorldService<FlowFieldVisualizer>().Disable();
            w1.GetWorldService<DataService>().currentSelectedScaler = "Total Temperature";
            w0.GetWorldService<DataService>().currentSelectedScaler = "Convective Temperature";
            w2.GetWorldService<DataService>().currentSelectedScaler = "No Flow Temperature";
            base.Load();
        }

        public override void Draw()
        {
            LayoutMain();
            Title("Temperature Fields");
            Presi.ViewPanel("v1", new Vec2(Presi.CanvasCenter.X - 481, 780), new Vec2(1, .5f) * Presi.CanvasSize.X * .5f, .8f);
            Presi.ViewPanel("v2", new Vec2(Presi.CanvasCenter.X + 481, 780), new Vec2(1, .5f) * Presi.CanvasSize.X * .5f, .8f);
            Presi.ViewPanel("v0", new Vec2(Presi.CanvasCenter.X, 260), new Vec2(1, .5f) * Presi.CanvasSize.X * .5f, .8f);
            Presi.Text("Temperature with Flow", new Vec2(Presi.CanvasCenter.X - 470, 1020), 64, true, Color.White);
            Presi.Text("Temperature without Flow", new Vec2(Presi.CanvasCenter.X + 470, 1020), 64, true, Color.White);
            Presi.Text("Convective Temperature", new Vec2(Presi.CanvasCenter.X, 490), 64, true, Color.White);
            Presi.Text("-", new Vec2(Presi.CanvasCenter.X - 30, 710), 204, true, Color.White);
            Presi.Text("=", new Vec2(Presi.CanvasCenter.X - 530, 250), 204, true, Color.White);
            //Presi.Text("u(x,y,t) = 0", new Vec2(Presi.CanvasCenter.X, 220), 80, true, Color.White);
            base.Draw();
        }
    }

}

public class HeatStructuresPresentation : Presentation
{
    public override Slide[] GetSlides()
    {
        return
        [
            new TitleSlide(),
            new HeatSubtractSlide(),
            new VectorFieldsSlide(),
            new HeatStructuresIntroSlide(),
            new HeatStructureSlide
            {
                Title = "Diffusion Sinks (T=0.01)",
                ScalerField = "Diffusion Sinks (T=0.01)",
                VectorField = "Diffusion Flux",
            },
            new HeatStructureSlide
            {
                Title = "Diffusion Sources (T=0.01)",
                ScalerField = "Diffusion Sources (T=0.01)",
                VectorField = "Diffusion Flux",
            },
            new HeatStructureSlide
            {
                Title = "Convection Sinks (T=0.01)",
                ScalerField = "Convection Sinks (T=0.01)",
                VectorField = "Convection Flux",
            },
            new HeatStructureSlide
            {
                Title = "Convection Sources (T=0.01)",
                ScalerField = "Convection Sources (T=0.01)",
                VectorField = "Convection Flux",
            },
            new HeatStructureSlide
            {
                Title = "Convection Sources (T=0.3)",
                ScalerField = "Convection Sources (T=0.3)",
                VectorField = "Convection Flux",
            },
            new HeatStructureSlide
            {
                Title = "Convection Sinks (T=0.3)",
                ScalerField = "Convection Sinks (T=0.3)",
                VectorField = "Convection Flux",
            },
            new LCSSlide(),
            new CriticalSlide(),
            new FlowExample(),
            new GradientSlide(),
            new GradientLCSSlide(),
            new FlattenSlide(),
        ];
    }

    public class FlattenSlide : Slide
    {
        public Texture Image;
        public override void Load()
        {
            Image = new ImageTexture("Assets/Images/presi/flatten.png")
            {
                TextureMagFilter = TextureMagFilter.Linear,
                TextureMinFilter = TextureMinFilter.Linear,
            };
            base.Load();
        }
        public override void Draw()
        {   
            LayoutMain();

            Title("??? visualization");
            Presi.Image(Image, Presi.CanvasCenter - new Vec2(0, 60), 1700);
            base.Draw();
        }
    }

    public class GradientLCSSlide : Slide
    {
        public override void OnEnter()
        {
            w0.GetWorldService<GridVisualizer>().SetGridDiagnostic(new FunctionGridDiagnostic()
            {
                UseGradient = true,
                StandardLCS = true,
                T = 3f,
            });
            base.OnEnter();
        }

        public override void Draw()
        {
            LayoutMain();
            Title("LCS: Arbitrary Function");
            Presi.ViewPanel("v0", new Vec2(Presi.CanvasCenter.X, Presi.CanvasCenter.Y), new Vec2(1, .5f) * Presi.CanvasSize.X * .9f, .8f);
            Presi.Text("average F(x,y) along trajectory", new Vec2(Presi.CanvasCenter.X, 100), 100, true, Color.White);
            base.Draw();
            base.Draw();
        }
    }

    public class GradientSlide : Slide
    {
        public override void OnEnter()
        {
            w0.GetWorldService<GridVisualizer>().SetGridDiagnostic(new FunctionGridDiagnostic()
            {
                UseGradient = true,
                StandardLCS = false,
                T = 0.01f,
                K = 1,
            });
            base.OnEnter();
        }

        public override void Draw()
        {
            LayoutMain();
            Title("Arbitrary Function");
            Presi.ViewPanel("v0", new Vec2(Presi.CanvasCenter.X, Presi.CanvasCenter.Y), new Vec2(1, .5f) * Presi.CanvasSize.X * .9f, .8f);
            Presi.Text("F(x,y) = sin(8(x+y))", new Vec2(Presi.CanvasCenter.X, 100), 100, true, Color.White);
            base.Draw();
            base.Draw();
        }
    }


    public class FlowExample : Slide
    {
        public override void OnEnter()
        {
            w0.GetWorldService<GridVisualizer>().TargetCellCount = 100000;
            w0.GetWorldService<GridVisualizer>().SetGridDiagnostic(new FunctionGridDiagnostic()
            {
                UseGradient = false,
                StandardLCS = true,
                T = 3,
            });
            w0.GetWorldService<GridVisualizer>().MarkDirty = true;
            w0.GetWorldService<GridVisualizer>().Continous = false;
            w0.GetWorldService<GridVisualizer>().Enable();
            w0.GetWorldService<DataService>().SimulationTime = 0;
            w0.GetWorldService<DataService>().currentSelectedVectorField = "Velocity";
            base.OnEnter();
        }

        public override void Draw()
        {
            LayoutMain();
            Title("LCS: Trajectory Length (T=3)");
            Presi.ViewPanel("v0", new Vec2(Presi.CanvasCenter.X, Presi.CanvasCenter.Y), new Vec2(1, .5f) * Presi.CanvasSize.X * .9f, .8f);
            ref float simulationTime = ref w0.GetWorldService<DataService>().SimulationTime;
            // Presi.Slider($"time = {simulationTime:N2}", ref simulationTime, 0, 1, new Vec2(Presi.CanvasCenter.X, 100f), 500);
            base.Draw();
        }
    }

    public class LCSSlide : Slide
    {
        public Texture Image;
        public override void Load()
        {
            Image = new ImageTexture("Assets/Images/presi/lcs.png")
            {
                TextureMagFilter = TextureMagFilter.Linear,
                TextureMinFilter = TextureMinFilter.Linear,
            };
            base.Load();
        }
        public override void Draw()
        {
            LayoutMain();

            Title("LCS");
            Presi.Image(Image, Presi.CanvasCenter - new Vec2(0, 50), 1500);
            base.Draw();
        }
    }

    public class CriticalSlide : Slide
    {
        public Texture Image;
        public override void Load()
        {
            Image = new ImageTexture("Assets/Images/presi/critical.png")
            {
                TextureMagFilter = TextureMagFilter.Linear,
                TextureMinFilter = TextureMinFilter.Linear,
            };
            base.Load();
        }
        public override void Draw()
        {
            LayoutMain();
            Title("Bias");
            Presi.Image(Image, Presi.CanvasCenter - new Vec2(0, 50), 1150);
            base.Draw();
        }
    }
    
    public class HeatStructureSlide : Slide
    {
        public string ScalerField;
        public string VectorField;
        public string Title;

        public override void OnEnter()
        {
            w0.GetWorldService<DataService>().currentSelectedScaler = ScalerField;
            w0.GetWorldService<DataService>().currentSelectedVectorField = VectorField;
            w0.GetWorldService<GridVisualizer>().SetGridDiagnostic(new ScalerGridDiagnostic());
            w0.GetWorldService<GridVisualizer>().Enable();
            w0.GetWorldService<GridVisualizer>().Continous = true;
            w0.GetWorldService<GridVisualizer>().TargetCellCount =100000;
            w0.GetWorldService<DataService>().TimeMultiplier = 0f;
            var flow = w0.GetWorldService<FlowDirectionVisualization>();
            flow.amount = 3000;
            if (!flow.IsEnabled)
            {
                flow.opacity = .3f;
                flow.speed = 2;
                flow.Initialize();
            }
            base.OnEnter();
        }
        public override void Draw()
        {
            LayoutMain();
            Title(Title);
            Presi.ViewPanel("v0", new Vec2(Presi.CanvasCenter.X, Presi.CanvasCenter.Y), new Vec2(1, .5f) * Presi.CanvasSize.X * .9f, .8f);
            ref float simulationTime = ref w0.GetWorldService<DataService>().SimulationTime;
            Presi.Slider($"time = {simulationTime:N2}", ref simulationTime, 0, 1, new Vec2(Presi.CanvasCenter.X, 100f), 500);
            var flow = w0.GetWorldService<FlowDirectionVisualization>();
            bool isEnabled = flow.IsEnabled;
            Presi.Checkbox("Show Flow", ref isEnabled, new Vec2(300, 100));

            if (flow.IsEnabled != isEnabled)
            {
                if (flow.IsEnabled)
                    flow.Disable();
                else
                    flow.Enable();
            }
            base.Draw();
        }
    }

    public class HeatStructuresIntroSlide : Slide
    {
        public Texture Image;

        public override void Load()
        {
            Image = new ImageTexture("Assets/Images/presi/trajectory.png")
            {
                TextureMagFilter = TextureMagFilter.Linear,
                TextureMinFilter = TextureMinFilter.Linear,
            };
            base.Load();
        }
        public override void Draw()
        {
            LayoutMain();
            Title("Heat Sources and Sinks");
            MainParagraph(
                @"
Sinks:
- Flux trajectory for a given time range
- Heat map of the trajectory

Sources:
- Along the negative flux direction
"
            );
            Presi.Image(Image, Presi.CanvasCenter - new Vec2(-500, 00), 600);

            base.Draw();
        }
    }

    private class TitleSlide : Slide
    {
        public override void Draw()
        {
            LayoutTitle();
            TitleTitle("Progress", "26-09-2025");
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
                dat.TimeMultiplier = 0f;
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
            base.Draw();
        }
    }

    public class HeatSubtractSlide : Slide
    {
        public override void OnEnter()
        {
            Presi.GetView("v0").World.GetWorldService<AxisVisualizer>().DrawWalls = false;
            World[] ws = [w0, w1, w2];
            foreach (var w in ws)
            {
                var dat = w.GetWorldService<DataService>();
                dat.TimeMultiplier = .1f;
                dat.SimulationTime = .0000001f;
                var gridVisualizer = w.GetWorldService<GridVisualizer>();
                gridVisualizer.Enable();
                gridVisualizer.Continous = true;
                gridVisualizer.SetGridDiagnostic(new ScalerGridDiagnostic());
                dat.currentSelectedVectorField = "Velocity";
            }

            w0.GetWorldService<FlowFieldVisualizer>().Disable();
            w1.GetWorldService<DataService>().currentSelectedScaler = "Total Temperature";
            w0.GetWorldService<DataService>().currentSelectedScaler = "Convective Temperature";
            w2.GetWorldService<DataService>().currentSelectedScaler = "No Flow Temperature";
            base.Load();
        }

        public override void Draw()
        {
            LayoutMain();
            Title("Temperature Fields");
            Presi.ViewPanel("v1", new Vec2(Presi.CanvasCenter.X - 481, 780), new Vec2(1, .5f) * Presi.CanvasSize.X * .5f, .8f);
            Presi.ViewPanel("v2", new Vec2(Presi.CanvasCenter.X + 481, 780), new Vec2(1, .5f) * Presi.CanvasSize.X * .5f, .8f);
            Presi.ViewPanel("v0", new Vec2(Presi.CanvasCenter.X, 260), new Vec2(1, .5f) * Presi.CanvasSize.X * .5f, .8f);
            Presi.Text("Temperature with Flow", new Vec2(Presi.CanvasCenter.X - 470, 1020), 64, true, Color.White);
            Presi.Text("Temperature without Flow", new Vec2(Presi.CanvasCenter.X + 470, 1020), 64, true, Color.White);
            Presi.Text("Convective Temperature", new Vec2(Presi.CanvasCenter.X, 490), 64, true, Color.White);
            Presi.Text("-", new Vec2(Presi.CanvasCenter.X - 30, 710), 204, true, Color.White);
            Presi.Text("=", new Vec2(Presi.CanvasCenter.X - 530, 250), 204, true, Color.White);
            //Presi.Text("u(x,y,t) = 0", new Vec2(Presi.CanvasCenter.X, 220), 80, true, Color.White);
            base.Draw();
        }
    }


    public override void Setup(FlowExplainer flowExplainer)
    {
        var manager = flowExplainer.GetGlobalService<WorldManagerService>();

        var w0 = manager.NewWorld();
        var w1 = manager.NewWorld();
        var w2 = manager.NewWorld();
        var w3 = manager.NewWorld();

        Scripting.SetGyreDataset(w0);
        Scripting.SetGyreDataset(w1);
        Scripting.SetGyreDataset(w2);
        Scripting.SetGyreDataset(w3);


        float[] ts = [0.01f, 0.3f];

        int timeSteps = 100;
        foreach (float t in ts)
        {
            var title = t.ToString(CultureInfo.InvariantCulture);
            w0.GetWorldService<DataService>().ScalerFields.Add($"Diffusion Sources (T={title})", RegularGridVectorField<Vec3, Vec3i, float>.Load($"diffusion-sources-T={title}.field"));
            w0.GetWorldService<DataService>().ScalerFields.Add($"Diffusion Sinks (T={title})", RegularGridVectorField<Vec3, Vec3i, float>.Load($"diffusion-sinks-T={title}.field"));
            w0.GetWorldService<DataService>().ScalerFields.Add($"Convection Sources (T={title})", RegularGridVectorField<Vec3, Vec3i, float>.Load($"convection-sources-T={title}.field"));
            w0.GetWorldService<DataService>().ScalerFields.Add($"Convection Sinks (T={title})", RegularGridVectorField<Vec3, Vec3i, float>.Load($"convection-sinks-T={title}.field"));
        }

        w0.GetWorldService<DataService>().currentSelectedVectorField = "Diffusion Flux";
        w0.GetWorldService<DataService>().currentSelectedVectorField = "Velocity";
        w1.GetWorldService<DataService>().currentSelectedScaler = "Total Temperature";


        var presentationService = flowExplainer.GetGlobalService<PresentationService>();
        presentationService.Presi.GetView("v0").World = w0;
        presentationService.Presi.GetView("v1").World = w1;
        presentationService.Presi.GetView("v2").World = w2;
        presentationService.Presi.GetView("v3").World = w3;
    }
}