using System.Runtime.CompilerServices;

namespace FlowExplainer;

public class VariationalPresentation : NewPresentation
{
    public static Dataset DatasetDoubleGyreContained;

    bool ftle;

    public ImageTexture fig4 = new ImageTexture("Assets/Images/Presi/fig4.png");
    public ImageTexture fig6 = new ImageTexture("Assets/Images/Presi/fig6.png");
    public ImageTexture fig7 = new ImageTexture("Assets/Images/Presi/fig7.png");
    public ImageTexture fig13 = new ImageTexture("Assets/Images/Presi/fig13.png");


    public class TrajDrawerService : WorldService
    {
        public IVectorField<Vec2, Vec2>? VectorField;
        public Trajectory<Vec2>[] Trajectories;
        public override void Initialize()
        {

        }
        public override void Draw(View view)
        {
            if (Trajectories == null)
                return;

            foreach (var traj in Trajectories)
            {
                foreach (var seg in traj.EnumerateSegments())
                {
                    Gizmos2D.Instanced.RegisterLine(seg.start, seg.end, Color.Green, .004f);
                }
            }
            Gizmos2D.Instanced.RenderRects(view.Camera2D);
        }
    }


    public class IntegratorService : WorldService
    {
        public IVectorField<Vec2, double>? ValidSubspace;
        public IVectorField<Vec2, Vec2>? VectorField;
        public Vec2 mousepos;
        public override void Initialize()
        {

        }
        public override void Draw(View view)
        {
            if (ValidSubspace == null || VectorField == null)
                return;

            var rk4 = IIntegrator<Vec2, Vec2>.Rk4Steady;
            if (VectorField.Domain.IsWithinBounds(mousepos))
            {
                var traj = VariationalLCS.IntegrateStrainLineFromLocalMax(mousepos, VectorField, ValidSubspace, .01, .3);
                if (traj.HasValue)
                    foreach (var seg in traj.Value.EnumerateSegments())
                    {
                        Gizmos2D.Instanced.RegisterLine(seg.start, seg.end, Color.Green, .01f);
                    }
            }
            Gizmos2D.Instanced.RenderRects(view.Camera2D);
        }
    }

    public class LocalMaxService : WorldService
    {
        public IVectorField<Vec2, double>? Eigen2;
        public IVectorField<Vec2, Vec2> EigenVector2;
        private IVectorField<Vec2, Vec2> eigen2Grad;
        public Vec2 mousepos;
        public override void Initialize()
        {

        }

        class Particle
        {
            public Vec2 pos;
        }

        private List<Particle> localMaximaParticles = new();
        public bool Move;
        public void SpawnParticles()
        {
            localMaximaParticles.Clear();

            Vec2i sseedGridSize = new Vec2i(32, 16) * 2;
            for (int i = 0; i < sseedGridSize.X; i++)
            for (int j = 0; j < sseedGridSize.Y; j++)
            {
                var domainRectBoundary = Eigen2.Domain.RectBoundary;
                var pos = domainRectBoundary.FromRelative(new Vec2(i, j) / (sseedGridSize.ToVec2() - Vec2.One));
                localMaximaParticles.Add(new Particle()
                {
                    pos = pos
                });
            }
            eigen2Grad = new ArbitraryField<Vec2, Vec2>(Eigen2.Domain, x => Eigen2.FiniteDifferenceGradient(x, .008));
        }

        public void Filter()
        {
            for (int i = localMaximaParticles.Count - 1; i >= 0; i--)
            {
                var p = localMaximaParticles[i];
                var finiteDifferenceGradient = Eigen2.FiniteDifferenceGradient(p.pos, .008);
                //if (finiteDifferenceGradient.Length() > 0.0001)
                if (double.Abs(Vec2.Dot(finiteDifferenceGradient, EigenVector2.Evaluate(p.pos))) > .00001)
                {
                    localMaximaParticles.RemoveAt(i);
                }
            }
        }

        public override void Draw(View view)
        {
            var stepSize = .000001;
            if (Move)
                for (int i = 0; i < 14; i++)
                {
                    var rk4 = IIntegrator<Vec2, Vec2>.Rk4Steady;
                    Parallel.ForEach(localMaximaParticles, p => { p.pos = rk4.Integrate(eigen2Grad, p.pos, stepSize); });
                }
            foreach (var p in localMaximaParticles)
            {
                Gizmos2D.Instanced.RegisterCircle(p.pos, .003f, Color.Red);
            }
            Gizmos2D.Instanced.RenderCircles(view.Camera2D);
        }
    }

    public double t;
    public override void Draw()
    {
        if (BeginSlide())
        {
            var view = DrawWorldPanel(new Vec2(.5, .44), new Vec2(1, .5), zoom: .34,
                load: (world) =>
                {
                    SetupContainedWorld(world);
                    var vec = DatasetDoubleGyreContained.VectorFields.First().Value.ReducedSlice<Vec3, Vec2, Vec2>(() => t);
                    world.AddVisualisationService(new ArrowVisualizer()
                    {
                        Vectorfield = new Artifact<IVectorField<Vec2, Vec2>>(vec, "v", "")
                    });
                });
            t += .1;
            Presi.LatexCentered(@"$$\dot{\mathbf{x}} = \mathbf{v}(\mathbf{x}, t), \quad \mathbf{x} \in U, \quad t \in [\alpha, \beta]$$", new Vec2(.5, .93), .15);
            Presi.LatexCentered(@"$$\mathbf{F}_{t_0}^t(\mathbf{x}_0) := \mathbf{x}(t, t_0, \mathbf{x}_0)$$", new Vec2(.5, .84), .15);
            // Presi.Slider("t", ref t, 0, 10, new Vec2(.5,.1), .7);
        }

        if (BeginSlide())
        {
            //Title("FTLE");
            Presi.LatexCentered(
                "$\\mathbf{C}_{t_0}^{t_0+T} =\\left(\\nabla \\mathbf{F}_{t_0}^{t_0+T}\\right)^{T}\\left(\\nabla\\mathbf{F}_{t_0}^{t_0+T}\\right)$",
                new Vec2(0.5, .8), .24);
            Presi.Text("Eigenvalue Decomposition:", new Vec2(0.1, .53), .02, false, Color.White);
            Presi.LatexCentered(
                "\\begin{align*}\n\\lambda_1 &: \\text{minimum stretching factor squared},\\\\[4pt]\n\\lambda_2 &: \\text{maximum stretching factor squared},\\\\[4pt]\n\\boldsymbol{\\xi}_1 &: \\text{direction of minimum stretching},\\\\[4pt]\n\\boldsymbol{\\xi}_2 &: \\text{direction of maximum stretching}.\n\\end{align*}",
                new Vec2(0.5, .3), .55);
        }


        if (BeginSlide())
        {
            var view = DrawWorldPanel(new Vec2(.5, .4), new Vec2(1, .5), zoom: .34,
                load: (world) =>
                {
                    SetupContainedWorld(world);
                    var grid = world.AddVisualisationService<GridVisualizer>();
                    grid.SetGridDiagnostic(new Scaler2DGridDiagnostic()
                    {
                        ScalerField = Artifacts[1].Get<IVectorField<Vec2, double>>("FTLE"),
                    });
                    grid.max = .36;
                    grid.AutoScale = false;
                    grid.TargetCellCount = 150_000;
                });
            //Title("FTLE");
            Presi.LatexCentered(
                "$\nFTLE_{t_0}^{T}(\\mathbf{x}_0)\n=\n\\frac{1}{|T|}\n\\ln\n\\sqrt{\n\\max(\\lambda_1,\\lambda_2)\n}$",
                new Vec2(0.5, .85), .18);
            view.Camera2D.Position = -new Vec2(2, 1) / 2;
        }

        if (BeginSlide(""))
        {
            FuncCompare(fig4, (p) => new Vec2(p.X, -p.Y - double.Pow(p.Y, 3)));
        }
        if (BeginSlide(""))
        {
            FuncCompare(fig6, (p) => new Vec2(2 + double.Tanh(p.Y), 0));
        }

        if (BeginSlide(""))
        {
            FuncCompare(fig7, (p) => new Vec2(1 + double.Tanh(p.X) * double.Tanh(p.X), -p.Y));
        }

        if (BeginSlide())
        {
            Title("Sufficient and necessary conditions\r\n for LCSs in two-dimensional flows");
            Presi.LatexCentered(
                @"
$$ 
\begin{aligned}  
&(A)\quad \lambda_1(\mathbf{x}_0) \neq \lambda_2(\mathbf{x}_0) > 1 \\  
&(B)\quad \left\langle \boldsymbol{\xi}_2(\mathbf{x}_0), \nabla^2 \lambda_2(\mathbf{x}_0)\, \boldsymbol{\xi}_2(\mathbf{x}_0) \right\rangle < 0 \\  
&(C)\quad \boldsymbol{\xi}_2(\mathbf{x}_0) \perp \mathcal{M}(t_0) \\  
&(D)\quad \left\langle \nabla \lambda_2(\mathbf{x}_0), \boldsymbol{\xi}_2(\mathbf{x}_0) \right\rangle = 0  
\end{aligned} $$", new Vec2(.5, .45), .55);
        }
        if (BeginSlide())
        {
            var view = DrawWorldPanel(new Vec2(.5, .45), new Vec2(1, .5), zoom: .3,
                load: (world) =>
                {
                    SetupContainedWorld(world);
                    var grid = world.AddVisualisationService<GridVisualizer>();
                    grid.SetGridDiagnostic(new Scaler2DGridDiagnostic()
                    {
                        ScalerField = Artifacts[1].Get<IVectorField<Vec2, double>>("Valid Subspace"),
                    });
                    grid.Bilinear = false;
                    grid.TargetCellCount = 150_000;
                    /*world.AddVisualisationService<AxisVisualizer>();
                    world.AddVisualisationService(new ArrowVisualizer()
                    {
                        Vectorfield = Artifacts.Get<IVectorField<Vec2, Vec2>>("Lambda 2 Gradient"),
                        GridCells = 10_000
                    });*/
                });

            Presi.LatexCentered(
                @"
$$ 
\begin{aligned}  
&(A)\quad \lambda_1(\mathbf{x}_0) \neq \lambda_2(\mathbf{x}_0) > 1 \\  
&(B)\quad \left\langle \boldsymbol{\xi}_2(\mathbf{x}_0), \nabla^2 \lambda_2(\mathbf{x}_0)\, \boldsymbol{\xi}_2(\mathbf{x}_0) \right\rangle < 0 \end{aligned} $$",
                new Vec2(.5, .85), .25);
            view.Camera2D.Position = -new Vec2(2, 1) / 2;
        }

        /*if (BeginSlide())
        {
            var view = DrawWorldPanel(new Vec2(.5, .5), new Vec2(1, .5), zoom: .35,
                load: (world) =>
                {
                    SetupContainedWorld(world);
                    var vectorfield = Artifacts.Get<IVectorField<Vec2, Vec2>>("Eigen Vector 1");
                    ((RegularGridVectorField<Vec2, Vec2i, Vec2>)vectorfield.Value).Interpolator = new NoGridInterpolator<Vec2, Vec2i, Vec2>();
                    world.AddVisualisationService(new ArrowVisualizer()
                    {
                        Vectorfield = vectorfield,
                        GridCells = 500
                    });
                });
            view.Camera2D.Position = -new Vec2(2, 1) / 2;
            Presi.LatexCentered("$\\boldsymbol{\\xi}_1$", new Vec2(0.5, .9), .14);
        }


        if (BeginSlide())
        {
            var view = DrawWorldPanel(new Vec2(.5, .5), new Vec2(1, .5), zoom: .35,
                load: (world) =>
                {
                    SetupContainedWorld(world);
                    var vectorfield = Artifacts.Get<IVectorField<Vec2, Vec2>>("Eigen Vector 2");
                    ((RegularGridVectorField<Vec2, Vec2i, Vec2>)vectorfield.Value).Interpolator = new NoGridInterpolator<Vec2, Vec2i, Vec2>();
                    world.AddVisualisationService(new ArrowVisualizer()
                    {
                        Vectorfield = vectorfield,
                        GridCells = 500
                    });
                });
            view.Camera2D.Position = -new Vec2(2, 1) / 2;
            Presi.LatexCentered("$\\boldsymbol{\\xi}_2$", new Vec2(0.5, .9), .14);
        }*/

        for (int i = 1; i <= 3; i++)
        {
            i = 3;
            var zoomFactor = 1.0;
            if (i > 1)
                zoomFactor = 2;


            if (i == 2 && BeginSlide())
            {
                Title("Forwards LCSs in Convective Flux");
                Presi.LatexCentered(@"
$$\begin{aligned} 
\dot{\mathbf{x}}= uT', t=0, T=3
\end{aligned}$$", new Vec2(0.5, .5), .2);
            }
            
            if (i == 3 && BeginSlide())
            {
                Title("Backwords LCSs in Convective Flux");
                Presi.LatexCentered(@"
$$\begin{aligned} 
\dot{\mathbf{x}}= uT', t=3, T=-3
\end{aligned}$$", new Vec2(0.5, .5), .2);
            }
            if (BeginSlide())
            {
                var view = DrawWorldPanel(new Vec2(.5, .5), new Vec2(1, .5), zoom: .35*zoomFactor,
                    load: (world) =>
                    {
                        SetupContainedWorld(world, i);
                        var vectorfield = Artifacts[i].Get<IVectorField<Vec2, Vec2>>("Eigen Vector 2");
                        ((RegularGridVectorField<Vec2, Vec2i, Vec2>)vectorfield.Value).Interpolator = new NoGridInterpolator<Vec2, Vec2i, Vec2>();
                        world.AddVisualisationService(new ArrowVisualizer()
                        {
                            Vectorfield = vectorfield,
                            GridCells = 800
                        });
                    });
                view.Camera2D.Position = -new Vec2(2, 1) / (2*zoomFactor);
                Presi.LatexCentered("(C) $\\boldsymbol{\\xi}_2(\\mathbf{x}_0) \\perp \\mathcal{M}(t_0)$", new Vec2(0.5, .9), .14);
            }

            if (BeginSlide())
            {
                var view = DrawWorldPanel(new Vec2(.5, .5), new Vec2(1, .5), zoom: .35*zoomFactor,
                    load: (world) =>
                    {
                        SetupContainedWorld(world,i);
                        var vectorfield = Artifacts[i].Get<IVectorField<Vec2, Vec2>>("Scaled Eigen Vector 2 Perp");
                        world.AddVisualisationService(new ArrowVisualizer()
                        {
                            Vectorfield = vectorfield,
                            GridCells = 1000
                        });
                        world.AddVisualisationService(new IntegratorService()
                        {
                            VectorField = vectorfield.Value,
                            //ValidSubspace =  Artifacts.Get<IVectorField<Vec2, double>>("Valid Subspace").Value,
                            ValidSubspace = IVectorField<Vec2, double>.Constant(1),
                        });
                    }, "#world", 0);
                var screenRelToWorld = ScreenRelToWorld(Presi.GetWidgetData("#world", 0), Presi.View.MousePosition);
                view.World.GetWorldService<IntegratorService>().mousepos = screenRelToWorld;
                view.Camera2D.Position = -new Vec2(2, 1) / (2*zoomFactor);
                Presi.LatexCentered("$\\boldsymbol{\\xi}'^\\perp_2(\\mathbf{x}_0)  \\| \\mathcal{M}(t_0)$", new Vec2(0.5, .9), .14);

            }

            if (BeginSlide())
            {

                var view = DrawWorldPanel(new Vec2(.5, .5), new Vec2(1, .5), zoom: .35*zoomFactor,
                    load: (world) =>
                    {
                        SetupContainedWorld(world, i);
                        var vectorfield = Artifacts[i].Get<IVectorField<Vec2, Vec2>>("Scaled Eigen Vector 2 Perp");

                        var grid = world.AddVisualisationService<GridVisualizer>();
                        grid.AltGradient = new ColorGradient("t", [(0, Color.Black), (1, Color.White)]);
                        grid.SetGridDiagnostic(new Scaler2DGridDiagnostic()
                        {
                            ScalerField = Artifacts[i].Get<IVectorField<Vec2, double>>("Valid Subspace"),
                        });
                        grid.Bilinear = false;
                        grid.TargetCellCount = 150_000;
                        world.AddVisualisationService(new IntegratorService()
                        {
                            VectorField = vectorfield.Value,
                            ValidSubspace = Artifacts[i].Get<IVectorField<Vec2, double>>("Valid Subspace").Value,
                            //ValidSubspace = IVectorField<Vec2,double>.Constant(1),
                        });
                    }, "#world1", 0);
                var screenRelToWorld = ScreenRelToWorld(Presi.GetWidgetData("#world1", 0), Presi.View.MousePosition);
                view.World.GetWorldService<IntegratorService>().mousepos = screenRelToWorld;
                view.Camera2D.Position = -new Vec2(2, 1) / (2*zoomFactor);
                Presi.LatexCentered("$\\text{Valid subspace and } \\boldsymbol{\\xi}'^\\perp_2(\\mathbf{x}_0)  \\| \\mathcal{M}(t_0)$", new Vec2(0.5, .9), .14);

            }

            if (i == 1 && BeginSlide())
            {
                Presi.LatexCentered("(D) $\\left\\langle \\nabla \\lambda_2(\\mathbf{x}_0), \\boldsymbol{\\xi}_2(\\mathbf{x}_0) \\right\\rangle = 0$",
                    new Vec2(.5, .5), .17, "#D");
            }
            if (BeginSlide())
            {

                var view = DrawWorldPanel(new Vec2(.5, .4), new Vec2(1, .5), zoom: .39*zoomFactor,
                    load: (world) =>
                    {
                        SetupContainedWorld(world,i);
                        var grid = world.AddVisualisationService<GridVisualizer>();
                        var scalerField = Artifacts[i].Get<IVectorField<Vec2, double>>("Log Lambda 2");
                        grid.SetGridDiagnostic(new Scaler2DGridDiagnostic()
                        {
                            ScalerField = scalerField,
                        });
                        var localMaxService = new LocalMaxService()
                        {
                            Eigen2 = scalerField.Value,
                            EigenVector2 = Artifacts[i].Get<IVectorField<Vec2, Vec2>>("Eigen Vector 2").Value
                        };
                        world.AddVisualisationService(localMaxService);
                        grid.TargetCellCount = 150_000;
                    });
                view.Camera2D.Position = -new Vec2(2, 1) / (2*zoomFactor);

                var localMaxService = view.World.GetWorldService<LocalMaxService>();
                Presi.LatexCentered("(D) $\\left\\langle \\nabla \\lambda_2(\\mathbf{x}_0), \\boldsymbol{\\xi}_2(\\mathbf{x}_0) \\right\\rangle = 0$",
                    new Vec2(.5, .95), .15, "#D");
                if (IsFirstStep())
                {
                    Presi.LatexCentered("$\\text{ln}(\\lambda_2)$",
                        new Vec2(.5, .8), .15);
                }
                if (BeginStep())
                {
                    Presi.LatexCentered("$\\text{Integrate along } \\nabla \\lambda_2$",
                        new Vec2(.5, .8), .15);
                    if (StepEnter())
                        localMaxService.SpawnParticles();
                }

                if (BeginStep())
                {
                    Presi.LatexCentered("$\\text{Integrate along } \\nabla \\lambda_2$",
                        new Vec2(.5, .8), .15);
                    localMaxService.Move = true;
                }

                if (BeginStep())
                {
                    Presi.LatexCentered(@"$$|\left\langle \nabla \lambda_2(x), \boldsymbol{\xi}_2(x) \right\rangle| < \epsilon$$",
                        new Vec2(.5, .8), .15);
                    if (StepEnter())
                        localMaxService.Filter();
                    localMaxService.Move = false;
                }
            }
            if (i == 1 && BeginSlide())
            {
                Title("Sufficient and necessary conditions\r\n for LCSs in two-dimensional flows");
                Presi.LatexCentered(
                    @"
$$ 
\begin{aligned}  
&(A)\quad \lambda_1(\mathbf{x}_0) \neq \lambda_2(\mathbf{x}_0) > 1 \\  
&(B)\quad \left\langle \boldsymbol{\xi}_2(\mathbf{x}_0), \nabla^2 \lambda_2(\mathbf{x}_0)\, \boldsymbol{\xi}_2(\mathbf{x}_0) \right\rangle < 0 \\  
&(C)\quad \boldsymbol{\xi}_2(\mathbf{x}_0) \perp \mathcal{M}(t_0) \\  
&(D)\quad \left\langle \nabla \lambda_2(\mathbf{x}_0), \boldsymbol{\xi}_2(\mathbf{x}_0) \right\rangle = 0  
\end{aligned} $$", new Vec2(.5, .45), .55);
            }

            if (BeginSlide())
            {
                Title("FTLE");
                var view = DrawWorldPanel(new Vec2(.5, .5), new Vec2(1, .5), zoom: .34*zoomFactor,
                    load: (world) =>
                    {
                        SetupContainedWorld(world, i);
                        var grid = world.AddVisualisationService<GridVisualizer>();
                        grid.SetGridDiagnostic(new Scaler2DGridDiagnostic()
                        {
                            ScalerField = Artifacts[i].Get<IVectorField<Vec2, double>>("FTLE"),
                        });
                        grid.AutoScale = false;
                        grid.max = .35;
                        grid.TargetCellCount = 150_000;

                    });
                view.Camera2D.Position = -new Vec2(2, 1) / (2*zoomFactor);
            }

            if (BeginSlide())
            {
                Title("Variational LCS");
                var view = DrawWorldPanel(new Vec2(.5, .5), new Vec2(1, .5), zoom: .34*zoomFactor,
                    load: (world) =>
                    {
                        SetupContainedWorld(world, i);
                        var traj = world.AddVisualisationService<TrajDrawerService>();
                        traj.Trajectories = Artifacts[i].Get<TrajectoryGroup<Vec2>>("trajs").Value.Trajectories;
                    });
                view.Camera2D.Position = -new Vec2(2, 1) / (2*zoomFactor);
            }
        }

        void FuncCompare(Texture image, Func<Vec2, Vec2> field)
        {

            Presi.Image(image, new Vec2(0.24, .5), .5);
            Presi.Text("(Source: G. Haller 2011)", new Vec2(0.02, .03), .015, false, Color.White);
            if (ftle)
                Presi.Text("FTLE", new Vec2(0.77, .9), .03, false, Color.White);

            var view = DrawWorldPanel(new Vec2(.8, .55), new Vec2(1) / 2.5, zoom: .5f, load: w =>
            {
                w.DataService.SetDataset(DatasetDoubleGyreContained);
                var vec = new ArbitraryField<Vec2, Vec2>(new RectDomain<Vec2>(-Vec2.One, Vec2.One), field);
                var domainUp = new RectDomain<Vec3>(vec.Domain.RectBoundary.Min.Up(0), vec.Domain.RectBoundary.Max.Up(1));
                w.DataService.LoadedDataset.VectorFields["t"] = new ArbitraryField<Vec3, Vec2>(domainUp, p => vec.Evaluate(p.XY));
                w.DataService.currentSelectedVectorField = "t";

                var stochAttracting = w.AddVisualisationService<StochasticVisualization>();
                stochAttracting.mode = StochasticVisualization.Mode.Instantaneous;
                stochAttracting.Count = 40000;
                stochAttracting.dt = .02f;
                stochAttracting.RenderRadius = .04f;
                stochAttracting.alpha = .2f;
                stochAttracting.ReseedChance = .2;
                stochAttracting.Color = new Color(.3, 1f, .3f, 1);

                if (image != fig6)
                {
                    var stochRepelling = w.AddVisualisationService<StochasticVisualization>();
                    stochRepelling.mode = StochasticVisualization.Mode.Instantaneous;
                    stochRepelling.Count = 40000;
                    stochRepelling.dt = .02f;
                    stochRepelling.RenderRadius = .04f;
                    stochRepelling.alpha = .2f;
                    stochRepelling.ReseedChance = .2;
                    stochRepelling.Color = new Color(1, .3f, .3f, 1);
                    stochRepelling.reverse = true;
                }
                w.DataService.TimeMultiplier = .5f;
                w.DataService.SimulationTime = 0;
                ftle = true;
                var grid = w.AddVisualisationService<GridVisualizer>();
                grid.SetGridDiagnostic(new FTLEGridDiagnostic());
                if (!ftle)
                    grid.Disable();
                w.AddVisualisationService<ArrowVisualizer>().colorByGradient = false;

                w.GetWorldService<ArrowVisualizer>().AutoResize = false;
                w.GetWorldService<ArrowVisualizer>().Length = .08;
                w.GetWorldService<ArrowVisualizer>().GridCells = 100;
                w.GetWorldService<ArrowVisualizer>().Thickness = .01;


            }, filePath: image.GetHashCode().ToString());

            Presi.Checkbox("Show FTLE", ref ftle, new Vec2(.7, .1));
            if (ftle != view.World.GetWorldService<GridVisualizer>().IsEnabled)
            {
                if (view.World.GetWorldService<GridVisualizer>().IsEnabled)
                    view.World.GetWorldService<GridVisualizer>().Disable();
                else
                    view.World.GetWorldService<GridVisualizer>().Enable();
            }
            view.Camera2D.Position = Vec2.Zero;
        }
    }


    static VariationalPresentation()
    {
        DatasetDoubleGyreContained = new Dataset(new()
            {
                {
                    "Name", "Double Gyre Contained"
                },
            },
            (d) =>
            {
                d.VectorFields.Add("Velocity", new AnalyticalEvolvingVelocityField()
                {
                    A = 0.1,
                    w = 2 * double.Pi / 10,
                    epsilon = 0.1,
                });
            });
    }

    static string folder = "variational";



    public static void RecomputeDataset(FlowExplainer flowExplainer, int d)
    {
        if (!Directory.Exists(folder))
        Directory.CreateDirectory(folder);
        var world = flowExplainer.GetGlobalService<WorldManagerService>().Worlds[0];
        var variational = world.AddVisualisationService<VariationalLCS>();
        if (d == 1)
        {
            world.DataService.SetDataset(DatasetDoubleGyreContained);
            variational.VelocityField = world.DataService.Artifacts.Get<IVectorField<Vec3, Vec2>>("Velocity");
            variational.t0 = 0;
            variational.T = 20;
        }
        if (d == 2)
        {
            world.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
            variational.VelocityField = world.DataService.Artifacts.Get<IVectorField<Vec3, Vec2>>("Convection Flux");
            variational.t0 = 0;
            variational.T = 3;
        }
        if (d == 3)
        {
            world.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
            variational.VelocityField = world.DataService.Artifacts.Get<IVectorField<Vec3, Vec2>>("Convection Flux");
            variational.t0 = 2;
            variational.T = -2;
            variational.l_min = .3;
        }
        variational.Recompute();
        string su = Path.Combine(folder, "contained"+d);
        if(Directory.Exists(su))
           Directory.Delete(su,true);
        Directory.CreateDirectory(su);
        foreach (var artifact in variational.Artifacts)
        {
            ArtifactSerializer.Save(artifact, Path.Combine(folder, "contained"+d, artifact.DisplayName));
        }

    }

    private Dictionary<int,ArtifactsManager> Artifacts = new();


    public void LoadDataset()
    {
        Artifacts = new Dictionary<int, ArtifactsManager>();
        foreach (var subfolder in Directory.GetDirectories(folder))
        {
            var index = int.Parse(subfolder.Last().ToString());
            Artifacts.Add(index, new());
            var Artifacts1 = Artifacts[index];
            foreach (var f in Directory.GetFiles(subfolder))
            {
                var artifact = ArtifactSerializer.Load(f);
                Artifacts1.RegisterOrUpdate(artifact);
            }
            var l1 = Artifacts1.Get<IVectorField<Vec2, double>>("Lambda 1").Value;
            var l2 = Artifacts1.Get<IVectorField<Vec2, double>>("Lambda 2").Value;
            var scaledEigenVector2 = Artifacts1.Get<IVectorField<Vec2, Vec2>>("Scaled Eigen Vector 2").Value;


            ((RegularGridVectorField<Vec2, Vec2i, Vec2>)scaledEigenVector2).Interpolator = new OrientedLinearInterpolation();

            Artifacts1.RegisterOrUpdate(new Artifact<IVectorField<Vec2, double>>(
                new ArbitraryField<Vec2, double>(l1.Domain,
                    x => 1.0 / 20.0 * double.Log(double.Sqrt(double.Max(l1.Evaluate(x), l2.Evaluate(x))))), "FTLE", ""));

            Artifacts1.RegisterOrUpdate(new Artifact<IVectorField<Vec2, double>>(
                new ArbitraryField<Vec2, double>(l2.Domain, x => double.Log(l2.Evaluate(x))), "Log Lambda 2", ""));

            Artifacts1.RegisterOrUpdate(new Artifact<IVectorField<Vec2, Vec2>>(
                new ArbitraryField<Vec2, Vec2>(l1.Domain, x =>
                {
                    var r = scaledEigenVector2.Evaluate(x);
                    return new Vec2(-r.Y, r.X);
                }), "Scaled Eigen Vector 2 Perp", ""));
        }
    }

    private void SetupContainedWorld(World world, int d = 1)
    {
        var data = world.GetWorldService<DataService>();
        data.SetDataset(DatasetDoubleGyreContained);
        if(d != 1)
            data.SetDataset("Double Gyre EPS=0.1, Pe=100");
        var axis = world.AddVisualisationService<AxisVisualizer>();
        foreach (var artifact in Artifacts[d])
            data.Artifacts.RegisterOrUpdate(artifact);
        axis.DrawTitle = false;
    }
}