using System.Globalization;
using ImGuiNET;

namespace FlowExplainer;

public class ImGuiHelpers
{
    public static string LastMessage;
    public static DateTime MessageTime;

    public static void SliderFloat(string name, ref float f, float min, float max)
    {
        if (ImGui.SliderFloat(name, ref f, min, max))
        {
            var format = "N2";
            if (float.Abs(max - min) < .6f)
                format = "N3";
            
            UpdateMsg(name, f.ToString(format, CultureInfo.InvariantCulture));
        }
    }

    private static void UpdateMsg(string name, string value)
    {
        LastMessage = name + ": " + value;
        MessageTime = DateTime.Now;
    }
}