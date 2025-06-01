namespace FlowExplainer;

public static class StreamLineGenerator
{
    public static Line<TOut> Generate<TOut, TPhase>(IVectorField<TPhase, TOut> flowField, IIntegrator<TPhase, TOut> integrator, TOut pos, float startTime, float elipson, int steps) where TOut : IAddDimension<TOut, TPhase>
    {
        List<TOut> points = new();
        float dt = elipson / ((float)steps - 1);

        points.Add(pos);
        var cur = pos;
        for (int i = 0; i < steps -1; i++)
        {
            var t = startTime + i * dt;
            cur = integrator.Integrate(flowField.Evaluate, cur.Up(t), dt);
            points.Add(cur);
        }

        return new Line<TOut>(points);
    }
}