
namespace FlowExplainer;

public class Line<T>
{
    public List<T> points;


    public Line(List<T> points)
    {
        this.points = points;
    }

    public IEnumerable<T> Points => points;
}

