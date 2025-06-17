

using System.Numerics;

namespace FlowExplainer;

public class Gradient<T> where T : IMultiplyOperators<T, float, T>, IAdditionOperators<T, T, T>
{
    private (float time, T value)[] entries;
    private T[] Cached;
    public static int CachedSize = 1024;
    
    public Gradient((float, T)[] entries)
    {
        this.entries = entries;

        Cached = new T[CachedSize];
        for (int i = 0; i < CachedSize; i++)
        {
            Cached[i] = Get(i / (float)CachedSize);
        }
    }


    public T GetCached(float t)
    {
        return Cached[int.Clamp((int)float.Round(t * CachedSize), 0, CachedSize-1)];
    }

    public T Get(float t)
    {
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].Item1 <= t && entries[i + 1].Item1 >= t)
            {
                var lt = (t - entries[i].time) / (entries[i + 1].time - entries[i].time);
                var pre = entries[i].Item2;
                var next = entries[i + 1].Item2;
                return Utils.Lerp(pre, next, lt);
            }
        }

        throw new Exception();
    }
}