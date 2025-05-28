using System.Numerics;

namespace FlowExplainer;

public class Line : ILine<Vector2>
{
    public List<Vector2> points;


    public Line(List<Vector2> points)
    {
        this.points = points;
    }

    public IEnumerable<Vector2> Points => points;
}