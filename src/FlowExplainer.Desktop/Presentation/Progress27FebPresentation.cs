namespace FlowExplainer;

public class Progress27FebPresentation : NewPresentation
{

    public override void Draw()
    {
        Gizmos2D.RectCenter(Presi.View.Camera2D, new Vec2(), new Vec2(8000, 8000), Color.Grey(1f));
        
        if (BeginSlide())
        {
            Presi.LatexCentered(
                @"
Lagrangian transport of particles occurs along trajectories is described by the following kinematic equations and their formal solutions:
\begin{align}
\frac{d\boldsymbol{x}}{dt}
&= \boldsymbol{u}(\boldsymbol{x}(t), t)
&\Rightarrow\quad
\boldsymbol{x}(t)
&= \boldsymbol{x}_0
+ \int_0^t \boldsymbol{u}(\boldsymbol{x}(\eta), \eta)\, d\eta
\equiv \boldsymbol{\Phi}_t(\boldsymbol{x}_0)
\tag{10} \\[1ex]
%
\frac{d\boldsymbol{x}^M}{d\xi}
&= \boldsymbol{M}_n(\boldsymbol{x}(\xi), \xi)
&\Rightarrow\quad
\boldsymbol{x}^M(\xi)
&= \boldsymbol{x}_0
+ \int_0^\xi \boldsymbol{M}_n(\boldsymbol{x}(\eta), \eta)\, d\eta
\equiv \boldsymbol{\Phi}^M_\xi(\boldsymbol{x}_0)
\tag{11} \\[1ex]
%
\frac{d\boldsymbol{x}^M}{dt}
&= \boldsymbol{M}_n(\boldsymbol{x}(t), t)
&\Rightarrow\quad
\boldsymbol{x}^M(t)
&= \boldsymbol{x}_0
+ \int_0^t \boldsymbol{M}_n(\boldsymbol{x}(\eta), \eta)\, d\eta
\equiv \boldsymbol{\Phi}^M_t(\boldsymbol{x}_0)
\tag{12}
\end{align}
with ﬁctitious time $\xi$ in (11) relating to physical time $t$ via
\[
\frac{dt}{d\xi} = n\bigl(x^{M}(\xi), \xi\bigr)
 \;\;\Rightarrow\;\;
t = f(\xi) = \int_{0}^{\xi} n\bigl(x^{M}(\eta), \eta\bigr)\, d\eta .
\]
",
                new Vec2(.5, .5), .9);
        }
        
        if(BeginSlide())
        {
          
            Presi.LatexCentered(
                @"
Generalized kinematic equation:
$$
\frac{d\boldsymbol{x}}{d\tau} = \boldsymbol{v}(\boldsymbol{x}(\tau), \tau)
\Rightarrow\quad
\boldsymbol{x}(\tau)
= \boldsymbol{x}_0
+ \int_0^\tau \boldsymbol{v}(\boldsymbol{x}(\eta), \eta)\, d\eta
\equiv \boldsymbol{\Phi}^v_\tau(\boldsymbol{x}_0)
$$
(10) $\boldsymbol{v} = \boldsymbol{u}, \tau = t$,\\
(11) $\boldsymbol{v} = \boldsymbol{M}_n, \tau = \xi$, \\
(12) $\boldsymbol{v} = \boldsymbol{M}_n, \tau = t$\\

with ﬁctitious time $\xi$ in (11) relating to physical time $t$ via
\[
\frac{dt}{d\xi} = n\bigl(x^{M}(\xi), \xi\bigr)
 \;\;\Rightarrow\;\;
t = f(\xi) = \int_{0}^{\xi} n\bigl(x^{M}(\eta), \eta\bigr)\, d\eta .
\]
",
                new Vec2(.5, .5), .75);
        }

        if (BeginSlide())
        {
            var world = DrawWorldPanel(new Vec2(.5, .57), new Vec2(1, .5), zoom: .6,
                load: (world) =>
                {
                    var data = world.GetWorldService<DataService>();
                    var axis = world.AddVisualisationService<AxisVisualizer>();
                    axis.DrawTitle = false;
                    axis.DrawAxis = false;
                    axis.DrawGradient = false;
                    data.SetDataset("Double Gyre EPS=0, Pe=100");
                    data.TimeMultiplier = .5f;
                    data.currentSelectedVectorField = "Total Flux";
                    var densityPathStructures = new DensityPathStructures
                    {
                        AccumelationFactor = .2f,
                        InfluenceRadius = .004f,
                        reseedRate = .01f,
                        Decay = .05,
                        ParticleCount = 10000,
                    };
                    world.AddVisualisationService(densityPathStructures);
                });
            
            Presi.LatexCentered(
                @"
In my previous visualizations, I was integrating flux in physical time: 
$\boldsymbol{v} = \boldsymbol{Q}', \tau = t$. \\

\vspace{130px}

This is equivalent to (12): 
$\boldsymbol{v} = \boldsymbol{M}_n, \tau = t$. \\
\\
As $\boldsymbol{M}_n = n\boldsymbol{u} = (\rho/\rho_0)\boldsymbol{u}=(\rho\boldsymbol{u})/\rho_0 = \boldsymbol{M}/\rho_0$, choosing $\rho_0=1$ and $\boldsymbol{M} = \boldsymbol{Q}'$ in our mass analogy gives $\boldsymbol{M}_n = \boldsymbol{Q}'$.
", new Vec2(.5, .5), 1);
        }

        if (BeginSlide())
        {
            Presi.LatexCentered(
                @"
(12) $\boldsymbol{v} = M_n, \tau = t$.\\

``\emph{This correctly delineates the topology of the trajectory in the space-time domain yet in general incorrectly predicts the physical particle position in time due to the non-trivial relation (13) between physical and ﬁctitious time.}''\\


$\boldsymbol{v}$ should be $\boldsymbol{Q}'/T'$ for correct timing.", new Vec2(.5, .5), .5);
        }
        
        if (BeginSlide())
        {
 
            var world = DrawWorldPanel(new Vec2(.5, .37), new Vec2(1, .5), zoom: .7,
                load: (world) =>
                {
                    var data = world.GetWorldService<DataService>();
                    var axis = world.AddVisualisationService<AxisVisualizer>();
                    axis.DrawTitle = false;
                    axis.DrawAxis = false;
                    axis.DrawGradient = true;

                    data.SimulationTime = 4;
                    data.TimeMultiplier = 0;
                    data.SetDataset("Double Gyre EPS=0, Pe=100");
                    data.currentSelectedVectorField = "Total Flux";
                    data.currentSelectedScaler = "Convective Temperature";
                    data.ColorGradient = Gradients.GetGradient("BlueGrayRed");
                    var gridVisualizer = new GridVisualizer();
                    world.AddVisualisationService(gridVisualizer);
                    gridVisualizer.SetGridDiagnostic(new ScalerGridDiagnostic());
                    world.AddVisualisationService(gridVisualizer);
                });
            
            Presi.LatexCentered(
                @"
(10) $\boldsymbol{v} = \boldsymbol{Q}'/T', \tau = t$.\\

Integrating numerically difficult near stagnation regions where $T'(x)$ approaches $0$.", new Vec2(.5, .8), .21);
        }

        if (BeginSlide(""))
        {
            Presi.LatexCentered(@"
(11) $\boldsymbol{v} = \boldsymbol{Q}', \tau = \xi$.\\

Use fictitious time $\xi$ to advance particle $p$ with position $\boldsymbol{x}_p$:
\begin{align*}
\boldsymbol{x}_p &\gets \boldsymbol{x}_p + \boldsymbol{Q}'(\boldsymbol{x}_p, t_p) \Delta\xi_p \\
t_p &\gets t_p + T'(\boldsymbol{x}_p, t_p) \Delta\xi_p
\end{align*}
No division by $T'$. Equivalent particle paths as (10) when $t$ and $\xi$ are coupled via:
\[
t = f(\xi) = \int_{0}^{\xi} n\bigl(x^{M}(\eta), \eta\bigr)\, d\eta .
\]
", new Vec2(.5,.5), .7);
        }
        
        
        if (BeginSlide(""))
        {
            
            var world = DrawWorldPanel(new Vec2(.5, .58), new Vec2(1, .5)/1.7f, zoom: 1,
                load: (world) =>
                {
                    var data = world.GetWorldService<DataService>();
                    var axis = world.AddVisualisationService<AxisVisualizer>();
                    axis.DrawTitle = false;
                    axis.DrawAxis = false;
                    axis.DrawGradient = false;
                    data.SetDataset("(P) Double Gyre EPS=0, Pe=100");
                    data.TimeMultiplier = 0;
                    data.SimulationTime = 0;
                    data.currentSelectedVectorField = "Total Flux";
                    data.currentSelectedScaler = "Convective Temperature";
                    var densityPathStructures = new DensityPathStructuresPresiTarget()
                    {
                        AccumelationFactor = 2f,
                        InfluenceRadius = .004f,
                        reseedRate = 0f,
                        Decay = 100,
                        ParticleCount = 10000,
                        sliceTime = 0,
                    };
                    world.AddVisualisationService(densityPathStructures);
                });
            
            Presi.LatexCentered(@"
Particles no longer have same physical times after $\Delta\xi$ step.
\vspace{170px}



Adaptive $\Delta\xi$ based on $T'$ does not resolve issue near stagnation regions.
", new Vec2(.5,.5), 1);

            bool integrate = false;
            Presi.Slider("Target Physical Time", ref world.World.GetWorldService<DensityPathStructuresPresiTarget>().sliceTime, 0,1, new Vec2(0.5,0.3),.3f);

            Presi.Checkbox("Filter By Time", ref world.World.GetWorldService<DensityPathStructuresPresiTarget>().filterByTime, new Vec2(0.23,0.21));
        }

        if (BeginSlide())
        {
            Presi.LatexCentered(@"
$T'$ at t=0 is a zero field as $T' \equiv T - \tilde{T}$ and $T(\boldsymbol{x},0) = \tilde{T}(\boldsymbol{x},0)$ for all $\boldsymbol{x} \in$ domain.\\

So integrating anything along $Q'$ at $t=0$ never advances in time?\\ Physical meaning?

", new Vec2(.5,.5), .28);
        }

        if (BeginSlide())
        {
            Presi.Image(imageTexture, new Vec2(0.5,0.5), 1f);
        }
    }

    
    private ImageTexture imageTexture = new ImageTexture("Assets/Images/presi/spacetime.png");
}