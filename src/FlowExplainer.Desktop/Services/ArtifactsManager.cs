namespace FlowExplainer;

public class ArtifactsManager
{
    private Dictionary<Type, List<IArtifact>> artifacts = new();

    public void Register<T>(Artifact<T> artifact)
    {
        if (!artifacts.ContainsKey(typeof(T)))
            artifacts.Add(typeof(T), new());
        artifacts[typeof(T)].Add(artifact);
    }

    public void Clear()
    {
        artifacts.Clear();
    }

    public Artifact<T> Get<T>(string name)
    {
        return (Artifact<T>)artifacts[typeof(T)].Find(p => p.DisplayName == name)!;
    }

    public List<IArtifact> GetAll(Type t)
    {
        if (!artifacts.TryGetValue(t, out var all))
            return [];
            return all;
    }
}