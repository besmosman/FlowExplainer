namespace FlowExplainer;

public class Dataset
{
    public Dictionary<string, IVectorField<Vec3, double>> ScalerFields = new();
    public Dictionary<string, IVectorField<Vec3, Vec2>> VectorFields = new();

    public string Name;
    public Action<Dataset> Load;
    public bool Loaded = false;

    public Dataset(string name, Action<Dataset> load)
    {
        Name = name;
        Load = load;
    }
}