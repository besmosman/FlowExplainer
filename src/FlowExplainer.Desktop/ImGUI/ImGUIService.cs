using System.Numerics;
using FlowExplainer.Logging;
using ImGuiNET;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FlowExplainer;

//based off https://stackoverflow.com/questions/9088227/using-getopenfilename-instead-of-openfiledialog
public class ImGuiRenderData
{
    public bool checkbox;
    public bool showPreferencesWindow;
    public bool showDemoWindow;
    public bool ShowTCKDataWindow = false;
    public bool ShowVisualizationManagerWindow = true;
    public bool ShowVisualisationsWindow = false;
    public bool ShowToolWindow = true;
    public int SelectedVisualiationIndex;
    public bool showTCKMetaDataPopup;

    public ImGuiRenderData()
    {
    }
}

public class ImGUIService : GlobalService
{
    public ImGuiRenderData RenderData = new();

    public override void Draw()
    {
        var window = GetRequiredGlobalService<WindowService>().Window;

        if (window.KeyboardState.IsKeyPressed(Keys.F11))
            SwapFullScreen(window);

        ImGui.BeginMainMenuBar();
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vec4(0.6f));
            ImGui.Text("FlowExplainer");
            ImGui.PopStyleColor();
            /*
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Open"))
                {
                    var x = OpenFile.GetOpenFileName(new OpenFile.OpenFileArgs
                    {
                        filter = "TCK files\0*.tck\0All files\0*.*\0",
                        title = "Load TCK File",
                    });
                }

                if (ImGui.MenuItem("Preferences", "", RenderData.showPreferencesWindow, true))
                {
                    RenderData.editingPreferences = GetRequiredGlobalService<PreferencesService>().Preferences;
                    RenderData.showPreferencesWindow = !RenderData.showPreferencesWindow;
                }

                ImGui.EndMenu();
            }
            */

            if (ImGui.BeginMenu("View"))
            {
                if (ImGui.MenuItem("Fullscreen", "F11", window.IsFullscreen, true))
                {
                    SwapFullScreen(window);
                }


                ImGui.MenuItem("ImGUI Demo", "", ref RenderData.showDemoWindow);
                ImGui.MenuItem("Visualisations", "", ref RenderData.ShowVisualisationsWindow);
                ImGui.MenuItem($"Services Tool Window", "", ref RenderData.ShowToolWindow);

                if (ImGui.MenuItem("New view"))
                    GetRequiredGlobalService<ViewsService>().NewView();
                if (ImGui.MenuItem("New world"))
                {
                    var w = GetRequiredGlobalService<WorldManagerService>().NewWorld();
                    Scripting.SetGyreDataset(w);
                    w.AddVisualisationService(new AxisVisualizer());
                }
                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }
        // ImGui.DockSpaceOverViewport(0);

        if (RenderData.showDemoWindow)
            ImGui.ShowDemoWindow();


        ImGuiToolWindows.Draw(this);


        DrawLogger();
    }

    private static void SwapFullScreen(NativeWindow window)
    {
        if (window.WindowState == OpenTK.Windowing.Common.WindowState.Fullscreen)
            window.WindowState = OpenTK.Windowing.Common.WindowState.Normal;
        else
            window.WindowState = OpenTK.Windowing.Common.WindowState.Fullscreen;
    }

    /*
    private void DrawPreferencesWindow()
    {
        if (!RenderData.showPreferencesWindow)
            return;
        //            ImGui.SetNextWindowDockID(ImGui.GetID("main_dock"));
        if (ImGui.Begin("Preferences", ref RenderData.showPreferencesWindow))
        {
            var prefs = GetRequiredGlobalService<PreferencesService>();

            ImGui.Checkbox("VSync", ref RenderData.editingPreferences.VSync);
            ImGuiHelpers.SliderFloat("UIScale", ref RenderData.editingPreferences.UIScale, .5f, 3);
            if (ImGui.Button("Reset"))
            {
                RenderData.editingPreferences = prefs.Preferences;
            }

            ImGui.SameLine();
            if (ImGui.Button("Save"))
            {
                prefs.UpdateAndSavePreferences(RenderData.editingPreferences);
            }

            ImGui.SameLine();
            if (ImGui.Button("Load default preferences"))
            {
                RenderData.editingPreferences = prefs.GenerateDefaultPreferences();
            }

            ImGui.End();
        }
    }
    */


    Dictionary<LogLevel, Vec4> LogColours = new()
    {
        {
            LogLevel.Message, new Vec4(0, 1, 0, 1)
        },
        {
            LogLevel.Debug, new Vec4(0, 1, 1, 1)
        },
        {
            LogLevel.Warning, new Vec4(1, 0, 1, 1)
        },
    };

    private int lastLogId;
    private string input = "";
    public bool ConsoleVisible = false;
    public CommandHandler CommandHandler = new();

    private void DrawLogger()
    {

        var window = GetRequiredGlobalService<WindowService>().Window;
        if (window.IsKeyPressed(Keys.GraveAccent))
        {
            ConsoleVisible = !ConsoleVisible;
        }

        if (!ConsoleVisible)
            return;

        Logger.Clean(500);
        ImGui.SetNextWindowPos(new Vector2(0, 0));
        var lh = ImGui.GetTextLineHeightWithSpacing();
        double spacing = ImGui.GetTextLineHeightWithSpacing() - ImGui.GetTextLineHeight();
        ImGui.SetNextWindowSize(new Vector2(window.Size.X, (float)(lh * 14 + spacing * 3)));
        ImGui.Begin("Logger", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize);
        ImGui.BeginChild("oi", new Vector2(window.Size.X, (float)(lh * 12 + spacing * 1)));
        foreach (var log in Logger.GetLogs())
        {
            ImGui.TextColored(LogColours[log.LogLevel], $"[{Enum.GetName(log.LogLevel)}]");
            ImGui.SameLine();
            ImGui.Text(log.Message);
        }

        if (lastLogId != Logger.LastEntryID)
        {
            lastLogId = Logger.LastEntryID;
            ImGui.SetScrollHereY();
        }

        ImGui.EndChild();
        ImGui.Spacing();
        ImGui.Separator();
        bool textEntered = ImGui.InputText("Command", ref input, 256, ImGuiInputTextFlags.EnterReturnsTrue);
        if (textEntered)
        {
            CommandHandler.Execute(input);
            input = "";
        }

        if (window.IsKeyPressed(Keys.GraveAccent) || textEntered)
            ImGui.SetKeyboardFocusHere(-1);
        ImGui.SameLine();
        if (ImGui.Button("Clear all"))
            Logger.Clean(0);
        ImGui.End();
    }

    public override void Initialize()
    {
        CommandHandler.FlowExplainer = FlowExplainer;
        CommandHandler.InitilizeCommands();
    }
}