using System.Buffers;
using System.Numerics;

namespace FlowExplainer;

public class Trajectory<T> where T : IVec<T>
{
    public T[] Entries;

    public Trajectory(T[] entries)
    {
        Entries = entries;
    }

    public Z AverageAlong<Z>(Func<T, T, Z> selector) where Z : IMultiplyOperators<Z, float, Z>, IAdditionOperators<Z, Z, Z>
    {
        Z sum = default!;

        for (int i = 1; i < Entries.Length; i++)
        {
            sum += selector(Entries[i - 1], Entries[i]) * float.Abs(Entries[i].Last - Entries[i - 1].Last);
        }

        var t = Entries.First().Last;
        var tau = Entries.Last().Last;
        return sum * (1f / float.Abs(t - tau));
    }
}

public interface IFlowOperator<X, P>
    where X : IVec<X>, IVecUpDimension<P>
    where P : IVec<P>, IVecDownDimension<X>
{
    Trajectory<P> Compute(float t_start, float t_end, X x, IVectorField<P, X> v);

    public static IFlowOperator<X, P> Default { get; } = new DefaultFlowOperator();

    class DefaultFlowOperator : IFlowOperator<X, P>
    {
        private const int Steps = 64;
        public static IIntegrator<P, X> Integrator = IIntegrator<P, X>.Rk4;

        public Trajectory<P> Compute(float t_start, float t_end, X x, IVectorField<P, X> v)
        {
            var duration = float.Abs(t_end - t_start);
            float dt = (1f / Steps) * duration;
            var cur = x;
            var points = new P[Steps];
            points[0] = x.Up(t_start);
            for (int i = 1; i < Steps; i++)
            {
                float t = (float)i / Steps * duration + t_start;
                cur = Integrator.Integrate(v.Evaluate, cur.Up(t), dt);
                points[i] = cur.Up(t);
            }

            return new Trajectory<P>(points);
        }
    }
}