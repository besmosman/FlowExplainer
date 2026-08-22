namespace FlowExplainer;

public class PullbackPresentation : NewPresentation
{
    public override void Draw()
    {
        if (BeginSlide())
        {
            var view = DrawWorldPanel(new Vec2(.23, .7), new Vec2(1, .5) / 2, zoom: 1, load: w =>
            {
                w.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
                var op = new OperatorSplittedPullbackTestService { IsSimulationSource = true };
                w.AddVisualisationService(op);
                //op.save = BinarySerializer.Load<SimulationSave>("particles.save");
                op.renderPosition = OperatorSplittedPullbackTestService.Position.x;
            });

            var view2 = DrawWorldPanel(new Vec2(.77, .7), new Vec2(1, .5) / 2, zoom: 1, load: w =>
            {
                w.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
                var op = w.AddVisualisationService<OperatorSplittedPullbackTestService>();
                op.IsSimulationSource = false;
                op.Data = view.World.GetWorldService<OperatorSplittedPullbackTestService>().Data;
                op.renderPosition = OperatorSplittedPullbackTestService.Position.x_0;
                op.renderValue = OperatorSplittedPullbackTestService.Value.T_noflow;
            });
            
            var view3 = DrawWorldPanel(new Vec2(.77, .2), new Vec2(1, .5) / 2, zoom: 1, load: w =>
            {
                w.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
                var op = w.AddVisualisationService<OperatorSplittedPullbackTestService>();
                op.IsSimulationSource = false;
                op.Data = view.World.GetWorldService<OperatorSplittedPullbackTestService>().Data;
                op.renderPosition = OperatorSplittedPullbackTestService.Position.x_0;
                op.renderValue = OperatorSplittedPullbackTestService.Value.T_diff;
            });
        }
    }
}