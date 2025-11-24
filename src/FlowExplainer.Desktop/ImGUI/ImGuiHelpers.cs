using System.Globalization;
using ImGuiNET;

namespace FlowExplainer;

public class ImGuiHelpers
{
    public static string LastMessage;
    public static DateTime MessageTime;

    public static bool SliderFloat(string name, ref double f, double min, double max)
    {
        float ff = (float)f;
        if (ImGui.SliderFloat(name, ref ff, (float)min, (float)max))
        {
            f = ff;
            var format = "N2";
            if (double.Abs(max - min) < .6f)
                format = "N3";

            UpdateMsg(name, f.ToString(format, CultureInfo.InvariantCulture));
            return true;
        }

        return false;
    }


    public static void OptionalGradientSelector(ref ColorGradient? ColorGradient)
    {
        var f = ColorGradient != null;

        string name = ColorGradient?.Name ?? "";
        ImGui.Text("Alt gradient");
        ImGui.SameLine();
        ImGui.SetCursorPosX(150);
        
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
        if (ImGui.BeginCombo("##gradient", name))
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

    public static void OptionalDoubleSlider(string name, ref double? v, double min, double max)
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

    public static void OptonalVectorFieldSelector(Dataset dataset, ref IVectorField<Vec3, Vec2>? vectorField)
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
            vectorField = dataset.VectorFields.First().Value;
        foreach (var loadedDatasetVectorField in dataset.VectorFields)
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
            foreach (var v in dataset.VectorFields)
                if (ImGui.Selectable(v.Key))
                    vectorField = v.Value;

            ImGui.EndCombo();
        }

        if (!f)
            ImGui.EndDisabled();
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

    public static bool StartDataSection()
    {
        return ImGui.BeginPopup("data");
    }

    public static void EndDataSection()
    {
        ImGui.EndPopup();
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