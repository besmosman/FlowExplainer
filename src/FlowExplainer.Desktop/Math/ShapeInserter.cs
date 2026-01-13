namespace FlowExplainer;

public class EarClipping<T>
{
    public static IEnumerable<(T, T, T)> Triangulate(T[] orderedEdges, Func<T, Vec2> getPos)
    {
        var remainingEdges = orderedEdges.ToList();

        while (remainingEdges.Count > 3)
            for (int i = 0; i < remainingEdges.Count; i++)
            {
                var pre = i - 1;
                var next = i + 1;
                if (i == 0)
                    pre = remainingEdges.Count - 1;
                if (i == remainingEdges.Count - 1)
                    next = 0;

                var e0 = remainingEdges[pre];
                var e1 = remainingEdges[i];
                var e2 = remainingEdges[next];

                var a = getPos(e0);
                var b = getPos(e1);
                var c = getPos(e2);
                var cross = (c.X - b.X) * (a.Y - b.Y) - (c.Y - b.Y) * (a.X - b.X);

                if (Math.Abs(cross) < 1e-10f)
                    continue;

                bool angleValid = cross < 0; //clockwise

                if (!angleValid)
                    continue;

                int overlapping = 0;
                for (int j = 0; j < remainingEdges.Count; j++)
                {
                    var n = remainingEdges[j];
                    if (j != pre && j != i && j != next)
                        if (IsInsideTriangle(a, b, c, getPos(n)))
                            overlapping++;
                }

                if (overlapping != 0)
                    continue;

                yield return (e0, e1, e2);
                remainingEdges.RemoveAt(i);
                break;
            }


        yield return (remainingEdges[0], remainingEdges[1], remainingEdges[2]);
    }

    //nils-olovsson.se/articles/ear_clipping_triangulation/
    private static bool IsInsideTriangle(Vec2 a, Vec2 b, Vec2 c, Vec2 p)
    {
        var v0 = c - a;
        var v1 = b - a;
        var v2 = p - a;

        var dot00 = Vec2.Dot(v0, v0);
        var dot01 = Vec2.Dot(v0, v1);
        var dot02 = Vec2.Dot(v0, v2);
        var dot11 = Vec2.Dot(v1, v1);
        var dot12 = Vec2.Dot(v1, v2);

        var denom = dot00 * dot11 - dot01 * dot01;
        if (double.Abs(denom) < 1e-10f)
            return true;
        var invDenom = 1.0f / denom;
        var u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        var v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        return (u >= 0) && (v >= 0) && (u + v < 1);
    }
}

public class ShapeInserter : WorldService
{
    public class Shape
    {
        public Mesh Mesh;
        public List<Vec2> Points;
        public List<double> InitialDistances;
    }

    private List<Shape> shapes = new();
    public override void Initialize()
    {

        var points = new List<Vec2>();
        var distances = new List<double>();
        int count = 32;
        double radius = .04;
        Vec2 offset = new Vec2(.5, .25);
        for (int i = 0; i < count; i++)
        {
            var circle = new Vec2(double.Cos((double)i / count * double.Pi * 2), double.Sin((double)i / count * double.Pi * 2));
            var pos = circle * radius + offset;
            points.Add(pos);
        }
        for (int i = 0; i < count; i++)
        {
            var cur = points[i];
            var next = i == points.Count - 1 ? points[0] : points[i + 1];
            distances.Add(Vec2.Distance(cur, next));
        }
        var verts = new Vertex[points.Count];
        for (int i = 0; i < points.Count; i++)
            verts[i].Position = points[i].Up(0);
        /*var triangles = EarClipping<Vertex>.Triangulate(verts, v => v.Position.XY).ToArray();
        var indicies = new int[triangles.Length * 3];
        for (int i = 0; i < triangles.Length; i++)
        {
            indicies[i * 3 + 0] = triangles[i].Item1;
        }*/
        shapes.Add(new Shape()
        {
            Points = points,
            InitialDistances = distances,
            //Mesh = new Mesh()
        });
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        var dat = GetRequiredWorldService<DataService>()!;
        var vectorfield = dat.VectorField;
        var time = dat.SimulationTime;

        foreach (var shape in shapes)
        {
            for (int i = 0; i < shape.Points.Count; i++)
            {
                shape.Points[i] = IIntegrator<Vec3, Vec2>.Rk4.Integrate(vectorfield, shape.Points[i].Up(time), dat.MultipliedDeltaTime).XY;
            }
            for (int i = shape.Points.Count - 1; i >= 0; i--)
            {
                var curI = i;
                var nextI = i == shape.Points.Count - 1 ? 0 : i + 1;
                double distance = Vec2.Distance(shape.Points[curI], shape.Points[nextI]);
                if (distance >= 2 * shape.InitialDistances[i])
                {
                    shape.InitialDistances.Insert(i + 1, shape.InitialDistances[i]);
                    shape.Points.Insert(i + 1, (shape.Points[curI] + shape.Points[nextI]) / 2);
                }
            }
        }
        foreach (var shape in shapes)
        {
            for (int i = 0; i < shape.Points.Count; i++)
            {
                var cur = shape.Points[i];
                var next = i == shape.Points.Count - 1 ? shape.Points[0] : shape.Points[i + 1];
                var curBound = vectorfield.Domain.Bounding.Bound(cur.Up(time)).XY;
                var nextBound = vectorfield.Domain.Bounding.Bound(next.Up(time)).XY;
                if (Vec2.DistanceSquared(curBound, nextBound) < shape.InitialDistances[i] * 2)
                {
                    Gizmos2D.Line(view.Camera2D, curBound, nextBound, Color.Green, .001f);
                }
            }
        }
    }
}