using System.Text;

namespace FlowExplainer;

public class SceneManager : GlobalService
{
    private Dictionary<string, Scene> scenesByKeys = null!;

    public override void Initialize()
    {
        scenesByKeys = new Dictionary<string, Scene>();
        foreach (var sceneType in ServicesInfo.RegisteredAssemblies
                     .SelectMany(s => s.GetTypes())
                     .Where(t => t.IsAssignableTo(typeof(Scene)) && !t.IsAbstract))
        {
            var instance = Activator.CreateInstance(sceneType) as Scene;
            scenesByKeys.Add(AddDashesToSentence(instance.Name, true), instance);
        }
    }

    public override void Draw()
    {
    }

    [Command]
    public void scenes()
    {
        foreach (var scenesByKey in scenesByKeys)
        {
            Logger.LogDebug(scenesByKey.Key);
        }
    }

    [Command]
    public void scene(string name)
    {
        bool found = scenesByKeys.TryGetValue(name, out var value);
        if (!found)
        {
            var f = scenesByKeys.Keys.SingleOrDefault(k => k.StartsWith(name));
            if (f != null)
            {
                value = scenesByKeys[f];
                found = true;
            }
        }

        if (found)
        {
            LoadScene(value);
            Logger.LogDebug($"Loaded {value.Name}");
        }
        else
        {
            Logger.LogDebug("Unkown scene");
        }
    }

    public void LoadScene(Scene value)
    {
        FlowExplainer.GetGlobalService<WorldManagerService>().Initialize();
        FlowExplainer.GetGlobalService<ViewsService>().Initialize();
        value.Load(FlowExplainer);
    }

    //https://stackoverflow.com/questions/272633/add-spaces-before-capital-letters
    string AddDashesToSentence(string text, bool preserveAcronyms)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;
        StringBuilder newText = new StringBuilder(text.Length * 2);
        newText.Append(text[0]);
        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]))
                if ((text[i - 1] != '-' && !char.IsUpper(text[i - 1])) ||
                    (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                     i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                    newText.Append('-');
            newText.Append(text[i]);
        }

        return newText.ToString().ToLowerInvariant();
    }
}