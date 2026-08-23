using static FlowExplainer.OperatorSplittedPullbackTestService;

namespace FlowExplainer;

public class PullbackPresentation : NewPresentation
{
    public override void Draw()
    {
        if (BeginSlide())
        {
            var view = DrawWorldPanel(new Vec2(.5, .43), new Vec2(1, .5) * .9, zoom: 1, load: w =>
            {
                w.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
                var op = new OperatorSplittedPullbackTestService { IsSimulationSource = true };
                w.AddVisualisationService(op);
                op.renderPosition = Position.x;
                op.renderValue = Value.T;
            });
            Presi.LatexCentered("$T$", new Vec2(.5, .9), .18f);
        }

        if (BeginSlide())
        {
            var view = DrawWorldPanel(new Vec2(.5, .43), new Vec2(1, .5) * .9, zoom: 1, load: w =>
            {
                w.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
                var op = new OperatorSplittedPullbackTestService { IsSimulationSource = true };
                w.AddVisualisationService(op);
                op.renderPosition = Position.x_0;
                op.renderValue = Value.T_noflow;
            });
            Presi.LatexCentered("$\\widetilde{T}$", new Vec2(.5, .9), .18f);
        }


        if (BeginSlide())
        {
            var view = DrawWorldPanel(new Vec2(.23, .5), new Vec2(1, .5) / 2, zoom: 1, load: w =>
            {
                w.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
                var op = new OperatorSplittedPullbackTestService { IsSimulationSource = true };
                w.AddVisualisationService(op);
                op.renderPosition = Position.x;
                op.renderValue = Value.T;
                op.targetTime = .0;
            });
            Presi.LatexCentered("$\\widetilde{T}$", new Vec2(.75, .8), .18f);

            var view2 = DrawWorldPanel(new Vec2(.77, .5), new Vec2(1, .5) / 2, zoom: 1, load: w =>
            {
                w.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
                var op = new OperatorSplittedPullbackTestService { IsSimulationSource = false };
                op.Data = view.World.GetWorldService<OperatorSplittedPullbackTestService>().Data;
                w.AddVisualisationService(op);
                op.renderPosition = Position.x_0;
                op.renderValue = Value.T_noflow;
            });
            Presi.LatexCentered("$T$", new Vec2(.25, .8), .18f);

            if (BeginStep())
            {
                view.World.GetWorldService<OperatorSplittedPullbackTestService>().targetTime = .8;
            }
        }

        if (BeginSlide())
        {
            var defPos = new Vec2(.5, .86);
            var view = DrawWorldPanel(new Vec2(.23, .5), new Vec2(1, .5) / 2, zoom: 1, load: w =>
            {
                w.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
                var op = new OperatorSplittedPullbackTestService { IsSimulationSource = true };
                w.AddVisualisationService(op);
                op.renderPosition = Position.x_0;
                op.renderValue = Value.T_diff;
                op.targetTime = 0;
            }, "#v1");

            Presi.LatexCentered(@"$$T'(\mathbf{x}_0,t) = T\left(\Phi_t(\mathbf{x}_0),t\right) - \widetilde{T}(\mathbf{x}_0,t)$$", defPos, .18f);

            DrawWorldPanel(new Vec2(.77, .5), new Vec2(1, .5) / 2, zoom: 1, load: w =>
            {
                w.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
                var op = new OperatorSplittedPullbackTestService { IsSimulationSource = false };
                op.Data = view.World.GetWorldService<OperatorSplittedPullbackTestService>().Data;
                w.AddVisualisationService(op);
                op.renderPosition = Position.x;
                op.renderValue = Value.T_diff;
            }, "#v2");

            Presi.LatexCentered("$T'(\\mathbf{x}_0,t)$", new Vec2(.23, .24), .1f);
            Presi.LatexCentered("$T'(\\mathbf{x},t)$", new Vec2(.77, .24), .1f);

            if (BeginStep())
            {
                view.World.GetWorldService<OperatorSplittedPullbackTestService>().targetTime = .8;
            }

            if (BeginStep())
            {
                view.World.GetWorldService<OperatorSplittedPullbackTestService>().targetTime = null;
            }
        }

        if (BeginSlide())
        {
            var view = DrawWorldPanel(new Vec2(.23, .8), new Vec2(1, .5) / 2, zoom: 1, load: w =>
            {
                w.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
                var op = new OperatorSplittedPullbackTestService { IsSimulationSource = true };
                w.AddVisualisationService(op);
                op.renderPosition = Position.x_0;
                op.renderValue = Value.T_diff;
                op.targetTime = 0;
            }, "#v1");

            // Presi.LatexCentered(@"$$T'(\mathbf{x}_0,t) = T\left(\Phi_t(\mathbf{x}_0),t\right) - \widetilde{T}(\mathbf{x}_0,t)$$", defPos, .18f);

            DrawWorldPanel(new Vec2(.77, .8), new Vec2(1, .5) / 2, zoom: 1, load: w =>
            {
                w.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
                var op = new OperatorSplittedPullbackTestService { IsSimulationSource = false };
                op.Data = view.World.GetWorldService<OperatorSplittedPullbackTestService>().Data;
                w.AddVisualisationService(op);
                op.renderPosition = Position.x;
                op.renderValue = Value.T_diff;
            }, "#v2");

            Presi.LatexCentered("$T'(\\mathbf{x}_0,t)$", new Vec2(.23, .54), .1f);
            Presi.LatexCentered("$T'(\\mathbf{x},t)$", new Vec2(.77, .54), .1f);

            DrawWorldPanel(new Vec2(.5, .26), new Vec2(1, .5) / 2, zoom: 1, load: w =>
            {
                w.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
                var po = w.AddVisualisationService<PoincareVisualizer>();
                po.Periods = 500;
                po.Generate();
            }, "#vp");
            Presi.Text("Poincare map", new Vec2(.5, .0), .02, true, Color.White);
        }

        
        if (BeginSlide())
        {
            var view = DrawWorldPanel(new Vec2(.23, .8), new Vec2(1, .5) / 2, zoom: 1, load: w =>
            {
                w.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
                var op = new OperatorSplittedPullbackTestService { IsSimulationSource = true };
                w.AddVisualisationService(op);
                op.renderPosition = Position.x_0;
                op.renderValue = Value.T_diff_avg;
            });

            // Presi.LatexCentered(@"$$T'(\mathbf{x}_0,t) = T\left(\Phi_t(\mathbf{x}_0),t\right) - \widetilde{T}(\mathbf{x}_0,t)$$", defPos, .18f);

            DrawWorldPanel(new Vec2(.77, .8), new Vec2(1, .5) / 2, zoom: 1, load: w =>
            {
                w.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
                var op = new OperatorSplittedPullbackTestService { IsSimulationSource = false };
                op.Data = view.World.GetWorldService<OperatorSplittedPullbackTestService>().Data;
                w.AddVisualisationService(op);
                op.renderPosition = Position.x;
                op.renderValue = Value.T_diff_avg;
            });

            Presi.LatexCentered("$T'(\\mathbf{x}_0,t)$", new Vec2(.23, .54), .1f);
            Presi.LatexCentered("$T'(\\mathbf{x},t)$", new Vec2(.77, .54), .1f);

            DrawWorldPanel(new Vec2(.5, .26), new Vec2(1, .5) / 2, zoom: 1, load: w =>
            {
                w.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
                var po = w.AddVisualisationService<PoincareVisualizer>();
                po.Periods = 500;
                po.Generate();
            },"$vp");
            Presi.Text("Poincare map", new Vec2(.5, .0), .02, true, Color.White);
        }


        if (BeginSlide())
        {
            var view = DrawWorldPanel(new Vec2(.23, .5), new Vec2(1, .5) / 2, zoom: 1, load: w =>
            {
                w.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
                var op = new OperatorSplittedPullbackTestService { IsSimulationSource = true };
                w.AddVisualisationService(op);
                op.renderPosition = Position.x_0;
                op.renderValue = Value.T_diff_avg;
                op.targetTime = 1;
            });
            Presi.LatexCentered(@"$$T'(\mathbf{x}_0,t) = T\left(\Phi_t(\mathbf{x}_0),t\right) - \widetilde{T}(\mathbf{x}_0,t)$$", new Vec2(.5, .86), .18f);

            var view2 = DrawWorldPanel(new Vec2(.77, .5), new Vec2(1, .5) / 2, zoom: 1, load: w =>
            {
                w.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
                var op = new OperatorSplittedPullbackTestService { IsSimulationSource = false };
                op.Data = view.World.GetWorldService<OperatorSplittedPullbackTestService>().Data;
                w.AddVisualisationService(op);
                op.renderPosition = Position.x;
                op.renderValue = Value.T_diff_avg;
            });
            Presi.LatexCentered("$T'(\\mathbf{x}_0,t)$", new Vec2(.23, .24), .1f);
            Presi.LatexCentered("$T'(\\mathbf{x},t)$", new Vec2(.77, .24), .1f);

            if (BeginStep())
            {
                view.World.GetWorldService<OperatorSplittedPullbackTestService>().targetTime = null;
            }
        }

        if (BeginSlide())
        {
            var view = DrawWorldPanel(new Vec2(.5, .43), new Vec2(1, .5) * .9, zoom: 1, load: w =>
            {
                w.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
                var op = new OperatorSplittedPullbackTestService { IsSimulationSource = true };
                w.AddVisualisationService(op);
                op.renderPosition = Position.x_0;
                op.renderValue = Value.T_noflow;
            });
            Presi.LatexCentered("$T$", new Vec2(.5, .9), .18f);
        }

        if (BeginSlide())
        {
            var view = DrawWorldPanel(new Vec2(.23, .7), new Vec2(1, .5) / 2, zoom: 1, load: w =>
            {
                w.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
                var op = new OperatorSplittedPullbackTestService { IsSimulationSource = true };
                w.AddVisualisationService(op);
                //op.save = BinarySerializer.Load<SimulationSave>("particles.save");
                op.renderPosition = Position.x;
            });

            var view2 = DrawWorldPanel(new Vec2(.77, .7), new Vec2(1, .5) / 2, zoom: 1, load: w =>
            {
                w.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
                var op = w.AddVisualisationService<OperatorSplittedPullbackTestService>();
                op.IsSimulationSource = false;
                op.Data = view.World.GetWorldService<OperatorSplittedPullbackTestService>().Data;
                op.renderPosition = Position.x_0;
                op.renderValue = Value.T_noflow;
            });

            var view3 = DrawWorldPanel(new Vec2(.77, .2), new Vec2(1, .5) / 2, zoom: 1, load: w =>
            {
                w.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
                var op = w.AddVisualisationService<OperatorSplittedPullbackTestService>();
                op.IsSimulationSource = false;
                op.Data = view.World.GetWorldService<OperatorSplittedPullbackTestService>().Data;
                op.renderPosition = Position.x_0;
                op.renderValue = Value.T_diff;
            });
        }
    }
}