using System.Buffers;

namespace FlowExplainer;

public interface IFlowOperatorSteady<TPhase>
    where TPhase : IVec<TPhase, double>
{
    Trajectory<TPhase> ComputeTrajectory(TPhase x, double duration, IVectorField<TPhase, TPhase> v);
    TPhase ComputeEnd(TPhase x, double duration, IVectorField<TPhase, TPhase> v);

    public static IFlowOperatorSteady<TPhase> Default => new DefaultFlowOperatorSteady(164);

    private class DefaultFlowOperatorSteady : IFlowOperatorSteady<TPhase>
    {
        public int Steps;
        private readonly IIntegrator<TPhase, TPhase> Integrator = IIntegrator<TPhase, TPhase>.Rk4Steady;

        public DefaultFlowOperatorSteady(int steps)
        {
            Steps = steps;
        }

        public Trajectory<TPhase> ComputeTrajectory(TPhase x, double duration, IVectorField<TPhase, TPhase> v)
        {
            int steps = Steps;
            double dt = duration / steps;
            var phase = x;
            var points = new List<TPhase>(steps);
            points.Add(phase);
            for (int i = 0; i < steps; i++)
            {
                phase = Integrator.Integrate(v, v.Domain.Bounding.Bound(phase), dt);
                points.Add(phase);
            }

            return new Trajectory<TPhase>(points.ToArray());
        }
        public TPhase ComputeEnd(TPhase x, double duration, IVectorField<TPhase, TPhase> v)
        {
            var dt = duration / Steps;

            for (int i = 0; i < Steps; i++)
                x = Integrator.Integrate(v, x, dt);

            return x;
        }
    }
}

public interface IFlowOperator<TInput, TOutput>
    where TInput : IVec<TInput, double>, IVecUpDimension<TOutput>
    where TOutput : IVec<TOutput, double>, IVecDownDimension<TInput>
{
    Trajectory<TOutput> ComputeTrajectory(double t_start, double t_end, TInput x, IVectorField<TOutput, TInput> v);
    TInput ComputeEnd(double t_start, double t_end, TInput x, IVectorField<TOutput, TInput> v);

    public static IFlowOperator<TInput, TOutput> Default { get; } = new DefaultFlowOperatorUnsteady(64);


    class DefaultFlowOperatorUnsteady : IFlowOperator<TInput, TOutput>
    {
        public int Steps;
        public DefaultFlowOperatorUnsteady(int steps)
        {
            Steps = steps;
        }

        public static IIntegrator<TOutput, TInput> Integrator = IIntegrator<TOutput, TInput>.Rk4;

        public Trajectory<TOutput> ComputeTrajectory(double t_start, double t_end, TInput x, IVectorField<TOutput, TInput> v)
        {
            var duration = (t_end - t_start);
            int steps = Steps;
            double dt = duration / steps;
            var phase = x.Up(t_start);
            var points = new List<TOutput>(steps);
            points.Add(phase);
            for (int i = 0; i < steps; i++)
            {
                phase = Integrator.Integrate(v, v.Domain.Bounding.Bound(phase), dt);
                points.Add(phase);
            }

            return new Trajectory<TOutput>(points.ToArray());
        }

        public TInput ComputeEnd(double t_start, double t_end, TInput x, IVectorField<TOutput, TInput> v)
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