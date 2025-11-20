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

    public void AltGradientSelector(ref ColorGradient? ColorGradient)
    {
        var f = ColorGradient != null;
        ImGui.Checkbox("##grad-check", ref f);

        if (!f && ColorGradient != null)
        {
            ColorGradient = null;
        }

        if (f && ColorGradient == null)
        {
            ColorGradient = Gradients.Parula;
        }
        ImGui.SameLine();
        if (ImGui.BeginCombo("Gradient", ColorGradient?.Name ?? ""))
        {
            foreach (var grad in Gradients.All)
            {
                bool isSelected = ColorGradient == grad;
                ImGui.Image(grad.Texture.Value.TextureHandle, new Vec2(ImGui.GetTextLineHeight(), ImGui.GetTextLineHeight()));
                ImGui.SameLine();
                if (ImGui.Selectable(grad.Name, ref isSelected))
                {
                    ColorGradient = grad;
                }
            }

            ImGui.EndCombo();
        }
    }

    public void OptionalDoubleSlider(string name, ref double? v, double min, double max)
    {
        var f = v != null;
        ImGui.Text(name);
        ImGui.SameLine();
        ImGui.SetCursorPosX(150);
        ImGui.Checkbox("##double", ref f);
        ImGui.SameLine();
        if (v == null)
            name = " ";

        if (!f)
            v = null;

        if (v == null && f)
            v = min;

        var t = v ?? 0;
        if (v == null)
        {
            ImGui.BeginDisabled();
        }
        ImGuiHelpers.SliderFloat($"##{name}", ref t, min, max);
        if (v == null)
        {
            ImGui.EndDisabled();
        }
        if (v != null)
            v = t;
    }

    public void OptonalVectorFieldSelector(ref IVectorField<Vec3, Vec2>? vectorField)
    {
        var f = vectorField != null;
        ImGui.Text("Alt vectorfield");
        ImGui.SameLine();
        ImGui.SetCursorPosX(150);
        ImGui.Checkbox("##r", ref f);
        ImGui.SameLine();
        string name = " ";
        if (vectorField == null)
            name = " ";

        if (!f)
            vectorField = null;

        if (vectorField == null && f)
            vectorField = World.DataService.LoadedDataset.VectorFields.First().Value;
        foreach (var loadedDatasetVectorField in World.DataService.LoadedDataset.VectorFields)
        {
            if (loadedDatasetVectorField.Value == vectorField)
            {
                name = loadedDatasetVectorField.Key;
            }
        }

        if (!f)
            ImGui.BeginDisabled();
        if (ImGui.BeginCombo("##a", name))
        {
            foreach (var v in World.DataService.LoadedDataset.VectorFields)
                if (ImGui.Selectable(v.Key))
                    vectorField = v.Value;

            ImGui.EndCombo();
        }
        
        if (!f)
            ImGui.EndDisabled();;
    }

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