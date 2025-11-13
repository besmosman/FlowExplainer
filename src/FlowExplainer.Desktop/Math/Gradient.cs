using System.Numerics;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class ColorGradient : Gradient<Color>
{
    public Lazy<Texture> Texture { get; private set; }
    public string Name;

    public ColorGradient(string name, (double, Color)[] entries) : base(entries)
    {
        Name = name;
        Texture = new Lazy<Texture>(() =>
        {
            Color[] pixels = new Color[256];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Get((double)i / (pixels.Length - 1f));
            }

            return new RgbArrayTexture(pixels.Length, 1, pixels)
            {
                TextureMagFilter = TextureMagFilter.Linear,
                TextureMinFilter = TextureMinFilter.Linear,
            };
        }, LazyThreadSafetyMode.None);
    }
}

public class Gradient<T> where T : IMultiplyOperators<T, double, T>, IAdditionOperators<T, T, T>
{
    private (double time, T value)[] entries;
    private T[] Cached;
    public static int CachedSize = 1024;

    public Gradient((double, T)[] entries)
    {
        this.entries = entries;

        Cached = new T[CachedSize];
        for (int i = 0; i < CachedSize; i++)
        {
            Cached[i] = Get(i / (double)CachedSize);
        }
    }


    public T GetCached(double t)
    {
        return Cached[int.Clamp((int)double.Round(t * CachedSize), 0, CachedSize - 1)];
    }

    public T Get(double t)
    {
        t = double.Clamp(t, 0, 1);
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