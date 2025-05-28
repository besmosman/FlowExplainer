using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace FlowExplainer
{
    public class ImageTexture : Texture
    {
        public readonly string FilePath;
        public bool AutoRefresh = true;
        private Rgba32[] pixels = Array.Empty<Rgba32>();

        public ImageTexture(string filePath) : base(0, 0)
        {
            FilePath = filePath;
            Configuration.Default.PreferContiguousImageBuffers = true;
            AssetWatcher.OnChange += OnAssetChange;
            RefreshSource();
            Upload();
        }

        private void OnAssetChange(FileSystemEventArgs obj)
        {
            if (AutoRefresh && obj.FullPath.StartsWith(Path.GetFullPath(FilePath)))
            {
                Thread.Sleep(100);
                RefreshSource();
                Upload();
            }
        }

        public void RefreshSource()
        {
            using var image = Image.Load<Rgba32>(FilePath);
            Size = new Vector2i(image.Width, image.Height);
            pixels = new Rgba32[image.Width * image.Height];
            image.CopyPixelDataTo(pixels);
        }

        public void Upload()
        {
            GL.BindTexture(TextureTarget.Texture2D, TextureHandle);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, Size.X, Size.Y, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
        }

        public override void Dispose()
        {
            base.Dispose();
            AssetWatcher.OnChange -= OnAssetChange;
        }
    }
}