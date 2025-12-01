using System.Diagnostics.CodeAnalysis;

namespace FlowExplainer;

public interface IIntegrator<TInput, TOutput> where TInput : IVec<TInput>
{
    TOutput Integrate(IVectorField<TInput, TOutput> f, TInput x, double dt);

}

public static class IIntegratorExtensions
{
    extension<TInput, TOutput>(IIntegrator<TInput, TOutput>)
        where TInput : IVec<TInput>, IVecDownDimension<TOutput>
        where TOutput : IVec<TOutput>, IVecUpDimension<TInput>
    {
        public static IIntegrator<TInput, TOutput> Rk4 => new RungeKutta4IntegratorGen<TInput, TOutput>();
    }

    extension<TInput>(IIntegrator<TInput, TInput>)
        where TInput : IVec<TInput>
    {
        public static IIntegrator<TInput, TInput> Rk4Steady => new RungeKutta4IntegratorBaseGen<TInput>();
    }
}

public class IncreasedDimensionVectorField<VecLowerIn, VecLowerOut, VecOut> : IVectorField<VecLowerIn, VecOut>
    where VecLowerIn : IVec<VecLowerIn>
    where VecLowerOut : IVecUpDimension<VecOut>
{
    private IVectorField<VecLowerIn, VecLowerOut> UnsteadyField;
    private readonly IVectorField<VecLowerIn, Vec1> final;
    public IDomain<VecLowerIn> Domain { get; set; }

    public IncreasedDimensionVectorField(IVectorField<VecLowerIn, VecLowerOut> unsteadyField, IVectorField<VecLowerIn, Vec1> final)
    {
        UnsteadyField = unsteadyField;
        Domain = UnsteadyField.Domain;
        this.final = final;
    }

    public VecOut Evaluate(VecLowerIn x)
    {
        return UnsteadyField.Evaluate(x).Up(final.Evaluate(x));

    }

    public bool TryEvaluate(VecLowerIn x, [MaybeNullWhen(false)] out VecOut value)
    {
        if (UnsteadyField.TryEvaluate(x, out var v))
        {
            value = v.Up(final.Evaluate(x));
            return true;
        }
        value = default;
        return false;
    }

}