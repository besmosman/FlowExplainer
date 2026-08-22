using Newtonsoft.Json;

namespace FlowExplainer
{
    public static class DatasetAnalyticalFields
    {
        public static Dictionary<Type, Dictionary<string, Type>> AnalyticalFieldsByType = new();

        static DatasetAnalyticalFields()
        {
            RegisterAnalyticalField(typeof(SpeetjensVelocityField));
            RegisterAnalyticalField(typeof(PipeFlow));
        }

        public static void RegisterAnalyticalField(Type type)
        {
            var vectorfieldType = type.GetInterfaces()
                .FirstOrDefault(f => f.IsGenericType
                                     && f.GetGenericTypeDefinition() == typeof(IVectorField<,>));

            if (!AnalyticalFieldsByType.TryGetValue(vectorfieldType, out var types))
            {
                types = new();
                AnalyticalFieldsByType.Add(vectorfieldType, types);
            }

            types.Add(type.Name, type);
        }

        public static IVectorField<TIn, TOut> BuildFieldFromSave<TIn, TOut>(AnalyticalVectorFieldSave save, IDomain<TIn> refDomain)
            where TIn : IVec<TIn, double>
        {
            var f = AnalyticalFieldsByType[typeof(IVectorField<TIn, TOut>)][save.TypeName];
            IVectorField<TIn, TOut> inst = null!;
            var domainConstructor = f.GetConstructor([typeof(IDomain<TIn>)]);
            if (domainConstructor != null)
            {
                inst = (IVectorField<TIn, TOut>)Activator.CreateInstance(f, [refDomain])!;
            }
            else inst = (IVectorField<TIn, TOut>)Activator.CreateInstance(f)!;

            if (save.Arguments != null)
                foreach (var arg in save.Arguments)
                {
                    var valueString = arg.Item2;
                    var fieldInfo = f.GetField(arg.Item1);
                    if (fieldInfo.FieldType == typeof(int))
                        fieldInfo.SetValue(inst, int.Parse(arg.Item2));
                    else if (fieldInfo.FieldType == typeof(double))
                        fieldInfo.SetValue(inst, double.Parse(arg.Item2));
                    else if (fieldInfo.FieldType == typeof(IDomain<TIn>))
                        fieldInfo.SetValue(inst, refDomain);
                    else throw new NotImplementedException();
                }

            return inst;
        }
    }

    public class DatasetsService : GlobalService
    {
        public Dictionary<string, Dataset> Datasets = new();

        public override void Initialize()
        {
            if (Directory.Exists("Datasets"))
                foreach (var fieldsFolder in Directory.GetDirectories("Datasets"))
                {
                    var props = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path.Combine(fieldsFolder, "properties.json")));
                    if (props != null)
                    {
                        props.TryAdd("Name", "?");
                        var data = new Dataset(props, dataset =>
                        {
                            IDomain<Vec3> lastDomain3d = null;
                            foreach (var f in Directory.EnumerateFiles(fieldsFolder))
                            {
                                if (f.EndsWith(".vec3_vec2_field"))
                                {
                                    var field = RegularGridVectorField<Vec3, Vec3i, Vec2>.Load(f);
                                    dataset.Vectorfields.Add(field.DisplayName, field);
                                    lastDomain3d = field.Domain;
                                }

                                if (f.EndsWith(".vec3_vec1_field"))
                                {
                                    var field = RegularGridVectorField<Vec3, Vec3i, Double>.Load(f);
                                    dataset.Vectorfields.Add(field.DisplayName, field);
                                    lastDomain3d = field.Domain;
                                }

                                if (f.EndsWith(".vec3_vec2_field_analytical"))
                                {
                                    var save = BinarySerializer.Load<AnalyticalVectorFieldSave>(f);
                                    var field = DatasetAnalyticalFields.BuildFieldFromSave<Vec3, Vec2>(save, lastDomain3d);
                                    dataset.Vectorfields.Add(save.DisplayName, field);
                                }

                                if (f.EndsWith(".vec3_vec1_field_analytical"))
                                {
                                    var save = BinarySerializer.Load<AnalyticalVectorFieldSave>(f);
                                    var field = DatasetAnalyticalFields.BuildFieldFromSave<Vec3, double>(save, lastDomain3d);
                                    dataset.Vectorfields.Add(field.DisplayName, field);
                                }
                            }
                        });

                        Datasets.Add(data.Name, data);
                    }
                }
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
            /*v.AddVisualisationService(new Axis3D()
            {
                IsEnabled = true,
            });*/
            //v.AddVisualisationService(new HeatSimTest(){ IsEnabled = true});
            /*v.AddVisualisationService(new HeatSimulationViewData());
            v.AddVisualisationService(new HeatSimulationVisualizer());
            v.AddVisualisationService(new GridVisualizer());
            v.AddVisualisationService(new Poincare3DVisualizer());
            v.AddVisualisationService(new ParticleLagrangianTest());
            v.AddVisualisationService(new FlowDirectionVisualization());
            v.AddVisualisationService(new HeatSimulation3DVisualizer());
            v.AddVisualisationService(new HeatSimulationService());
            v.AddVisualisationService(new StochasticVisualization());
            v.AddVisualisationService(new CriticalPointIdentifier());
            v.AddVisualisationService(new HeatSimulationReplayer());
            v.AddVisualisationService(new FlowArrowVisualizer());
            v.AddVisualisationService(new PoincareVisualizer());
            v.AddVisualisationService(new AxisVisualizer()
            {
                IsEnabled = true,
            });
            v.AddVisualisationService(new StructureIdentifier());
            //v.AddVisualisationService(new FDTest(){ IsEnabled = true});
            v.AddVisualisationService(new FlowVisService());*/
            //v.AddVisualisationService(new FDTest());
            //v.AddVisualisationService(new Heat3DViewer());
            Worlds.Add(v);
            return v;
        }

        public override void Initialize()
        {
            Worlds.Clear();
            NewWorld();
        }
    }
}