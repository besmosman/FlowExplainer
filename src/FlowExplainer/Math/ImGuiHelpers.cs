using System.Globalization;
using ImGuiNET;

namespace FlowExplainer;

public class ImGuiHelpers
{
    public static string LastMessage;
    public static DateTime MessageTime;

    public static bool SliderFloat(string name, ref float f, float min, float max)
    {
        if (ImGui.SliderFloat(name, ref f, min, max))
        {
            var format = "N2";
            if (float.Abs(max - min) < .6f)
                format = "N3";

            UpdateMsg(name, f.ToString(format, CultureInfo.InvariantCulture));
            return true;
        }
        return false;
    }

    public static bool SliderInt(string name, ref int f, int min, int max)
    {
        if (ImGui.SliderInt(name, ref f, min, max))
        {
            UpdateMsg(name, f.ToString(CultureInfo.InvariantCulture));
            return true;
        }
        return false;
    }

    private static void UpdateMsg(string name, string value)
    {
        LastMessage = name + ": " + value;
        MessageTime = DateTime.Now;
    }
    public static bool Combo<T>(string name, ref T value) where T : struct, Enum
    {
        bool set = false;
        if (ImGui.BeginCombo(name, Enum.GetName(value)))
        {
            foreach (var v in Enum.GetValues<T>())
            {
                if (ImGui.Selectable(Enum.GetName(v), value.Equals(v)))
                {
                    value = v;
                    set = true;
                }
            }
            ImGui.EndCombo();
        }
        return set;
    }
}