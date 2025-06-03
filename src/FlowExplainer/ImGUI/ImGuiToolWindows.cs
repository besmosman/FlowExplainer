using System.Reflection;
using ImGuiNET;

namespace FlowExplainer;

public static class ImGuiToolWindows
{
    private static Dictionary<Type, bool> DrawsImGuiElements = new();
    
    public static bool OverridesImGuiDrawCall(Type type)
    {
        if (!DrawsImGuiElements.TryGetValue(type, out var v))
        {
            var m = type.GetRuntimeMethods().Where(m => m.Name == nameof(WorldService.DrawImGuiEdit));
            v = m.Single().DeclaringType == type;
            DrawsImGuiElements.Add(type, v);
        }

        return v;
    }

    public static void Draw(ImGUIService imguiService)
    {
        var values = Enum.GetValues<ToolCategory>();
        for (int ind = 0; ind < values.Length; ind++)
        {
            var category = values[ind];


            if (values[ind] == ToolCategory.None || !imguiService.RenderData.ShowToolServices[ind])
                continue;

            var visualizationService = imguiService.GetRequiredGlobalService<VisualisationManagerService>();

            ImGui.Begin(Enum.GetName(category), ref imguiService.RenderData.ShowToolServices[ind]);

            var vis = visualizationService.Worlds[imguiService.RenderData.SelectedVisualiationIndex];

            foreach (var s in vis.Services)
            {
                if (s.IsEnabled && OverridesImGuiDrawCall(s.GetType()) && s.Category == category)
                {
                    ImGui.Separator();
                    ImGui.TextColored(ImGuiController.highlightColor,
                        s.GetType().Name.Replace("RenderService", " renderer").Replace("Service", ""));
                    ImGui.Separator();
                    s.DrawImGuiEdit();
                }
            }

            ImGui.End();
        }
    }
}