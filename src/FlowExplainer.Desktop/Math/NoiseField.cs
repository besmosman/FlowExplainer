namespace FlowExplainer;

public class NoiseField : IVectorField<Vec2, double>
{
    public IDomain<Vec2> Domain => IDomain<Vec2>.Infinite;
    public IBounding<Vec2> Bounding { get; } = BoundingFunctions.None<Vec2>();

    FastNoise noise = new FastNoise();

    public double Evaluate(Vec2 x)
    {
        TryEvaluate(x, out var v);
        return v;
    }
    

    public bool TryEvaluate(Vec2 x, out double value)
    {
        value = (double)((noise.GetNoise((float)x.X * 4000, (float)x.Y * 4000)) + 1) * .5f;
        return true;
    }
}