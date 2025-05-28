using FlowExplainer;
using FlowExplainer;

namespace FlowExplainer;

/// <summary>
/// Service running in a <see cref="Services.Visualisation"/> instance. Zero or one per <see cref="Services.Visualisation"/> instance.
/// </summary>
public abstract class VisualisationService : Service
{
    public virtual ToolCategory Category => ToolCategory.None;
    public virtual bool IsUniqueRenderOption => false;
    public Visualisation Visualisation { get; internal set; } = null!;
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets called every frame before executing rendertasks.
    /// </summary>
    public abstract void Draw(RenderTexture rendertarget, View view);

    public T? GetVisualisationService<T>() where T : VisualisationService =>
        Visualisation.GetVisualisationService<T>();

    public T GetRequiredVisualisationService<T>() where T : VisualisationService => GetVisualisationService<T>() ?? throw new Exception($"No instance of {typeof(T)} found in the visualisation");

    public abstract bool HasImGuiEditElements { get; }

    public virtual void DrawImGuiEdit()
    {
    }
}