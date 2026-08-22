namespace FlowExplainer;

public class Dataset
{
    /*public Dictionary<string, IVectorField<Vec3, double>> ScalerFields = new();
    public Dictionary<string, IVectorField<Vec3, Vec2>> VectorFields = new();*/


    public Dictionary<string, object> Vectorfields = new();
    public string Name => Properties["Name"];
    public Action<Dataset> Load;
    public bool Loaded = false;
    
    public Dictionary<string, string> Properties;

    public IVectorField<TIn, TOut> GetVectorField<TIn, TOut>(string name) where TIn : IVec<TIn, double>
    {
        return (IVectorField<TIn, TOut>)Vectorfields[name];
    }

    public IEnumerable<(string name, IVectorField<TIn, TOut> vectorField)> GetAllVectorFields<TIn, TOut>() where TIn : IVec<TIn, double>
    {
        foreach (var vectorfield in Vectorfields)
        {
            if (vectorfield.Value is IVectorField<TIn, TOut> vec)
            {
                yield return (vectorfield.Key, vec);
            }
        }
    }
    
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