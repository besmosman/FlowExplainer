using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FlowExplainer
{
    public class Texture : IDisposable
    {
        public int TextureHandle;
        public Vector2i Size { get; protected set; }
        public TextureMinFilter TextureMinFilter = TextureMinFilter.Nearest;
        public TextureMagFilter TextureMagFilter = TextureMagFilter.Linear;
        public TextureTarget TextureTarget = TextureTarget.Texture2D;

        public Texture(int width, int height, bool skipGeneration = false)
        {
            Size = new Vector2i(width, height);
            if (!skipGeneration)
            {
                TextureHandle = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, TextureHandle);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, Size.X, Size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            }
        }

        public virtual void Dispose()
        {
            GL.DeleteTexture(TextureHandle);
        }
    }
}