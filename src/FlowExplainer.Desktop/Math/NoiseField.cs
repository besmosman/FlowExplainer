namespace FlowExplainer;

public class NoiseField : IVectorField<Vec2, float>
{
    public IDomain<Vec2> Domain => IDomain<Vec2>.Infinite;
    public IBounding<Vec2> Bounding { get; } = BoundingFunctions.None<Vec2>();

    FastNoise noise = new FastNoise();

    public float Evaluate(Vec2 x)
    {
        TryEvaluate(x, out var v);
        return v;
    }
    

    public bool TryEvaluate(Vec2 x, out float value)
    {
        value = ((noise.GetNoise(x.X * 4000, x.Y * 4000)) + 1) * .5f;
        return true;
    }
}