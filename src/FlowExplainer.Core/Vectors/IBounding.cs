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
        var min = t_start;
        var size = t_period;
        var oriLast = x.Last;
        var bounded = Original.Bound(x);
        bounded[bounded.ElementCount - 1] = PeriodicBound(oriLast, min, size);
        return bounded;
    }
    private static double PeriodicBound(double x, double min, double size)
    {
        var t = (x - min) % size;
        if (t < 0)
            t += size;
        var newT = t + min;
        return newT;
    }
}

public class LastSliceBounding<Vec> : IBounding<Vec> where Vec : IVec<Vec>
{
    private IBounding<Vec> Original;
    private double sliceStart;
    private double sliceEnd;
    private double new_t_start;
    private double new_t_end;

    public LastSliceBounding(IBounding<Vec> original, double slice_start, double slice_end, double newTStart, double newTEnd)
    {
        Original = original;
        this.sliceStart = slice_start;
        this.sliceEnd = slice_end;
        new_t_start = newTStart;
        new_t_end = newTEnd;
    }


    public Vec Bound(Vec x)
    {
        //oldrange = 0..7
        //newrange = 0..1

        //slice= 5..6
        //newT = 0..1
        //5.5 - 5 = .5 / (end-start)


        //input = new_t_start..new_t_end
        //output = 
        var bounded = Original.Bound(x);
        var oriT = bounded[bounded.ElementCount - 1];
        var sliceRelativeT = (oriT - sliceStart) / (sliceEnd - sliceStart);
        sliceRelativeT = sliceRelativeT % (sliceEnd - sliceStart);
        var newT = sliceRelativeT * (new_t_end - new_t_start) + new_t_start;
        bounded[bounded.ElementCount - 1] = newT;
        return bounded;
    }
}