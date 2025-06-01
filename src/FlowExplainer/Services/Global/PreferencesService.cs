using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FlowExplainer;

public class PreferencesService : GlobalService
{
    public Preferences Preferences;
    public event Action<Preferences>? OnPreferencesChange;

    public override unsafe void Initialize()
    {
        Preferences = GenerateDefaultPreferences();
    }

    public Preferences GenerateDefaultPreferences()
    {
        float x;
        unsafe
        {
            GLFW.Init();
            GLFW.GetMonitorContentScale(GLFW.GetPrimaryMonitor(), out x, out float _);
        }
        return new Preferences()
        {
            UIScale = x,
            VSync = false,
            WindowSizeOnStartup = new Vec2i((int)(1280 * x), (int)(720 * x))
        };
    }

    [Command]
    public void UIScale(float scale)
    {
        Preferences.UIScale = scale;
    }

    public override void Draw()
    {
    }

    public void UpdateAndSavePreferences(Preferences preferences)
    {
        Preferences = preferences;
        OnPreferencesChange?.Invoke(preferences);
    }


}