namespace FlowExplainer
{
    public class DatasetsService : GlobalService
    {
        public Dictionary<string, Dataset> Datasets = new();

        public override void Initialize()
        {
            var eps01 = new Dataset("Double Gyre eps=0.1", dataset =>
            {
                string fieldsFolder = "speetjens-computed-fields";
                var DiffFluxField = RegularGridVectorField<Vec3, Vec3i, Vec2>.Load(Path.Combine(fieldsFolder, "diffFlux.field"));
                var ConvFluxField = RegularGridVectorField<Vec3, Vec3i, Vec2>.Load(Path.Combine(fieldsFolder, "convectiveHeatFlux.field"));
                var TempConvection = RegularGridVectorField<Vec3, Vec3i, double>.Load(Path.Combine(fieldsFolder, "tempConvection.field"));
                var TempTot = RegularGridVectorField<Vec3, Vec3i, double>.Load(Path.Combine(fieldsFolder, "tempTot.field"));
                var TempTotNoFlow = RegularGridVectorField<Vec3, Vec3i, double>.Load(Path.Combine(fieldsFolder, "tempNoFlow.field"));
                var totalFlux = new ArbitraryField<Vec3, Vec2>(DiffFluxField.Domain, p => DiffFluxField.Evaluate(p) + ConvFluxField.Evaluate(p));
                var velocityField = new SpeetjensVelocityField()
                {
                    epsilon = .1f,
                };
                dataset.VectorFields.Clear();
                dataset.ScalerFields.Clear();
                dataset.VectorFields.Add("Velocity", velocityField);
                dataset.VectorFields.Add("Diffusion Flux", DiffFluxField);
                dataset.VectorFields.Add("Convection Flux", ConvFluxField);
                dataset.VectorFields.Add("Total Flux", totalFlux);
                dataset.ScalerFields.Add("Total Temperature", TempTot);
                dataset.ScalerFields.Add("Convective Temperature", TempConvection);
                dataset.ScalerFields.Add("No Flow Temperature", TempTotNoFlow);
            });
            Datasets.Add(eps01.Name, eps01);
        }

        public override void Draw()
        {
        }
    }

    public class WorldManagerService : GlobalService
    {
        public List<World> Worlds = new();

        public override void Draw()
        {
            foreach (var world in Worlds)
            {
                if (world.IsViewed)
                    world.Update();

                world.IsViewed = false;
            }
            /*            foreach (var v in Visualisation)
                            v.Draw();*/
        }

        public World NewWorld(bool skipInit = false)
        {
            if (FlowExplainer == null)
                throw new Exception();

            World v = new(FlowExplainer);
            v.AddVisualisationService(new DataService()
            {
                IsEnabled = true,
            });
            v.AddVisualisationService(new Axis3D()
            {
                IsEnabled = true,
            });
            //v.AddVisualisationService(new HeatSimTest(){ IsEnabled = true});
            v.AddVisualisationService(new HeatSimulationViewData());
            v.AddVisualisationService(new HeatSimulationVisualizer());
            v.AddVisualisationService(new GridVisualizer());
            v.AddVisualisationService(new Poincare3DVisualizer());
            v.AddVisualisationService(new ParticleLagrangianTest());
            v.AddVisualisationService(new FlowDirectionVisualization());
            v.AddVisualisationService(new HeatSimulation3DVisualizer());
            v.AddVisualisationService(new HeatSimulationService());
            v.AddVisualisationService(new StochasticPoincare());
            v.AddVisualisationService(new CriticalPointIdentifier());
            v.AddVisualisationService(new HeatSimulationReplayer());
            v.AddVisualisationService(new FlowFieldVisualizer());
            v.AddVisualisationService(new PoincareVisualizer());
            v.AddVisualisationService(new AxisVisualizer()
            {
                IsEnabled = true,
            });
            v.AddVisualisationService(new StructureIdentifier());
            //v.AddVisualisationService(new FDTest(){ IsEnabled = true});
            v.AddVisualisationService(new FlowVisService());
            //v.AddVisualisationService(new FDTest());
            //v.AddVisualisationService(new Heat3DViewer());
            Worlds.Add(v);
            return v;
        }

        public override void Initialize()
        {
        }
    }
}