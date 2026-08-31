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


    public struct PipeMaterial
    {
        public double StartR;
        public string Name;
        public double Density; //kg/m3
        public double HeatCapacity; //J/kgK
        public double ThermalConductivity; //W/mK
    }

    public static void PipeFlowDatasetLoader(Dataset dataset)
    {
        LoadSerializedFields(dataset);
        var pipeFlow = new PoiseuilleFlowPipe(dataset.GetVectorField<Vec3, double>("Temperature").Domain);
        dataset.Vectorfields.Add("Velocity", pipeFlow);

        var rw = 27e-3;
        var rs = 30.15e-3;
        var ri = 59.5e-3;
        var rc = 62.5e-3;
        var rg = 1.5 * rc;
        var L = 20 * rw;
        var U_mean = 1e-2;

        PipeMaterial[] materials =
        [
            new()
            {
                StartR = 0,
                Name = "Water",
                Density = 996.7,
                HeatCapacity = 4066.7,
                ThermalConductivity = 0.605,
            },
            new()
            {
                StartR = rw,
                Name = "Steel",
                ThermalConductivity = 51,
                HeatCapacity = 502.5,
                Density = 7900,
            },
            new()
            {
                StartR = rs,
                Name = "Insulation",
                ThermalConductivity = 0.027,
                HeatCapacity = 1400,
                Density = 30,
            },
            new()
            {
                StartR = ri,
                Name = "Casing",
                ThermalConductivity = 0.43,
                Density = 944,
                HeatCapacity = 2250,
            },
            new()
            {
                StartR = rc,
                Name = "Ground",
                ThermalConductivity = 1.6,
                Density = 1500,
                HeatCapacity = 1200,
            }
        ];

        var PeField = new ArbitraryField<Vec3, double>(pipeFlow.Domain, x =>
        {
            var r = x.Y;
            for (int i = materials.Length - 1; i >= 0; i--)
            {
                var material = materials[i];
                if (material.StartR <= r)
                {
                    var alpha = material.ThermalConductivity / (material.Density * material.HeatCapacity);
                    var U = U_mean;
                    var Pe = U * L / alpha;
                    return Pe;
                }
            }

            return 0;
            //throw new ArgumentException();
        });
        dataset.Vectorfields.Add("Pe", PeField);
    }
}