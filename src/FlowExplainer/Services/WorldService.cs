using FlowExplainer;
using FlowExplainer;

namespace FlowExplainer;

/// <summary>
/// Service running in a <see cref="Services.Visualisation"/> instance. Zero or one per <see cref="World"/> instance.
/// </summary>
public abstract class WorldService : Service
{
    public virtual ToolCategory Category => ToolCategory.Flow;
    public World World { get; internal set; } = null!;
    public bool IsEnabled { get; set; } = false;

    public virtual void OnEnable()
    {
    }

    public void Enable()
    {
        IsEnabled = true;
        OnEnable();
    }
    
    public void Disable()
    {
        IsEnabled = false;
        OnDisable();
    }

    
    public virtual void OnDisable()
    {
        
    }
    
    /// <summary>
    /// Can be called multiple times each frame (multiple views with same world).
    /// </summary>
    public abstract void Draw(RenderTexture rendertarget, View view);

    
    /// <summary>
    /// Gets called once per frame.
    /// </summary>
    public virtual void Update() {}
    
    public T? GetWorldService<T>() where T : WorldService =>
        World.GetWorldService<T>();

    public T GetRequiredWorldService<T>() where T : WorldService => GetWorldService<T>() ?? throw new Exception($"No instance of {typeof(T)} found in the visualisation");
    
    public virtual void DrawImGuiEdit()
    {
    }
}