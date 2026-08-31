using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace FlowExplainer;

public class ArtifactsManager : IEnumerable<IArtifact>
{
    private Dictionary<Type, Dictionary<string, IArtifact>> artifacts = new();

    public void RegisterOrUpdate(IArtifact artifact)
    {
        var type = artifact.ValueType;
        artifact.ID = artifact.ID.ToUpperInvariant();
        if (!artifacts.ContainsKey(type))
            artifacts.Add(type, new());
        var dictionary = artifacts[type];
        if (dictionary.TryGetValue(artifact.ID, out var value))
        {
            value.ValueObj = artifact.ValueObj;
        }
        else
        {
            dictionary.Add(artifact.ID, artifact);
        }
    }

    public void Clear()
    {
        artifacts.Clear();
    }

    public Artifact<T> Get<T>(string id)
    {
        return (Artifact<T>)(artifacts[typeof(T)][id.ToUpperInvariant()]);
    }

    public bool TryGet<T>(string id, [NotNullWhen(true)]out Artifact<T>? artifact)
    {
        if (artifacts.TryGetValue(typeof(T), out var list))
            if (list.TryGetValue(id.ToUpperInvariant(), out var value))
            {
                artifact = (Artifact<T>)value;
                return true;
            }   
        artifact = null;
        return false;
    }

    public IEnumerable<Artifact<T>> GetAll<T>()
    {
        if (!artifacts.TryGetValue(typeof(T), out var all))
            return [];
        return all.Values.Cast<Artifact<T>>();
    }

    public IEnumerable<IArtifact> GetAll(Type t)
    {
        if (!artifacts.TryGetValue(t, out var all))
            return [];
        return all.Values;
    }

    public IEnumerable<IArtifact> GetAll()
    {
        return artifacts.SelectMany(s => s.Value.Values);
    }

    public IEnumerator<IArtifact> GetEnumerator()
    {
        foreach (var artifact in artifacts)
        foreach (var a in artifact.Value)
            yield return a.Value;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}