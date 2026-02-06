using System.Net.Http.Headers;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FlowExplainer
{

    public class Texture3D : IDisposable
    {
        public int TextureHandle;
        public readonly Color[] Pixels;
        public Vec3i Size { get; protected set; }
        public TextureTarget TextureTarget = TextureTarget.Texture3D;

        private Color temp;
        
        public ref Color GetPixelAt(Vec3i p)
        {
            return ref GetPixelAt(p.X, p.Y, p.Z);
        }
        public ref Color GetPixelAt(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0 || x >= Size.X || y >= Size.Y || z >= Size.Z)
                return ref temp;
            return ref Pixels[z * Size.X * Size.Y + y * Size.X + x];
        }

        public Texture3D(Vec3i size)
        {
            Size = size;
            Pixels = new Color[size.Volume()];
            TextureHandle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture3D, TextureHandle);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexImage3D(TextureTarget.Texture3D, 0, PixelInternalFormat.Rgba32f, Size.X, Size.Y, Size.Z, 0, PixelFormat.Rgba, PixelType.Float, Pixels);
        }

        public void UpdateData()
        {
            GL.TextureSubImage3D(TextureHandle, 0, 0, 0, 0, Size.X, Size.Y, Size.Z, PixelFormat.Rgba, PixelType.Float, Pixels);
        }

        public void Dispose()
        {
            GL.DeleteTexture(TextureHandle);
        }
    }

    public class Texture : IDisposable
    {
        public int TextureHandle;
        public Vec2i Size { get; protected set; }
        public TextureMinFilter TextureMinFilter = TextureMinFilter.Nearest;
        public TextureMagFilter TextureMagFilter = TextureMagFilter.Linear;
        public TextureTarget TextureTarget = TextureTarget.Texture2D;

        public static Texture White1x1 => RgbArrayTexture.White1x1;

        public Texture(int width, int height, bool skipGeneration = false)
        {
            Size = new Vec2i(width, height);
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