using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class RgbArrayTexture : ArrayTexture
{
    public readonly Color[] Pixels;
    public static RgbArrayTexture White1x1 => new RgbArrayTexture(1, 1, [new Color(1, 1, 1, 1)]);

    public RgbArrayTexture(int width, int height, Color[] pixels) : base(width, height)
    {
        this.TextureMinFilter = TextureMinFilter.Nearest;
        this.TextureMagFilter = TextureMagFilter.Nearest;

        Pixels = pixels;
        Upload();
    }

    protected override void TexImage2DImpl()
    {
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb32f, Size.X, Size.Y, 0, PixelFormat.Rgba, PixelType.Float, Pixels);
    }
}