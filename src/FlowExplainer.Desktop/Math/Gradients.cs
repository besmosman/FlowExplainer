using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace FlowExplainer;

public static class Gradients
{
    private static Dictionary<string, Lazy<ColorGradient>> grads = new();

    static Gradients()
    {
        foreach (var f in Directory.GetFiles("Assets/Images/Colormaps"))
        {
            grads.Add(Path.GetFileNameWithoutExtension(f), new Lazy<ColorGradient>(() => LoadGradient(f)));
        }
        
    }

    public static IEnumerable<ColorGradient> All => grads.Select(s => s.Value.Value);

    public static ColorGradient Grayscale => GetGradient("grayscale");
    public static ColorGradient Parula => GetGradient("matlab_parula");
    public static ColorGradient GetGradient(string name) => grads[name].Value;

    private static ColorGradient LoadGradient(string path)
    {
        using var image = Image.Load<Rgba32>(path);
        var samples = 64;
        int padding = 2;
        var y = image.Height / 2;
        (float time, Color value)[] entries = new (float time, Color value)[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = i / (samples - 1f);
            var x = int.Clamp((int)float.Round(t * (image.Width - padding * 2) + padding), padding + 1, image.Width - padding - 1);
            var pixel = image[x, y];
            var color = new Color(pixel.R, pixel.G, pixel.B, pixel.A) / 255f;
            entries[i] = (t, color);
        }

        return new ColorGradient(Path.GetFileNameWithoutExtension(path).Replace("_", " "), entries);
    }
}