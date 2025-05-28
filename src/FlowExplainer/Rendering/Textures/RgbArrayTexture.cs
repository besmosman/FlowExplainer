using System.Numerics;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class RgbArrayTexture : ArrayTexture
{
    public readonly Vector3[] Pixels;

    public RgbArrayTexture(int width, int height, Vector3[] pixels) : base(width, height)
    {
        this.TextureMinFilter = TextureMinFilter.Nearest;
        this.TextureMagFilter = TextureMagFilter.Nearest;

        Pixels = pixels;
        Upload();
    }

    protected override void TexImage2DImpl()
    {
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb32f, Size.X, Size.Y, 0, PixelFormat.Rgb, PixelType.Float, Pixels);
    }
}