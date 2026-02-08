using System.Reflection;

namespace FlowExplainer;

public static class ServicesInfo
{
    public static Dictionary<string, List<WorldService>> ServicesByCategory = new();
    public static List<WorldService> ServicesOrderedByName = new();
    public static List<Assembly> RegisteredAssemblies = new();

    static ServicesInfo()
    {
    }

    public static void Init()
    {
        ServicesByCategory.Add("General", new()); //force first pos
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.FullName?.StartsWith("System.") != true)
                RegisterAssembly(assembly);
        }
    }

    private static void RegisterAssembly(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes().Where(t => t.IsAssignableTo(typeof(WorldService)) && !t.IsAbstract))
        {
            var instance = (WorldService)Activator.CreateInstance(type)!;
            if (instance.CategoryName != null)
            {
                var name = instance.Name ?? "?";
                ServicesByCategory.TryAdd(instance.CategoryName, new());
                ServicesByCategory[instance.CategoryName].Add(instance);
                ServicesOrderedByName.Add(instance);
            }
        }

        ServicesOrderedByName = ServicesOrderedByName.OrderBy(o => o.Name ?? "?").ToList();
        RegisteredAssemblies.Add(assembly);
    }
}