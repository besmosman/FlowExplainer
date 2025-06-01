

using System.Numerics;

namespace FlowExplainer;

public class Gradient<T> where T : IMultiplyOperators<T, float, T>, IAdditionOperators<T, T, T>
{
    private (float time, T value)[] entries;
    private T[] Cached;

    public Gradient((float, T)[] entries)
    {
        this.entries = entries;

        Cached = new T[255];
        for (int i = 0; i < 255; i++)
        {
            Cached[i] = Get(i / 255f);
        }
    }


    public T GetCached(float t)
    {
        return Cached[int.Clamp((int)float.Round(t * 255f), 0, 254)];
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