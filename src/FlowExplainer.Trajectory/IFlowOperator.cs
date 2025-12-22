using System.Buffers;

namespace FlowExplainer;

public interface IFlowOperator<X, P>
    where X : IVec<X, double>, IVecUpDimension<P>
    where P : IVec<P, double>, IVecDownDimension<X>
{
    Trajectory<P> ComputeTrajectory(double t_start, double t_end, X x, IVectorField<P, X> v);
    X ComputeEnd(double t_start, double t_end, X x, IVectorField<P, X> v);

    public static IFlowOperator<X, P> Default { get; } = new DefaultFlowOperator(64);

    class DefaultFlowOperator : IFlowOperator<X, P>
    {
        public int Steps;
        public DefaultFlowOperator(int steps)
        {
            Steps = steps;
        }
        public static IIntegrator<P, X> Integrator = IIntegrator<P, X>.Rk4;

        public Trajectory<P> ComputeTrajectory(double t_start, double t_end, X x, IVectorField<P, X> v)
        {
            var duration = (t_end - t_start);
            double dt = duration / Steps;
            var phase = x.Up(t_start);
            var points = new List<P>(Steps);
            points.Add(phase);
            for (int i = 0; i < Steps; i++)
            {
                phase = Integrator.Integrate(v, v.Domain.Bounding.Bound(phase), dt);
                points.Add(phase);
            }

            return new Trajectory<P>(points.ToArray());
        }

        public X ComputeEnd(double t_start, double t_end, X x, IVectorField<P, X> v)
        {
            var phase = x.Up(t_start);
            var dt = (t_end - t_start) / Steps;
            for (int i = 0; i < Steps; i++)
            {
                phase = Integrator.Integrate(v, phase, dt);
            }
            phase.Last = t_end; //floating points really do float
            return phase.Down();
        }
    }
}