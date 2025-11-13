using MemoryPack;

namespace FlowExplainer;

public interface IBounding<Vec> where Vec : IVec<Vec>
{
    Vec Bound(Vec x);
}


public class LastPeriodicBounding<Vec> : IBounding<Vec> where Vec : IVec<Vec>
{
    private IBounding<Vec> Original;
    private double t_start;
    private double t_period;
    
    public LastPeriodicBounding(IBounding<Vec> original, double tStart, double tPeriod)
    {
        Original = original;
        t_start = tStart;
        t_period = tPeriod;
    }
    public Vec Bound(Vec x)
    {
        var oriLast = x.Last;
        var bounded = Original.Bound(x);
        bounded[bounded.ElementCount - 1] = ((oriLast - t_start) % t_period) + t_start;
        return bounded;
    }
}