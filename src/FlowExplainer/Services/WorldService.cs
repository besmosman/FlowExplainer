using FlowExplainer;
using FlowExplainer;

namespace FlowExplainer;

/// <summary>
/// Service running in a <see cref="Services.Visualisation"/> instance. Zero or one per <see cref="World"/> instance.
/// </summary>
public abstract class WorldService : Service
{
    public virtual ToolCategory Category => ToolCategory.Simulation;
    public World World { get; internal set; } = null!;
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets called every frame before executing rendertasks.
    /// </summary>
    public abstract void Draw(RenderTexture rendertarget, View view);

    public T? GetWorldService<T>() where T : WorldService =>
        World.GetWorldService<T>();

    public T GetRequiredWorldService<T>() where T : WorldService => GetWorldService<T>() ?? throw new Exception($"No instance of {typeof(T)} found in the visualisation");
    
    public virtual void DrawImGuiEdit()
    {
    }
}