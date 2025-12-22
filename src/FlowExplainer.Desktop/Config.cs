using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class Config
{
    private static Dictionary<string, JValue> entries;
    public static bool IsDirty { get; set; }

    public static void UpdateValue<T>(string name, T val)
    {
        var jval = val switch
        {
            string s => new JValue(s),
            int i => new JValue(i),
            bool b => new JValue(b),
            double f => new JValue(f),
            _ => throw new ArgumentException()
        };
        entries[name] = jval;
        MarkDirty();
    }

    private static void MarkDirty()
    {
        IsDirty = true;
    }

    public static void Save()
    {
        File.WriteAllText("config.json", JsonConvert.SerializeObject(entries, Formatting.Indented));
    }

    public static T? GetValue<T>(string path)
    {
        if (!entries.TryGetValue(path, out var value))
            return default;
            
        return value.Value<T>();
    }

    public static void Load(Dictionary<string, JValue> dict)
    {
        entries = dict; 
    }

    public static void Load(string path)
    {
        entries = JsonConvert.DeserializeObject<Dictionary<string, JValue>>(File.ReadAllText(path)) ??
                  throw new ArgumentException();
    }
}