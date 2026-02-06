using System.Globalization;
using System.Text;

namespace FlowExplainer;

public class VectorFieldWriterPlaintext
{
    public static RegularGridVectorField<Vec3, Vec3i, double> Load(string path)
    {
        var lines = File.ReadAllLines(path);
        var gridSizeText = lines[0].Split(' ');
        var domainSizeText = lines[1].Split(' ');
        var gridSize = new Vec3i(int.Parse(gridSizeText[0]), int.Parse(gridSizeText[1]), int.Parse(gridSizeText[2]));
        var domainSize = new Vec3(double.Parse(domainSizeText[0]), double.Parse(domainSizeText[1]), double.Parse(domainSizeText[2]));
        RegularGridVectorField<Vec3, Vec3i, double> vectorField = new(gridSize, Vec3.Zero, domainSize);
        for (int i = 2; i < lines.Length; i++)
        {
            vectorField.Grid.Data[i - 2] = double.Parse(lines[i]);
        }
        return vectorField;
    }

    public static void Write(RegularGridVectorField<Vec3, Vec3i, double> field, string path)
    {
        StringBuilder s = new StringBuilder();
        s.Append(field.GridSize.X);
        s.Append(' ');
        s.Append(field.GridSize.Y);
        s.Append(' ');
        s.Append(field.GridSize.Z);
        s.AppendLine();

        s.Append(field.RectDomain.Rect.Size.X);
        s.Append(' ');
        s.Append(field.RectDomain.Rect.Size.Y);
        s.Append(' ');
        s.Append(field.RectDomain.Rect.Size.Z);
        s.AppendLine();

        foreach (var v in field.Grid.Data)
        {
            s.AppendLine(v.ToString(CultureInfo.InvariantCulture));
        }

        File.WriteAllText(path, s.ToString());
    }
}