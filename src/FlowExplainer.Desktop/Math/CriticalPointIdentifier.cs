using System.Collections.Concurrent;
using OpenTK.Mathematics;

namespace FlowExplainer;

public static class FD
{

    public struct Neighbors(Vec2 left, Vec2 right, Vec2 up, Vec2 down, Vec2 delta)
    {
        public double dFx_dx => FD.Derivative(left.X, right.X, delta.X);
        public double dFy_dx => FD.Derivative(left.Y, right.Y, delta.X);
        public double dFx_dy => FD.Derivative(down.X, up.X, delta.Y);
        public double dFy_dy => FD.Derivative(down.Y, up.Y, delta.Y);
    }

    public static Neighbors CentralDifference(Vec2 center, Vec2 delta, Func<Vec2, Vec2> eval)
    {
        var left = eval(center - new Vec2(delta.X, 0));
        var right = eval(center + new Vec2(delta.X, 0));
        var up = eval(center + new Vec2(0, delta.Y));
        var down = eval(center - new Vec2(0, delta.Y));
        return new Neighbors(left, right, up, down, delta);
    }

    public static Neighbors CentralDifference(Vec2 left, Vec2 right, Vec2 up, Vec2 down, Vec2 delta)
    {
        return new Neighbors(left, right, up, down, delta);
    }

    public static double Derivative(double left, double right, double d)
    {
        return (right - left) / (2 * d);
    }

    public static Vec2 Derivative(Vec2 left, Vec2 right, Vec2 d)
    {
        return new Vec2(
            Derivative(left.X, right.X, d.X),
            Derivative(left.Y, right.Y, d.Y));
    }

    public static double Divergence(Vec2 left, Vec2 right, Vec2 up, Vec2 down, Vec2 d)
    {
        return FD.Derivative(left.X, right.X, d.X) + FD.Derivative(up.Y, down.Y, d.Y);
    }
}

public class CriticalPointIdentifier : WorldService
{

    private ConcurrentQueue<Rect<Vec2>> toTestRegions = new();
    private ConcurrentQueue<Rect<Vec2>> criticalRegions = new();

    public override void Initialize()
    {
        FindCriticalPoints();
    }
    private void FindCriticalPoints()
    {
        criticalRegions.Clear();
        var vectorField = GetRequiredWorldService<DataService>().VectorField;
        var reduce = vectorField.Domain.RectBoundary.Reduce<Vec2>();
        reduce.Max*=1.2f;
        reduce.Min/=1.2f;
        reduce.Max+=new Vec2(Random.Shared.NextDouble(), Random.Shared.NextDouble())/10f;
        reduce.Min+=new Vec2(Random.Shared.NextDouble(), Random.Shared.NextDouble())/10f;

        //reduce.Min -= new Vec2(.25f, 0);
        //reduce.Max += new Vec2(.25f, 0);
        toTestRegions.Enqueue(reduce);
        double t = 0;
        double minCellAreaRatioOfDomain = 1 / 30000.0;
        double domainArea = vectorField.Domain.RectBoundary.Size.X * vectorField.Domain.RectBoundary.Size.Y;
        double minCellArea = domainArea * minCellAreaRatioOfDomain;
        while (!toTestRegions.IsEmpty)
        {
            if (toTestRegions.TryDequeue(out var rect))
            {
                if (ContainsCriticalPoint(rect, vectorField, t) || rect.Size.X * rect.Size.Y > minCellArea*100)
                {
                    if (rect.Size.X * rect.Size.Y < minCellArea)
                    {
                        criticalRegions.Enqueue(rect);
                    }
                    else
                    {
                        toTestRegions.Enqueue(Rect<Vec2>.FromSize(rect.Min, rect.Size / 2));
                        toTestRegions.Enqueue(Rect<Vec2>.FromSize(rect.Min + rect.Size / 2, rect.Size / 2));
                        toTestRegions.Enqueue(Rect<Vec2>.FromSize(rect.Min + new Vec2(rect.Size.X / 2, 0), rect.Size / 2));
                        toTestRegions.Enqueue(Rect<Vec2>.FromSize(rect.Min + new Vec2(0, rect.Size.Y / 2), rect.Size / 2));
                    }
                }
            }

        }
    }

    private bool ContainsCriticalPoint(Rect<Vec2> rect, IVectorField<Vec3, Vec2> vectorField, double t)
    {

        // Evaluate vector field at corners
        var lb = vectorField.Evaluate(rect.Min.Up(t)); // lower-left
        var rb = vectorField.Evaluate((rect.Min + new Vec2(rect.Size.X, 0)).Up(t)); // lower-right
        var rt = vectorField.Evaluate(rect.Max.Up(t)); // upper-right
        var lt = vectorField.Evaluate((rect.Min + new Vec2(0, rect.Size.Y)).Up(t)); // upper-left

        // Put vectors in order along loop (counter-clockwise)
        var vectors = new Vec2[]
        {
            lb, rb, rt, lt, lb
        }; // close loop

        double totalAngleChange =0.0;

        for (int i = 0; i < 4; i++)
        {
            var v1 = vectors[i];
            var v2 = vectors[i + 1];

            // Compute angles of vectors
            double angle1 = Math.Atan2(v1.Y, v1.X);
            double angle2 = Math.Atan2(v2.Y, v2.X);

            // Compute difference and wrap to [-pi, pi]
            double delta = angle2 - angle1;
            while (delta <= -Math.PI) delta += 2 * Math.PI;
            while (delta > Math.PI) delta -= 2 * Math.PI;

            totalAngleChange += delta;
        }

        // Compute Poincaré index
        double index = totalAngleChange / (2 * Math.PI);

        // Small tolerance for doubleing point errors
        if(Math.Abs(index) > 0.5f)
        return true;



        // Sample 9 points: corners, mid-edges, center
        var pts = new List<Vec2>
        {
            rect.Min,
            rect.Min + new Vec2(rect.Size.X, 0),
            rect.Min + new Vec2(0, rect.Size.Y),
            rect.Min + rect.Size,
            rect.Min + rect.Size / 2, // center
            rect.Min + new Vec2(rect.Size.X / 2, 0),
            rect.Min + new Vec2(0, rect.Size.Y / 2),
            rect.Min + new Vec2(rect.Size.X, rect.Size.Y / 2),
            rect.Min + new Vec2(rect.Size.X / 2, rect.Size.Y)
        };

        var values = pts.Select(p => vectorField.Evaluate(p.Up(t))).ToArray();

        bool signChangeX = values.Select(v => Math.Sign(v.X)).Distinct().Count() > 1;
        bool signChangeY = values.Select(v => Math.Sign(v.Y)).Distinct().Count() > 1;
        bool nearZero = values.Any(v => v.Length() < 1e-12f);

        return (signChangeX && signChangeY) || nearZero;
    }
    public override void Draw(RenderTexture rendertarget, View view)
    {
        return;
        FindCriticalPoints();
        var t = GetRequiredWorldService<DataService>().SimulationTime;
        var vectorField = GetRequiredWorldService<DataService>().VectorField;
        foreach (var r in criticalRegions)
        {
            var pp = r.Center;
            var d = r.Size / 2;
            var u = FD.CentralDifference(pp, d, (v) => vectorField.Evaluate(v.Up(t)));

            var jacobian = new Matrix2d(u.dFx_dx, u.dFx_dy, u.dFy_dx, u.dFy_dy);
            var m = jacobian.Trace * .5f;
            var p = jacobian.Determinant;
            var n = m * m - p;

            if (n < 1e-05)
                n = 0;

            var right = double.Sqrt(n);

            var eigen1 = m + right;
            var eigen2 = m - right;

            if (double.Sign(eigen1) == double.Sign(eigen2))
            {
                //not saddlepoint
                // continue;
            }
            Gizmos2D.Circle(view.Camera2D, pp, Color.White, .005f);

                        var rk4 = IIntegrator<Vec3, Vec2>.Rk4;
            for (int k = 0; k < 1; k++)
            {
                var pos = Utils.Random(r);
                for (int c = 0; c < 64; c++)
                {
                    var lastPos = pos;
                    for (int i = 0; i < 64; i++)
                    {
                        //if (vec2.Length() > 0.000000001)
                        pos = rk4.Integrate(vectorField, pos.Up(t), .004);
                    }
                    Gizmos2D.Line(view.Camera2D, lastPos, pos, new Color(1,0,0,1), .001);
                }
            }
        }
    }
}