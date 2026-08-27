namespace FlowExplainer;

public static class DatasetLoaders
{
    private static Dictionary<string, Action<Dataset>> Loaders = new();
    private static Dictionary<Action<Dataset>, string> LoadersLookup = new();

    static DatasetLoaders()
    {
        RegisterLoader("Default", DefaultDatasetLoader);
        RegisterLoader("PipeFlow", PipeFlowDatasetLoader);
    }
    
    public static void RegisterLoader(string name, Action<Dataset> loader)
    {
        Loaders.Add(name, loader);
        LoadersLookup.Add(loader, name);
    }

    public static Action<Dataset> GetLoader(string name)
    {
        return Loaders[name];
    }

    public static void DefaultDatasetLoader(Dataset dataset)
    {
        LoadSerializedFields(dataset);
    }

    private static void LoadSerializedFields(Dataset dataset)
    {
        IDomain<Vec3> lastDomain3d = null;
        foreach (var f in Directory.EnumerateFiles(dataset.Path))
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
    }

    public static void PipeFlowDatasetLoader(Dataset dataset)
    {
        LoadSerializedFields(dataset);
        var pipeFlow = new PoiseuilleFlowPipe(dataset.GetVectorField<Vec3, double>("Temperature").Domain);
        dataset.Vectorfields.Add("Velocity", pipeFlow);
    }
}