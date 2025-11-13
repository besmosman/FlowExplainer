using System.Numerics;
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

            var visualizationService = imguiService.GetRequiredGlobalService<WorldManagerService>();

            ImGui.Begin(Enum.GetName(category), ref imguiService.RenderData.ShowToolServices[ind]);

            var vis = imguiService.GetGlobalService<ViewsService>().Views[0].World;
            foreach (var s in vis.Services)
            {
                if (OverridesImGuiDrawCall(s.GetType()) && s.Category == category)
                {
                    string name = s.GetType().Name.Replace("RenderService", " renderer").Replace("Service", "");
                    bool sIsEnabled = s.IsEnabled;

                    if (CheckableCollapsingHeader(name, ref sIsEnabled, ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        // ImGui.TextColored(ImGuiController.highlightColor, name);
                        /*ImGui.SameLine();
                        ImGui.Checkbox("##Checkbox", ref sIsEnabled);
                        ImGui.Text("Header");
                        s.IsEnabled = sIsEnabled;
                        ImGui.Separator();
                        ImGui.Separator();*/ /**/
                        if (s.IsEnabled)
                        {
                            ImGui.PushID(name);
                            s.DrawImGuiEdit();
                            ImGui.PopID();
                            ImGui.Spacing();
                            ImGui.Spacing();
                        }
                        // ImGui.colla();
                    }

                    if (s.IsEnabled != sIsEnabled)
                    {

                        if (s.IsEnabled)
                            s.OnDisable();
                        else
                            s.OnEnable();

                        if (s is IAxisTitle axisTitle)
                        {
                            if (s.IsEnabled)
                                s.GetRequiredWorldService<AxisVisualizer>().titler = null;
                            else
                                s.GetRequiredWorldService<AxisVisualizer>().titler = axisTitle;
                        }

                        if (s is IGradientScaler scaler)
                        {
                            if (s.IsEnabled)
                                s.GetRequiredWorldService<AxisVisualizer>().scaler = null;
                            else
                                s.GetRequiredWorldService<AxisVisualizer>().scaler = scaler;
                        }

                        s.IsEnabled = sIsEnabled;
                    }
                }
            }

            ImGui.End();
        }
    }

    static bool CheckableCollapsingHeader(string label, ref bool v, ImGuiTreeNodeFlags flags = 0)
    {
        unsafe
        {
            ImGui.PushID(label);
            bool is_open = ImGui.CollapsingHeader("##CollapsingHeader", flags | ImGuiTreeNodeFlags.AllowOverlap);
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(1), label);
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 50);
            var c = ImGui.GetStyleColorVec4(ImGuiCol.Header);
            ImGui.PushStyleColor(ImGuiCol.FrameBg, Color.Grey(.2f).ToNumerics());
            ImGui.Checkbox("##Checkbox", ref v);
            ImGui.PopStyleColor();
            ImGui.PopID();
            return is_open;
        }
    }
}