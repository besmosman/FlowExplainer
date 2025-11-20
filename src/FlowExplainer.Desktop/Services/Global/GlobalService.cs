namespace FlowExplainer;

/// <summary>
/// Global service. Zero or one instance per <see cref="FlowExplainer"/> instance.
/// </summary>
public abstract class GlobalService : Service
{
    /// <summary>
    /// Gets called every frame before executing rendertasks.
    /// </summary>
    public abstract void Draw();

    public virtual void AfterDraw() {}
}