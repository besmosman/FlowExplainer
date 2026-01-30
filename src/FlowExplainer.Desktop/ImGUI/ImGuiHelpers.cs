using System.Globalization;
using System.Numerics;
using ImGuiNET;

namespace FlowExplainer;

public class ImGuiHelpers
{
    public static string LastMessage;
    public static DateTime MessageTime;

    public static bool Slider(string name, ref double f, double min, double max)
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
        ImGuiHelpers.Slider($"##{name}", ref t, min, max);
        if (v == null)
        {
            ImGui.EndDisabled();
        }
        if (v != null)
            v = t;
    }

    public static void OptonalVectorFieldSelector(World world, ref IVectorField<Vec3, Vec2>? vectorField)
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

        var vectorfields = world.GetSelectableVectorFields<Vec3, Vec2>().ToList();
        if (vectorField == null && f)
            vectorField =  vectorfields.First().VectorField;
        foreach (var selectable in vectorfields)
        {
            if (selectable.VectorField == vectorField)
            {
                name = selectable.DisplayName;
            }
        }

        if (!f)
            ImGui.BeginDisabled();
        if (ImGui.BeginCombo("##a", name))
        {
            foreach (var v in vectorfields)
                if (ImGui.Selectable(v.DisplayName))
                    vectorField = v.VectorField;

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
    public static void ColorPicker(string name, ref Color color)
    {
        ImGui.BeginGroup();
        if (ImGui.ColorButton(name, color.ToNumerics()))
        {
        }
        ImGui.SameLine();
        ImGui.Text("Color");
        ImGui.EndGroup();
        if (ImGui.IsItemClicked())
            ImGui.OpenPopup(name + " picker popup");
        if (ImGui.BeginPopup(name + " picker popup"))
        {
            var r = color.ToVec4().Down().ToNumerics();
            ImGui.ColorPicker3(name + " picker", ref r);
            color = new Color(r.X, r.Y, r.Z, color.A);
            ImGui.EndPopup();
        }
    }
}