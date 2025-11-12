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

    public IEnumerable<(T vector, float c)> Enumerate()
    {
        float t_start = Entries[0].Last;
        float t_end = Entries[^1].Last;
        foreach (var t in Entries)
        {
            yield return (t, (t.Last - t_start) / (t_end - t_start));
        }
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


    private int last_returned_Entry = 0;
    public T AtC(float c)
    {
        return AtTime(c * (Entries[^1].Last - Entries[0].Last) + Entries[0].Last);
    }
    
    public T AtTimeBilinear(float t)
    {
        int L = 0;
        int R = Entries.Length - 1;

        while (L <= R)
        {
            var m = L + (int)float.Floor((R - L) / 2f);
            if (Entries[m].Last < t)
            {
                L = m + 1;
            }
            else if (Entries[m].Last > t)
            {
                R = m + 1;
            }
            else return Entries[m];
        }
        float c = t - Entries[L].Last / (Entries[R].Last - Entries[L].Last);
        return Utils.Lerp(Entries[L], Entries[R], c);
    }

    public T AtTime(float t)
    {
        //So this should be fast for t queries that are increasing or decreasing.
        //First checking last result - 1 to count. so increasing queries only require 2 checks while
        //decreasing require one check. 

        int start = last_returned_Entry;
        for (int i = int.Max(0, last_returned_Entry - 1); i < Entries.Length - 1; i++)
        {
            if (Entries[i].Last <= t && Entries[i + 1].Last >= t)
            {
                var c = (t - Entries[i].Last) / (Entries[i + 1].Last - Entries[i].Last);
                last_returned_Entry = i;
                return Utils.Lerp(Entries[i], Entries[i + 1], c);
            }
        }

        if (t >= Entries[^1].Last)
            return Entries[^1];
        if (t <= Entries[0].Last)
            return Entries[0];

        for (int i = 0; i < start; i++)
        {
            if (Entries[i].Last <= t && Entries[i + 1].Last >= t)
            {
                var c = (t - Entries[i].Last) / (Entries[i + 1].Last - Entries[i].Last);
                last_returned_Entry = i;
                return Utils.Lerp(Entries[i], Entries[i + 1], c);
            }
        }

        throw new Exception();
    }
}