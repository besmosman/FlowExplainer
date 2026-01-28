using FlowExplainer;
using FlowExplainer;
using ImGuiNET;

namespace FlowExplainer;

/// <summary>
/// Service running in a <see cref="Services.Visualisation"/> instance. Zero or one per <see cref="World"/> instance.
/// </summary>
public abstract class WorldService : Service
{
    public World World { get; internal set; } = null!;
    public bool IsEnabled { get; set; } = false;
    public bool IsInitialzied { get; set; }

    public bool ui_needs_open;

    public virtual string? Description { get; }
    public virtual string? Name { get; }
    public virtual string? CategoryN { get; }


    public DataService DataService => GetRequiredWorldService<DataService>();

    public virtual void OnEnable()
    {
    }

    public void Enable()
    {
        IsEnabled = true;
        if (!IsInitialzied)
        {
            Initialize();
            IsInitialzied = true;
        }
        OnEnable();
    }

    public void Disable()
    {
        IsEnabled = false;
        OnDisable();
    }


    public virtual IEnumerable<ISelectableVectorField<Vec3, double>> GetSelectableVec3Vec1()
    {
        yield break;
    }
    
    public virtual IEnumerable<ISelectableVectorField<Vec2, double>> GetSelectableVec2Vec1()
    {
        yield break;
    }
    
    public virtual IEnumerable<ISelectableVectorField<Vec3, Vec2>> GetSelectableVec3Vec2()
    {
        yield break;
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
    public virtual void Update() { }

    public T? GetWorldService<T>() where T : WorldService =>
        World.GetWorldService<T>();

    public T GetRequiredWorldService<T>() where T : WorldService => GetWorldService<T>() ?? throw new Exception($"No instance of {typeof(T)} found in the visualisation");

    public virtual void DrawImGuiDataSettings()
    {

    }

    public virtual void DrawImGuiSettings()
    {

    }
}