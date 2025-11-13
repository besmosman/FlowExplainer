using System.Buffers;

namespace FlowExplainer;

public interface IFlowOperator<X, P>
    where X : IVec<X>, IVecUpDimension<P>
    where P : IVec<P>, IVecDownDimension<X>
{
    Trajectory<P> Compute(double t_start, double t_end, X x, IVectorField<P, X> v);

    public static IFlowOperator<X, P> Default { get; } = new DefaultFlowOperator(64);

    class DefaultFlowOperator : IFlowOperator<X, P>
    {
        public int Steps;
        public DefaultFlowOperator(int steps)
        {
            Steps = steps;
        }
        public static IIntegrator<P, X> Integrator = IIntegrator<P, X>.Rk4;

        public Trajectory<P> Compute(double t_start, double t_end, X x, IVectorField<P, X> v)
        {
            var duration = (t_end - t_start);
            double dt = duration / Steps;
            var cur = x;
            var points = new List<P>(Steps);
            points.Add(x.Up(t_start));
            for (int i = 1; i < Steps; i++)
            {
                double t = ((double)i / (Steps - 1)) * duration + t_start;
                cur = Integrator.Integrate(v, v.Domain.Bounding.Bound(cur.Up(t)), dt);
                //if(!v.Domain.IsWithinSpace(cur))
                //    break;
                points.Add(cur.Up(t));
            }

            return new Trajectory<P>(points.ToArray());
        }
    }
}