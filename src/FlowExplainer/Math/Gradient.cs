using System.Numerics;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class ColorGradient : Gradient<Color>
{
    public Lazy<Texture> Texture { get; private set; }

    public ColorGradient((float, Color)[] entries) : base(entries)
    {
        Texture = new Lazy<Texture>(() =>
        {
            Vec3[] pixels = new Vec3[256];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Get((float)i / (pixels.Length - 1f)).RGB;
            }

            return new RgbArrayTexture(pixels.Length, 1, pixels)
            {
                TextureMagFilter = TextureMagFilter.Linear,
                TextureMinFilter = TextureMinFilter.Linear,
            };
        }, LazyThreadSafetyMode.None);
    }
}

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
        return Cached[int.Clamp((int)float.Round(t * CachedSize), 0, CachedSize - 1)];
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