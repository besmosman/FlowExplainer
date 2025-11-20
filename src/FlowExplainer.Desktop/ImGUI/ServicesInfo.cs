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
        RegisterAssembly(typeof(Poincare3DVisualizer).Assembly);
    }

    private static void RegisterAssembly(Assembly assembly)
    {
        ServicesByCategory.Add("General", new());//force first pos
        foreach (var type in assembly.GetTypes().Where(t => t.IsAssignableTo(typeof(WorldService)) && !t.IsAbstract))
        {
            var instance = (WorldService)Activator.CreateInstance(type)!;
            if (instance.CategoryN != null)
            {
                var name = instance.Name ?? "?";
                ServicesByCategory.TryAdd(instance.CategoryN, new());
                ServicesByCategory[instance.CategoryN].Add(instance);
                ServicesOrderedByName.Add(instance);
            }
        }
        ServicesOrderedByName = ServicesOrderedByName.OrderBy(o => o.Name ?? "?").ToList();
        RegisteredAssemblies.Add(assembly);
    }
}