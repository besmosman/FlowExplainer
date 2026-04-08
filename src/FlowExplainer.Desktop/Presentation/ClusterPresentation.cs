namespace FlowExplainer;

public class ClusterPresentation : NewPresentation
{
    const string AssetsPath = "Assets/Images/Presentations/density-paper-cluster";
    private Texture michel0 { get; } = new ImageTexture(Path.Combine(AssetsPath, "michel0.png"));
    private Texture michel1 { get; } = new ImageTexture(Path.Combine(AssetsPath, "michel1.png"));
    private Texture structure_accentuating { get; } = new ImageTexture(Path.Combine(AssetsPath, "structure-accentuating.png"));


    public double t;
    public override void Draw()
    {
        Introduction();
        StructureAcentuating();
        Structures();


        if (BeginSlide())
        {
            var view3d = DrawWorldPanel(new Vec2(.5, .5), new Vec2(1, .8), zoom: .76,
                load: (world) =>
                {
                    var data = world.GetWorldService<DataService>();
                    data.currentSelectedVectorField = "Total Flux";
                    data.SetDataset("Double Gyre EPS=0.1, Pe=100");
                    data.currentSelectedScaler = "Convective Temperature";
                    world.AddVisualisationService<Axis3D>();
                    //data.ColorGradient = Gradients.Grayscale;
                    var particles = world.AddVisualisationService<DensityParticlesData>();
                    var structure = world.AddVisualisationService<DensityPathStructuresSpaceTime>();
                    world.AddVisualisationService<Slice3DVisualizer>().Disable();
                    particles.SeedTimeRange = 1.4;
                    particles.Particles.ResizeIfNeeded(0);
                    structure.Tau = 0;
                    world.AddVisualisationService<DensityStructuresSpaceTime3DUI>().Disable();
                    world.AddVisualisationService<DensityParticles3DVisualizer>().Radius = .005;
                    data.SimulationTime = 2;
                }, "#world", 0);

            if (!view3d.Is3DCamera)
            {
                view3d.CameraZoom = 7;
                view3d.CameraOffset = new Vec3(-.5, -.25, view3d.World.DataService.SimulationTime);
                view3d.Is3DCamera = true;
                view3d.CameraPitch = 1.25;
                view3d.CameraYaw = -3.85;
            }

            if (BeginStep())
            {
                view3d.World.GetWorldService<DensityStructuresSpaceTime3DUI>().Enable();
            }
            if (BeginStep())
            {
                var densityParticlesData = view3d.World.GetWorldService<DensityParticlesData>();
                var structuresSpaceTime = view3d.World.GetWorldService<DensityPathStructuresSpaceTime>();
                densityParticlesData.Particles.ResizeIfNeeded(15000);
            }
            
            if (BeginStep())
            {
                var densityParticlesData = view3d.World.GetWorldService<DensityParticlesData>();
                var structuresSpaceTime = view3d.World.GetWorldService<DensityPathStructuresSpaceTime>();
                densityParticlesData.dt = -.02;
            }

            if (BeginStep())
            {
                if (StepEnter())
                {
                    view3d.World.GetWorldService<Slice3DVisualizer>().Enable();
                    var structuresSpaceTime = view3d.World.GetWorldService<DensityPathStructuresSpaceTime>();
                    structuresSpaceTime.Tau = .05;
                }
            }
            Title("Spacetime Visualization");
        }

    }
    private void Structures()
    {

        if (BeginSlide())
        {
            var m = IsFirstStep() ? 1 : .89;
            var p = IsFirstStep() ? new Vec2(0.5, .53) : new Vec2(0.5, .4);
            var view = DrawWorldPanel(p, new Vec2(1, .5) * m, zoom: .78, load: w =>
            {
                w.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
                w.DataService.currentSelectedVectorField = "Total Flux";
                var stoch = w.AddVisualisationService<StochasticVisualization>();
                var axis = w.AddVisualisationService<AxisVisualizer>();
                axis.DrawTitle = false;
                w.DataService.SimulationTime = 1.5f;
                stoch.dt = .1f;
                stoch.reverse = true;
                stoch.ReseedChance = .1f;
                //  w.AddVisualisationService<ArrowVisualizer>().colorByGradient = false;
            });
            if (IsFirstStep())
            {
                Presi.Slider("time", ref view.World.DataService.SimulationTime, 0, view.World.DataService.ScalerField.Domain.RectBoundary.Max.Z, new Vec2(0.5, .1), .6);
            }
            if (BeginStep())
            {
                Presi.Text("are @red[not physically meaningfull]", new Vec2(.5, .86), .03, true, Color.White);

            }
            Title("Instantanous Structures");
        }

        if (BeginSlide())
        {
            var relCenterPos = new Vec2(0.5, .53);
            var relSize = new Vec2(1, .5);


            Action<World>? worldLoad = w =>
            {
                w.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
                w.DataService.currentSelectedVectorField = "Total Flux";
                var stoch = w.AddVisualisationService<StochasticVisualization>();
                var axis = w.AddVisualisationService<AxisVisualizer>();
                axis.DrawTitle = false;
                w.DataService.SimulationTime = 0;
                w.DataService.TimeMultiplier = .5f;
                stoch.mode = StochasticVisualization.Mode.TimeIntegration;
                stoch.reverse = true;
                stoch.ReseedChance = .1f;
                //  w.AddVisualisationService<ArrowVisualizer>().colorByGradient = false;
            };
            View view = null;
            if (IsFirstStep())
            {
                view = DrawWorldPanel(relCenterPos, relSize, zoom: .78, load: worldLoad, "#T");
                Title("Time Evolving Structures");
                Presi.Slider("time", ref view.World.DataService.SimulationTime, 0, view.World.DataService.ScalerField.Domain.RectBoundary.Max.Z, new Vec2(0.5, .1), .6);
            }
            if (BeginStep())
            {
                var size = new Vec2(1, .5) / 1.9;
                var x1 = .24;
                var x2 = .79;
                var y1 = .18;
                var y2 = .75;
                DrawWorldPanel(new Vec2(x1, y2), size, zoom: .78, load: (w) =>
                {
                    w.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
                    w.DataService.currentSelectedVectorField = "Total Flux";
                    var stoch = w.AddVisualisationService<StochasticVisualization>();
                    var axis = w.AddVisualisationService<AxisVisualizer>();
                    axis.DrawTitle = false;
                    w.DataService.SimulationTime = 0;
                    stoch.mode = StochasticVisualization.Mode.Instantaneous;
                    stoch.reverse = true;
                    stoch.dt = .1;
                    w.DataService.SimulationTime = 3.4;
                    stoch.ReseedChance = .1f;
                });
                /*Presi.MainParagraph(
@"Issues:
- How to visualize changing directions?");*/

                view = DrawWorldPanel(new Vec2(x1, y1), size, zoom: .78, load: worldLoad, "#T");

                var temp = DrawWorldPanel(new Vec2(x2, y2), size, zoom: .78,
                    load: (world) =>
                    {
                        var data = world.GetWorldService<DataService>();
                        data.SetDataset("Double Gyre EPS=0.1, Pe=100");
                        data.TimeMultiplier = .0f;
                        data.currentSelectedVectorField = "Total Flux";
                        var axis = world.AddVisualisationService<AxisVisualizer>();
                        var structure = world.AddVisualisationService<StructureAccentuatingService>();
                        structure.Colored = false;
                        structure.Integration = true;
                        structure.Reseed = true;
                        structure.DrawTexture = true;
                        axis.DrawTitle = false;
                    });

                DrawWorldPanel(new Vec2(x2, y1), size, zoom: .78,
                    load: (world) =>
                    {
                        var data = world.GetWorldService<DataService>();
                        data.SetDataset("Double Gyre EPS=0.1, Pe=100");
                        data.TimeMultiplier = .0f;
                        data.SimulationTime = 3.4;
                        data.currentSelectedVectorField = "Total Flux";
                        var axis = world.AddVisualisationService<AxisVisualizer>();
                        axis.DrawTitle = false;
                    });


                var arrow_x1 = .52;
                var arrow_x2 = .48;
                var arrow_y1 = .7;
                var arrow_y2 = .2;


                DrawArrow(Presi.RelToSceen(new Vec2(arrow_x1, arrow_y1)), Presi.RelToSceen(new Vec2(arrow_x2, arrow_y1)));
                DrawArrow(Presi.RelToSceen(new Vec2(arrow_x1, arrow_y2)), Presi.RelToSceen(new Vec2(arrow_x2, arrow_y2)));
                Gizmos2D.Text(Presi.View.Camera2D, Presi.RelToSceen(new Vec2(.78, .18)), 90, Color.White, "???");
                Gizmos2D.Text(Presi.View.Camera2D, Presi.RelToSceen(new Vec2(.5, .97)), 40, Color.White, "Instantaneous Structures", centered: true);
                Gizmos2D.Text(Presi.View.Camera2D, Presi.RelToSceen(new Vec2(.5, .43)), 40, Color.White, "Time-Evolving Structures", centered: true);
                //Gizmos2D.Text(Presi.View.Camera2D, Presi.RelToSceen(new Vec2(.48,.5)), 50, Color.White, "==>");
                if (StepEnter())
                {
                    foreach (ref var p in temp.World.GetWorldService<StructureAccentuatingService>().Particles.AsSpan())
                        p.Direction = -1;
                }
            }

            if (view != null && view.World.DataService.SimulationTime > view.World.DataService.ScalerField.Domain.RectBoundary.Max.Z)
            {
                view.World.DataService.SimulationTime = 0;
                view.World.GetWorldService<StochasticVisualization>().Initialize();
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

            bool beginStep = BeginStep();
            bool afterCurrentStep = AfterCurrentStep();
            var x = 0.2;
            var lh = 0.02f;
            var spacing = lh * 3;
            var middleY = .14;
            double lineThick = 0.005f;
            var offsetX = 0.2;
            if (afterCurrentStep)
                offsetX = -.0;
            if (beginStep || afterCurrentStep)
            {
                Title("Thermal velocity");
                if (StepEnter())
                {
                    var grid = temp.World.AddVisualisationService<GridVisualizer>(0);
                    grid.SetGridDiagnostic(new ScalerGridDiagnostic());
                    grid.AltGradient = Gradients.BlueGrayRed;
                    grid.WaitForComputation();
                }





                Presi.Text("'thermal velocity'  = ", new Vec2(.2 + offsetX, middleY), lh, true, Color.White);
                Presi.RectCentered(new Vec2(.4 + offsetX, middleY + .013), new Vec2(.1, lineThick), Color.White);
                //Presi.RectCentered(new Vec2(.45, .15), new Vec2(.1, lineThick), Color.White);
                Presi.Text("Flux", new Vec2(.4 + offsetX, 0.18), lh, true, Color.White);
                Presi.Text("T'", new Vec2(.4 + offsetX, 0.09), lh, true, Color.White);
            }
            if (BeginStep())
            {

                var screenRelToWorld = ScreenRelToWorld(Presi.GetWidgetData("#world", 0), Presi.View.MousePosition);
                var p = new Vec3(screenRelToWorld, temp.World.DataService.SimulationTime);
                double temper = temp.World.DataService.ScalerField.Evaluate(p);
                var flux = temp.World.DataService.VectorField.Evaluate(p);
                var vel = flux / temper;

                if (temp.World.DataService.ScalerField.Domain.Bounding.Bound(screenRelToWorld.Up(0)).XY == screenRelToWorld)
                {
                    var thick = 10f;
                    var dir = vel.Normalized();
                    var Length = vel.Length() * 100;
                    var pos = Presi.View.MousePosition;
                    var color = Color.Green;
                    var bot = pos - dir * Length / 2 - dir * Length * .1f;
                    var top = pos + dir * Length / 2 + dir * Length * .1f;
                    var perpDir = new Vec2(dir.Y, -dir.X) * Length * .8f;
                    var targetPos = perpDir / 2 + (top * .6f + bot * .4f);
                    var targetPos2 = -perpDir / 2 + (top * .6f + bot * .4f);
                    var offset = Vec2.Normalize(-(targetPos - top)) * thick / 2;
                    Gizmos2D.Instanced.RegisterLine(bot, top, color, thick);
                    Gizmos2D.Instanced.RegisterLine(top + offset, targetPos, color, thick);
                    Gizmos2D.Instanced.RegisterLine(top, targetPos2, color, thick);
                }

                //DrawArrow(Presi.View.MousePosition, Presi.View.MousePosition + vel*90);
                /*
                Gizmos2D.Circle(Presi.View.Camera2D, Presi.View.MousePosition,
                    Color.White,vel.Length()*10);
                    */

                Presi.RectCentered(new Vec2(.62, middleY + .013), new Vec2(.2, lineThick), Color.White);
                Presi.Text($"({double.Round(flux.X, 3)}, {double.Round(flux.X, 3)})", new Vec2(.62, 0.18), lh, true, Color.White);
                Presi.Text(double.Round(temper, 5).ToString(), new Vec2(.62, 0.09), lh, true, Color.White);
                Presi.Text("= ", new Vec2(.49, middleY), lh, true, Color.White);
                Presi.Text($"  =  ({double.Round(vel.X, 3)}, {double.Round(vel.X, 3)})", new Vec2(.73, 0.14), lh, false, Color.White);
            }
          
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
        if (BeginSlide())
        {
            var t = DrawWorldPanel(new Vec2(.5,.5), new Vec2(1,.5), zoom: .76,
                load: (w) =>
                {
                    w.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
                    var flux = w.DataService.LoadedDataset.VectorFields["Total Flux"];
                    var T = w.DataService.LoadedDataset.ScalerFields["Convective Temperature"];
                    w.DataService.LoadedDataset.VectorFields["temp"] = new ArbitraryField<Vec3, Vec2>(flux.Domain, x => flux.Evaluate(x)/(T.Evaluate(x)));
                    w.DataService.currentSelectedVectorField = "temp";
                    var stoch = w.AddVisualisationService<StochasticVisualization>();
                    var axis = w.AddVisualisationService<AxisVisualizer>();
                    axis.DrawTitle = false;
                    w.DataService.SimulationTime = 0;
                    w.DataService.TimeMultiplier = .1f;
                    stoch.mode = StochasticVisualization.Mode.TimeIntegration;
                    stoch.reverse = false;
                    stoch.ReseedChance = .1f;
                    stoch.alpha = 1;
                }, "#w", 0);
            Title("Artifacts");
        }
        if (BeginSlide())
        {
            Title("Problems");
            Presi.MainParagraph(
                @"
1. Unsteady flow 
2. Division by (near) zero");

            if (BeginStep() || AfterCurrentStep())
            {
                Presi.LatexCentered(
                    @"$$\mathbf{v}(t) = \begin{pmatrix} v_x(t) \\ v_y(t) \end{pmatrix} = \begin{pmatrix} v_x(t) \\ v_y(t) \\ 1 \end{pmatrix} = \begin{pmatrix} Q_x / T' \\ Q_y / T' \\ 1 \end{pmatrix}$$",
                    new Vec2(.5, .5), .25);
            }

            if (BeginStep() || AfterCurrentStep())
            {
                string latex = @"
$$\begin{pmatrix} Q_x / T' \\ Q_y / T' \\ 1 \end{pmatrix} \Leftrightarrow \begin{pmatrix} Q_x \\ Q_y \\ T' \end{pmatrix}$$
$$\text{ when physical time of trajectories is parameterised via } t(\mathbf{\xi}) = \int_0^\xi T'(\chi^{Q'}(\eta)) \text{d}\eta$$
";
                Presi.LatexCentered(latex, new Vec2(.5, .16), .38);
            }
            if (BeginStep())
            {
                Presi.RectCentered(new Vec2(.15, .805), new Vec2(.265, .01), Color.Yellow);
                Presi.RectCentered(new Vec2(.2, .735), new Vec2(.36, .01), Color.Yellow);
                Presi.MainParagraph(
                    @"
                                         @yellow[3D flow visualization]
                                                 @yellow[Particles at different physical times]");
            }
        }
    }
    private void DrawArrow(Vec2 end, Vec2 start)
    {

        var dir = Vec2.NormalizeSafe(end - start);
        dir = new Vec2(dir.Y, dir.X);
        var top = Utils.Lerp(start, end, .6) + dir * (end - start).Length() / 4;
        var bot = Utils.Lerp(start, end, .6) - dir * (end - start).Length() / 4;
        Gizmos2D.Line(Presi.View.Camera2D, start, end, Color.White, 10.1);
        Gizmos2D.Line(Presi.View.Camera2D, top, end + -dir * (end - start).Length() / 30, Color.White, 10.1);
        Gizmos2D.Line(Presi.View.Camera2D, bot, end + dir * (end - start).Length() / 30, Color.White, 10.1);
    }
    private void StructureAcentuating()
    {

        void StructureSlide(int type)
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
                    data.TimeMultiplier = .0f;
                    data.currentSelectedVectorField = "Total Flux";
                    var axis = world.AddVisualisationService<AxisVisualizer>();
                    var structure = world.AddVisualisationService<StructureAccentuatingService>();
                    structure.Colored = type == 2;
                    axis.DrawTitle = false;
                }, $"#{type}");

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

        if (BeginSlide())
        {
            Title("EuroGraphics Paper (Park et al. 2006)");
            Presi.Image(structure_accentuating, new Vec2(0.5, .33), .9);
            if (BeginStep())
            {
                Presi.Text("*Only for steady flow", new Vec2(0.1,.75), .03, false, Color.Yellow);
            }
        }
    }
    private void Introduction()
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
                    Gizmos2D.AdvText(Presi.View.Camera2D, rt + new Vec2(40, -20), 50f, grad.Get(0.2f), "Cold");
                }

                if (BeginStep())
                {
                    // Title("Temperature");
                    //DrawWalls();
                    Gizmos2D.Rect(Presi.View.Camera2D, lb, rt, grad.Get(.3f));
                    Gizmos2D.AdvText(Presi.View.Camera2D, WorldToScreenRel(worldPanel, new Vec2(0.5, .25)) + new Vec2(0, -20), 40f, Color.White, "Constant Initial Temperature", centered: true);

                }
                if (BeginStep())
                {
                    //Title("Hot/Cold Wall");
                    Gizmos2D.Rect(Presi.View.Camera2D, lb, rt, grad.Get(.3f));
                    Gizmos2D.AdvText(Presi.View.Camera2D, WorldToScreenRel(worldPanel, new Vec2(0.5, .25)) + new Vec2(0, -20), 40f, Color.White, "Constant Initial Temperature", centered: true);
                    DrawWalls();
                }



                if (BeginStep())
                {
                    Title("Heat Simulation");
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

                var size = new Vec2(1, .5) / 1.77;
                var posY = .5;
                var offsetX = -.26;

                if (IsFirstStep())
                {
                    Title("Capturing effect of fluid flow");
                    DrawPanels();
                }

                if (BeginStep())
                {
                    Title("Lagrangian Formalism");
                    size = new Vec2(1, .5) / 2.5;
                    posY = .14;
                    offsetX = -.23;
                    Presi.Image(michel0, new Vec2(0.5, .6), .8);
                    DrawPanels();

                }

                if (BeginStep())
                {
                    Title("Lagrangian Formalism");
                    size = new Vec2(1, .5) / 1.9;
                    posY = .5;
                    offsetX = -.26;
                    DrawPanels();

                }

                if (BeginStep())
                {
                    Title("Lagrangian Formalism");
                    size = new Vec2(1, .5) / 1.96;
                    posY = .66;
                    offsetX = -.28;

                    var lineWidth = .03;
                    DrawPanels();
                    Gizmos2D.Line(Presi.View.Camera2D, Presi.RelToSceen(new Vec2(.5 - lineWidth / 2 - .03, posY)), Presi.RelToSceen(new Vec2(.5 + lineWidth / 2, posY)), Color.White, 20f);


                    var y1 = .18;
                    var y2 = .24;
                    Gizmos2D.Line(Presi.View.Camera2D, Presi.RelToSceen(new Vec2(.1, y1)), Presi.RelToSceen(new Vec2(.2, y1)), Color.White, 16f);
                    Gizmos2D.Line(Presi.View.Camera2D, Presi.RelToSceen(new Vec2(.1, y2)), Presi.RelToSceen(new Vec2(.2, y2)), Color.White, 16f);

                    var vR = DrawWorldPanel(new Vec2(0.5, .22), size, zoom: .78,
                        load: (world) =>
                        {
                            var data = world.GetWorldService<DataService>();
                            var axis = world.AddVisualisationService<AxisVisualizer>();
                            axis.DrawTitle = true;
                            axis.DrawGradient = false;
                            axis.Title = "Convective Temperature";
                            data.SetDataset("Double Gyre EPS=0.1, Pe=100");
                            data.TimeMultiplier = .2f;
                            data.currentSelectedScaler = "Convective Temperature";
                            world.AddVisualisationService<GridVisualizer>().SetGridDiagnostic(new ScalerGridDiagnostic());
                            world.AddVisualisationService<ArrowVisualizer>().Disable();

                        }, "#m");
                    vR.World.DataService.SimulationTime = t;
                    vR.World.GetWorldService<GridVisualizer>().Continous = true;


                }


                void DrawPanels()
                {

                    var viewR = DrawWorldPanel(new Vec2(.5 - offsetX, posY), size, zoom: .78,
                        load: (world) =>
                        {
                            var data = world.GetWorldService<DataService>();
                            var axis = world.AddVisualisationService<AxisVisualizer>();
                            axis.DrawTitle = true;
                            axis.DrawGradient = false;
                            axis.Title = "Temperature without Flow";
                            data.SetDataset("Double Gyre EPS=0.1, Pe=100");
                            data.currentSelectedScaler = "No Flow Temperature";
                            world.AddVisualisationService<GridVisualizer>().SetGridDiagnostic(new ScalerGridDiagnostic());
                        }, "#r");

                    var view = DrawWorldPanel(new Vec2(.5 + offsetX, posY), size, zoom: .78,
                        load: (world) =>
                        {
                            var data = world.GetWorldService<DataService>();
                            var axis = world.AddVisualisationService<AxisVisualizer>();
                            axis.DrawTitle = true;
                            axis.DrawGradient = false;
                            axis.Title = "Temperature with Flow";
                            data.SetDataset("Double Gyre EPS=0.1, Pe=100");
                            data.currentSelectedScaler = "Total Temperature";
                            world.AddVisualisationService<GridVisualizer>().SetGridDiagnostic(new ScalerGridDiagnostic());
                        }, "#l");



                    t += Presi.View.World.FlowExplainer.DeltaTime / 2;
                    if (t + 1 >= view.World.DataService.ScalerField.Domain.RectBoundary.Max.Z)
                    {
                        t = 0.01f;
                    }
                    view.World.GetWorldService<GridVisualizer>().Continous = true;
                    viewR.World.GetWorldService<GridVisualizer>().Continous = true;
                    view.World.DataService.SimulationTime = t;
                    viewR.World.DataService.SimulationTime = t;
                }
            }
        }

        if (BeginSlide())
        {
            var relCenterPos = new Vec2(.5, .56);
            double d = .95;


            if (IsFirstStep())
            {
                var view = DrawMainView();
                Title("Computing Heat Flux");
                t = 1.2;
            }
            if (SlideEnter())
            {
                var view = DrawMainView();
                view.World.GetWorldService<AxisVisualizer>().DrawTitle = false;
            }
            if (BeginStep())
            {
                var view = DrawMainView();
                if (!view.World.GetWorldService<ArrowVisualizer>().IsEnabled)
                {
                    view.World.GetWorldService<ArrowVisualizer>().Enable();
                    view.World.GetWorldService<ArrowVisualizer>().colorByGradient = false;
                    //  view.World.GetWorldService<GridVisualizer>().Disable();
                }

                view.World.DataService.currentSelectedVectorField = "Convection Flux";
                Title("Convection Flux");
                Presi.LatexCentered("$\\boldsymbol{u}T$", new Vec2(.5, .08), .17);
            }

            if (BeginStep())
            {
                var view = DrawMainView();
                view.World.DataService.currentSelectedVectorField = "Diffusion Flux";
                Title("Diffusion Flux");
                Presi.LatexCentered("$$-\\frac{1}{Pe}\\nabla T'$$", new Vec2(.5, .08), .27);
            }

            if (BeginStep())
            {
                var view = DrawMainView();
                if (!view.World.GetWorldService<ArrowVisualizer>().IsEnabled)
                {
                    view.World.GetWorldService<ArrowVisualizer>().Enable();
                    view.World.GetWorldService<ArrowVisualizer>().colorByGradient = false;
                    //  view.World.GetWorldService<GridVisualizer>().Disable();
                }

                view.World.DataService.currentSelectedVectorField = "Total Flux";
                Title("Total Flux");
                Presi.LatexCentered("$$\\boldsymbol{u}T -\\frac{1}{Pe}\\nabla T'$$", new Vec2(.5, .08), .27);
            }

            if (BeginStep())
            {
                var view = DrawMainView();
                if (!view.World.GetWorldService<ArrowVisualizer>().IsEnabled)
                {
                    view.World.GetWorldService<ArrowVisualizer>().Enable();
                    view.World.GetWorldService<ArrowVisualizer>().colorByGradient = false;
                    //  view.World.GetWorldService<GridVisualizer>().Disable();
                }

                view.World.DataService.currentSelectedVectorField = "Total Flux";
                Title("Total Flux");
                Presi.Slider("t", ref t, 0.01, 5, new Vec2(0.5, .1), .8f);
            }

            if (BeginStep())
            {
                Title("Coherent Strucutres");
                Presi.Image(michel1, new Vec2(.5, .5), 1);
                d = .4;
                relCenterPos = new Vec2(.8, .4);
                // var view = DrawMainView();
            }

            View DrawMainView()
            {
                var view1 = DrawWorldPanel(relCenterPos, new Vec2(1, .5) * d, zoom: .78,
                    load: (world) =>
                    {
                        var data = world.GetWorldService<DataService>();
                        var axis = world.AddVisualisationService<AxisVisualizer>();
                        data.SetDataset("Double Gyre EPS=0.1, Pe=100");
                        data.currentSelectedScaler = "Total Temperature";
                        world.AddVisualisationService<GridVisualizer>().SetGridDiagnostic(new ScalerGridDiagnostic());
                        world.AddVisualisationService<ArrowVisualizer>().Disable();
                    }, "#m");
                view1.World.DataService.SimulationTime = t;
                return view1;
            }
        }

        if (BeginSlide())
        {
            Title("Broad Goal");
            Presi.MainParagraph(
                @"


- Visualize thermal structures to give insight into heat transfer
    - How to define them?
    - How to visualize them?
    - Physical meaning?
");

            if (BeginStep())
            {
                Presi.Text("First project: Attracting/Repelling structures", new Vec2(.1, .3), .02, false, Color.Green);
            }
        }
    }


}