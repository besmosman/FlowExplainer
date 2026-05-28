namespace FlowExplainer;

public class ArtifactsManager
{
    private Dictionary<Type, Dictionary<string, IArtifact>> artifacts = new();

    public void RegisterOrUpdate<T>(Artifact<T> artifact)
    {
        if (!artifacts.ContainsKey(typeof(T)))
            artifacts.Add(typeof(T), new());
        var dictionary = artifacts[typeof(T)];
        if (dictionary.TryGetValue(artifact.DisplayName, out var value))
        {
            value.ValueObj = artifact.ValueObj;
        }
        else
        {
            dictionary.Add(artifact.DisplayName, artifact);
        }
    }

    public void Clear()
    {
        artifacts.Clear();
    }

    public Artifact<T> Get<T>(string name)
    {
        return (Artifact<T>)(artifacts[typeof(T)][name]);
    }

    public IEnumerable<IArtifact> GetAll(Type t)
    {
        if (!artifacts.TryGetValue(t, out var all))
            return [];
        return all.Values;
    }
}