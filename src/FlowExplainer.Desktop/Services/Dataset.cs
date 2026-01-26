namespace FlowExplainer;

public class Dataset
{
    public Dictionary<string, IVectorField<Vec3, double>> ScalerFields = new();
    public Dictionary<string, IVectorField<Vec3, Vec2>> VectorFields = new();

    public string Name => Properties["Name"];
    public Action<Dataset> Load;
    public bool Loaded = false;

    public Dictionary<string, string> Properties;

    public Dataset(Dictionary<string, string> properties, Action<Dataset> load)
    {
        Properties = properties;
        Load = load;
    }

    
    public Dataset Clone()
    {
        return new Dataset(new Dictionary<string, string>(Properties), Load);
    }
}