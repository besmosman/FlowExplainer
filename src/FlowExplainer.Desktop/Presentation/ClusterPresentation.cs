namespace FlowExplainer;

public class ClusterPresentation : NewPresentation
{    
    string AssetsPath => "Assets/Images/Presentation/density-paper-cluster";
    
    public override void Draw()
    {
        {
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

                var worldPanel = Presi.GetWidgetData("#w", 0);
                var grad = view.World.GetWorldService<DataService>().ColorGradient;
                var lb = WorldToScreenRel(worldPanel, new Vec2(0, 0.0));
                var rb = WorldToScreenRel(worldPanel, new Vec2(1, 0.0));
                var lt = WorldToScreenRel(worldPanel, new Vec2(0, 0.5));
                var rt = WorldToScreenRel(worldPanel, new Vec2(1, 0.5));

                void DrawWalls()
                {
                    Gizmos2D.Line(Presi.View.Camera2D, lb, rb, grad.Get(1), 15f);
                    Gizmos2D.Line(Presi.View.Camera2D, lt, rt, grad.Get(0.0f), 15f);
                    Gizmos2D.AdvText(Presi.View.Camera2D, rb + new Vec2(40, -20), 50f, grad.Get(1), "Hot");
                    Gizmos2D.AdvText(Presi.View.Camera2D, rt + new Vec2(40, -20), 50f, grad.Get(0.0f), "Cold");
                }

                if (BeginStep())
                {
                    Title("Hot/Cold Wall");
                    //var rect= new Rect<Vec2>(worldPanel.RenderMin, worldPanel.RenderMax);
                    //Gizmos2D.Circle(Presi.View.Camera2D, WorldToScreenRel(worldPanel, new Vec2(0.5,0.25)), Color.White,10f);
                    //  if(isFirst)
                    //  Gizmos2D.Rect(Presi.View.Camera2D, lb, rt, grad.Get(0.2f));
                    DrawWalls();
                }

                if (BeginStep())
                {
                    DrawWalls();
                    Gizmos2D.Rect(Presi.View.Camera2D, lb, rt, grad.Get(.3f));
                    Gizmos2D.AdvText(Presi.View.Camera2D, WorldToScreenRel(worldPanel, new Vec2(0.5, .25)) + new Vec2(0, -20), 40f, Color.White, "Constant Initial Temperature", centered: true);


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
                    Presi.Text("u =  sin(πL)cos(πy), v = -cos(πL)sin(πy)\r\n L = x - sin(2πt)", new Vec2(0.5, 0.05), .024f, true, Color.White);
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
            }
            if (BeginSlide())
            {
                if (IsFirstStep())
                {
                    Title("Capturing effect of fluid flow");
                    //Presi.Image(Presi.GetPresiImage());
                    var size = new Vec2(1, .5) /1.95;
                    var view = DrawWorldPanel(new Vec2(0.75,.2), size, zoom: .78,
                        load: (world) =>
                        {
                            var data = world.GetWorldService<DataService>();
                            var axis = world.AddVisualisationService<AxisVisualizer>();
                            axis.DrawTitle = true;
                            axis.DrawGradient = false;
                            axis.Title = "Temperature with Flow";
                            data.SetDataset("Double Gyre EPS=0.1, Pe=100");
                            data.TimeMultiplier = .2f;
                            data.currentSelectedScaler = "Total Temperature";
                            world.AddVisualisationService<GridVisualizer>().SetGridDiagnostic(new ScalerGridDiagnostic());
                        }, "#l");
                    
                    var viewR = DrawWorldPanel(new Vec2(0.25, .2), size, zoom: .78,
                        load: (world) =>
                        {
                            var data = world.GetWorldService<DataService>();
                            var axis = world.AddVisualisationService<AxisVisualizer>();
                            axis.DrawTitle = true;
                            axis.DrawGradient = false;
                            axis.Title = "Temperature without Flow";
                            data.SetDataset("Double Gyre EPS=0.1, Pe=100");
                            data.TimeMultiplier = .2f;
                            data.currentSelectedScaler = "No Flow Temperature";
                            world.AddVisualisationService<GridVisualizer>().SetGridDiagnostic(new ScalerGridDiagnostic());
                        }, "#r");
                }
            }
        }

        if (BeginSlide())
        {
            //Presi.RectCentered(Vec2.One/2, Vec2.One, Color.Green);
            var relCenterPos = new Vec2(0.5, 0.6);
            var relSize = new Vec2(1, 0.5);
            var relSizeM = 0.8;
            if (IsFirstStep())
            {
                relSizeM = 1;
                relCenterPos.Y = 0.5;
            }

            var temp = DrawWorldPanel(relCenterPos, relSize * relSizeM, zoom: .76,
                load: (world) =>
                {
                    var data = world.GetWorldService<DataService>();
                    data.currentSelectedVectorField = "Total Flux";
                    data.SetDataset("Double Gyre EPS=0.1, Pe=100");
                    data.currentSelectedScaler = "Convective Temperature";

                    data.SimulationTime = 1.5f;
                    data.TimeMultiplier = .0f;
                    var grid = world.AddVisualisationService<ArrowVisualizer>();
                    var axis = world.AddVisualisationService<AxisVisualizer>();
                    axis.DrawTitle = false;

                }, "#world", 0);

            if (IsFirstStep())
                Title("Heat Flux ≠ Thermal velocity");

            if (BeginStep())
            {
                Title("Thermal velocity");
                if (StepEnter())
                {
                    var grid = temp.World.AddVisualisationService<GridVisualizer>(0);
                    grid.SetGridDiagnostic(new ScalerGridDiagnostic());
                    grid.AltGradient = Gradients.BlueGrayRed;
                    grid.WaitForComputation();
                }
                var x = 0.2;
                var lh = 0.02f;

                var screenRelToWorld = ScreenRelToWorld(Presi.GetWidgetData("#world", 0), Presi.View.MousePosition);
                var p = new Vec3(screenRelToWorld, temp.World.DataService.SimulationTime);
                double temper = temp.World.DataService.ScalerField.Evaluate(p);
                var flux = temp.World.DataService.VectorField.Evaluate(p);
                Gizmos2D.Circle(Presi.View.Camera2D, Presi.View.MousePosition,
                    Color.White, 10f);


                var spacing = lh * 3;
                Presi.Text("'thermal velocity' = ", new Vec2(.2, 0.1 + lh), lh, true, Color.White);
                Presi.Text("= ", new Vec2(.54, 0.1 + lh), lh, true, Color.White);
                Presi.RectCentered(new Vec2(.45, .15), new Vec2(.1, 0.005f), Color.White);
                Presi.Text("Flux", new Vec2(.45, 0.17), lh, true, Color.White);
                Presi.Text($"({double.Round(flux.X, 3)}, {double.Round(flux.X, 3)})", new Vec2(.7, 0.17), lh, true, Color.White);
                Presi.Text(double.Round(temper, 5).ToString(), new Vec2(.7, 0.08), lh, true, Color.White);
                Presi.RectCentered(new Vec2(.7, .15), new Vec2(.25, 0.005f), Color.White);
                Presi.Text("T'", new Vec2(.45, 0.08), lh, true, Color.White);
                var vel = flux / temper;
                Presi.Text($"= ({double.Round(vel.X, 3)}, {double.Round(vel.X, 3)})", new Vec2(.84, 0.1 + lh), lh, false, Color.White);


                Logger.LogDebug(screenRelToWorld);
                /*var vel = DrawWorldPanel(tempRelPos, tempRelSize, zoom: .76,
                    load: (world) =>
                    {
                        var data = world.GetWorldService<DataService>();
                        data.SetDataset("Double Gyre EPS=0.1, Pe=100");
                        data.TimeMultiplier = .5f;
                        data.currentSelectedVectorField = "Total Flux";
                        data.currentSelectedScaler = "Convective Temperature";
                        var a = world.AddVisualisationService<ArrowVisualizer>();
                        var grid = world.AddVisualisationService<GridVisualizer>();
                        a.AltGradient = Gradients.Parula;
                        grid.SetGridDiagnostic(new ScalerGridDiagnostic());
                        grid.WaitForComputation();
                        var axis = world.AddVisualisationService<AxisVisualizer>();
                        data.ColorGradient = Gradients.GetGradient("BlueGrayRed");
                        axis.DrawTitle = false;
                    });*/
                //  vel.World.GetWorldService<DataService>().SimulationTime = temp.World.DataService.SimulationTime;


            }
        }


        bool second = true;
        if (BeginSlide())
        {
            StructureSlide(0);
        }

        if (BeginSlide())
        {
            StructureSlide(1);
        }


        if (BeginSlide())
        {
            StructureSlide(2);
        }
        //Presi.Text(text, new Vec2(0,.9), .016,false, Color.Red);
    }

    private void StructureSlide(int type)
    {

        int curStep = 0;

        void RegisterStep(string name, ref bool v)
        {
            var lh = .1f;
            Presi.Text(name, new Vec2(0, curStep * lh * 1.4), lh, false, Color.White);
        }

        var tempRelSize = new Vec2(1, .5) / 1;
        var tempRelPos = new Vec2(.5, .6);
        var temp = DrawWorldPanel(tempRelPos, tempRelSize, zoom: .76,
            load: (world) =>
            {
                var data = world.GetWorldService<DataService>();
                data.SetDataset("Double Gyre EPS=0.1, Pe=100");
                data.TimeMultiplier = .5f;
                data.currentSelectedVectorField = "Total Flux";
                var axis = world.AddVisualisationService<AxisVisualizer>();
                var structure = world.AddVisualisationService<StructureAccentuatingService>();

                axis.DrawTitle = false;
            });

        if (SlideEnter())
        {
            var structure = temp.World.GetWorldService<StructureAccentuatingService>();
            structure.Initialize();
            if (type == 0)
                foreach (ref var p in structure.Particles.AsSpan())
                    p.Direction = 1;

            if (type == 1)
                foreach (ref var p in structure.Particles.AsSpan())
                    p.Direction = -1;

            if (type == 2)
            {
                foreach (ref var p in structure.Particles.AsSpan().Slice(structure.Particles.Length / 2, structure.Particles.Length / 2))
                    p.Direction = 1;
                foreach (ref var p in structure.Particles.AsSpan().Slice(0, structure.Particles.Length / 2))
                    p.Direction = -1;
            }
        }
        var structureAccentuatingService = temp.World.GetWorldService<StructureAccentuatingService>();
        Presi.Checkbox("Integration", ref structureAccentuatingService.Integration, new Vec2(0.1, 0.1));
        Presi.Checkbox("Periodic Reseed", ref structureAccentuatingService.Reseed, new Vec2(0.1, 0.02));
        Presi.Checkbox("Transparency", ref structureAccentuatingService.Transparency, new Vec2(0.36, 0.1));
        Presi.Checkbox("Accumelation Texture", ref structureAccentuatingService.DrawTexture, new Vec2(0.36, 0.02));
    }
}