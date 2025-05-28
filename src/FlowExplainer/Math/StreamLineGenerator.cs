using System.Numerics;

namespace FlowExplainer;

public static class StreamLineGenerator
{
    public static Line Generate(IIntegrator<Vector3, Vector2> integrator, IVectorField<Vector3, Vector2> flowField, Vector2 p, float startTime, float endTime, float steps)
    {
        List<Vector2> points = new();

        points.Add(p);
        float dt = (endTime - startTime) / (steps);
        for (int i = 1; i < steps; i++)
        {
            var t = Single.Lerp(startTime, endTime, i / (float)steps);
            t = startTime;
            var next = integrator.Integrate(flowField.Evaluate, new Vector3(points.Last(), t), dt);
            points.Add(next);
        }

        return new Line(points);
    }
}