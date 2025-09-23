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

    
    public Trajectory<Z> Select<Z>(Func<T, Z> selector) where Z : IVec<Z>
    {
        var entries = new Z[Entries.Length];
        for (int i = 0; i < Entries.Length; i++)
        {
            entries[i] += selector(Entries[i]);
        }
        return new Trajectory<Z>(entries);
    }

    
    public Trajectory<Z> Select<Z>(Func<T, T, Z> selector) where Z : IVec<Z>
    {
        var entries = new Z[Entries.Length];
        var last = Entries[0];
        for (int i = 0; i < Entries.Length; i++)
        {
            entries[i] += selector(last, Entries[i]);
            last = Entries[i];
        }
        return new Trajectory<Z>(entries);
    }
    public Trajectory<T> Reverse()
    {
        var entries = new T[Entries.Length];
        for (int i = 0; i < Entries.Length; i++)
        {
            entries[i] = Entries[Entries.Length - i - 1];
        }
        return new Trajectory<T>(entries);
    }
    public T AtTime(float t)
    {
        for (int i = 0; i < Entries.Length - 1; i++)
        {
            if (Entries[i].Last <= t && Entries[i + 1].Last > t)
            {
                var c = (t - Entries[i].Last) / (Entries[i + 1].Last - Entries[i].Last);
                return Utils.Lerp(Entries[i], Entries[i + 1], c);
            }
        }
        return Entries[^1];
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
            var duration = (t_end - t_start);
            float dt = duration / Steps;
            var cur = x;
            var points = new List<P>(Steps);
            points.Add(x.Up(t_start));
            for (int i = 1; i < Steps; i++)
            {
                float t = ((float)i / (Steps - 1)) * duration + t_start;
                cur = Integrator.Integrate(v, cur.Up(t), dt);
                //if(!v.Domain.IsWithinSpace(cur))
                //    break;
                points.Add(cur.Up(t));
            }

            return new Trajectory<P>(points.ToArray());
        }
    }
}