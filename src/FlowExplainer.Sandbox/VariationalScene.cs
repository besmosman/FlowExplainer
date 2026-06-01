namespace FlowExplainer;

public class VariationalScene : Scene
{
    public override void Load(FlowExplainer flowExplainer)
    {
        var world = flowExplainer.GetGlobalService<WorldManagerService>().Worlds[0];
        
        var dataset1 = new Dataset(new()
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
                    w = 2*double.Pi/10,
                    epsilon = 0.1,
                });
            });

        var dataset2 = new Dataset(new()
            {
                {
                    "Name", "Test"
                },
            },
            (d) =>
            {
                d.VectorFields.Add("Velocity", new ArbitraryField<Vec3, Vec2>(
                    new RectDomain<Vec3>(new Rect<Vec3>(new Vec3(-1,-1,0),new Vec3(1,1,10))),
                    x => new Vec2(x.X, -x.Y)));
            });
        var dataset = dataset1;
        
        flowExplainer.GetGlobalService<DatasetsService>().Datasets.Add(dataset.Name, dataset);
        world.DataService.SetDataset(dataset.Name);
        world.DataService.currentSelectedVectorField = "Velocity";
        world.DataService.currentSelectedScaler = "Convective Temperature";
        world.DataService.SimulationTime = 0f;
        world.AddVisualisationService<AxisVisualizer>();
        var scaler = world.AddVisualisationService<GridVisualizer>();
        scaler.TargetCellCount = 1024 * 512 / 1000;
        var variational = world.AddVisualisationService<VariationalLCS>();
     
       // world.DataService.SetDataset("Double Gyre EPS=0.1, Pe=100");
       // variational.VelocityField = world.DataService.Artifacts.Get<IVectorField<Vec3, Vec2>>("Convection Flux");
       // variational.t0 = 3;
       // variational.T = 3;
        variational.Recompute();
        var l2 = variational.Artifacts.Get<IVectorField<Vec2, double>>("Lambda 2");
        scaler.SetGridDiagnostic(new Scaler2DGridDiagnostic()
        {
            ScalerField = new Artifact<IVectorField<Vec2, double>>(l2.Value.Select(s => double.Log(s)), "ln l2", ""),
            // ScalerField = variational.GetSelectableVec2Vec1().ElementAt(2).VectorField
        });
        world.AddVisualisationService(new VariationalPresentation.IntegratorService()
        {
            VectorField = variational.Artifacts.Get<IVectorField<Vec2,Vec2>>("Scaled Eigen Vector 2 Perp").Value,
             ValidSubspace= variational.Artifacts.Get<IVectorField<Vec2,double>>("Valid Subspace").Value
        });

        /*var arrow1 = world.AddVisualisationService<ArrowVisualizer>();
        arrow1.AltVectorfield = variational.GetSelectableVec3Vec2().ElementAt(1).VectorField;*/
    }
}