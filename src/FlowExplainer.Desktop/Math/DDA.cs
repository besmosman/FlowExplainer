namespace FlowExplainer;

public static class DDA
{
    public static IEnumerable<Vec2i> Line(Vec2i start, Vec2i end)
    {
        var delta = end - start;
        int steps = (int)double.Round(double.Max(
            double.Abs(delta.X), double.Abs(delta.Y)));

        if (steps == 0)
        {
            yield return start;
            yield break;
        }

        var increment = delta.ToVecF() / steps;
        var cur = start.ToVecF();

        for (int i = 0; i <= steps; i++)
        {
            yield return cur.RoundInt();
            cur += increment;
        }
    }

    public static IEnumerable<Vec3i> Line(Vec3i start, Vec3i end)
    {
        var delta = end - start;
        int steps = (int)double.Round(double.Max(
            double.Abs(delta.X),
            double.Max(double.Abs(delta.Y), double.Abs(delta.Z))));

        if (steps == 0)
        {
            yield return start;
            yield break;
        }

        var increment = delta.ToVecF() / steps;
        var cur = start.ToVecF();

        for (int i = 0; i <= steps; i++)
        {
            yield return cur.RoundInt();
            cur += increment;
        }
    }
}