using System.Diagnostics.CodeAnalysis;

namespace FlowExplainer;

public class Dataset
{
    /*public Dictionary<string, IVectorField<Vec3, double>> ScalerFields = new();
    public Dictionary<string, IVectorField<Vec3, Vec2>> VectorFields = new();*/


    public Dictionary<string, object> Vectorfields = new();
    public string Name => Properties["Name"];
    public Action<Dataset> Load;
    public bool Loaded = false;
    public string Path;
    public Dictionary<string, string> Properties;

    public IVectorField<TIn, TOut> GetVectorField<TIn, TOut>(string name) where TIn : IVec<TIn, double>
    {
        return (IVectorField<TIn, TOut>)Vectorfields[name];
    }
    
    public bool TryGetVectorField<TIn, TOut>(string name, [NotNullWhen(true)]out IVectorField<TIn, TOut>? field) where TIn : IVec<TIn, double>
    {
        var r = Vectorfields.TryGetValue(name, out object value);
        field = value as IVectorField<TIn, TOut>;
        return r;
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