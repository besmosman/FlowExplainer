using System.Diagnostics.CodeAnalysis;

namespace FlowExplainer;


/*public readonly struct IntegralCurve<TPhase, TExtendedPhase> 
    where TPhase : IVec<TPhase, double>, IVecUpDimension<TExtendedPhase>
    where TExtendedPhase : IVec<TExtendedPhase, double>, IVecDownDimension<TPhase>
{
    public double t_0 => Entries[0].Last;
    public TPhase x_0 => Entries[0].Down();
    public double Interval => Entries[^1].Last - t_0;
    
    private readonly TExtendedPhase[] Entries;
    
    public IntegralCurve(TExtendedPhase[] entries)
    {
        Entries = entries;
    }

    public Orbit<TPhase> Orbit()
    {
        return new Orbit<TPhase>(Entries.Select(s => s.Down()).ToArray());
    }
}

public readonly struct Orbit<TPhase>
{
    private readonly TPhase[] Entries;
    
    public Orbit(TPhase[] entries)
    {
        Entries = entries;
    }
}*/

public interface IIntegrator<TPhase, TSpace> where TPhase : IVec<TPhase, double>
{
    TPhase Integrate(IVectorField<TPhase, TSpace> f, TPhase x, double dt);
}

public static class IVectorFieldExtensions
{
    extension<TInput, TOutput>(IVectorField<TInput, TOutput> v) where TInput : IVec<TInput, double>
    {
        
        public ArbitraryField<TInput, D> Select<D>(Func<TOutput, D> selector)
        {
            return new ArbitraryField<TInput, D>(v.Domain, p => selector(v.Evaluate(p)));
        }
    }
}
public static class IIntegratorExtensions
{
    extension<TInput, TOutput>(IIntegrator<TInput, TOutput>)
        where TInput : IVec<TInput, double>, IVecDownDimension<TOutput>
        where TOutput : IVec<TOutput, double>, IVecUpDimension<TInput>
    {
        public static IIntegrator<TInput, TOutput> Rk4 => RungeKutta4IntegratorGen<TInput, TOutput>.Instance;
    }

    extension<TInput>(IIntegrator<TInput, TInput>)
        where TInput : IVec<TInput, double>
    {
        public static IIntegrator<TInput, TInput> Rk4Steady => new RungeKutta4IntegratorBaseGen<TInput>();
    }
}

public class IncreasedDimensionVectorField<VecLowerIn, VecLowerOut, VecOut> : IVectorField<VecLowerIn, VecOut>
    where VecLowerIn : IVec<VecLowerIn, double>
    where VecLowerOut : IVecUpDimension<VecOut>
{
    private IVectorField<VecLowerIn, VecLowerOut> UnsteadyField;
    private readonly IVectorField<VecLowerIn, double> final;
    public IDomain<VecLowerIn> Domain { get; set; }

    public IncreasedDimensionVectorField(IVectorField<VecLowerIn, VecLowerOut> unsteadyField, IVectorField<VecLowerIn, double> final)
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