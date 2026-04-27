using System.Buffers;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace FlowExplainer
{
    public class RenderTexture : Texture
    {
        public readonly int FramebufferHandle;
        public readonly int? DepthTextureHandle;
        public bool IsMultisampled;

        public const int Samples = 8;

        private static Stack<RenderTexture> targets = new();

        public RenderTexture(int width, int height, bool depth = true, bool multisample = false) : base(width, height, true)
        {
            IsMultisampled = multisample;
            FramebufferHandle = GL.GenFramebuffer();
            TextureMinFilter = TextureMinFilter.Linear;
            TextureMagFilter = TextureMagFilter.Linear;
            if (IsMultisampled)
            {
                TextureHandle = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2DMultisample, TextureHandle);
                GL.TexParameter(TextureTarget.Texture2DMultisample, TextureParameterName.TextureMinFilter, (int)TextureMinFilter);
                GL.TexParameter(TextureTarget.Texture2DMultisample, TextureParameterName.TextureMagFilter, (int)TextureMagFilter);
                GL.TexParameter(TextureTarget.Texture2DMultisample, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
                GL.TexParameter(TextureTarget.Texture2DMultisample, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
                GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, Samples, PixelInternalFormat.Rgba8, Size.X, Size.Y, true);
            }
            else
            {
                TextureHandle = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, TextureHandle);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, Size.X, Size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            }

            if (IsMultisampled)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, FramebufferHandle);
                GL.FramebufferTexture2D(
                    FramebufferTarget.Framebuffer,
                    FramebufferAttachment.ColorAttachment0,
                    TextureTarget.Texture2DMultisample,
                    TextureHandle,
                    0);
            }
            else
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, FramebufferHandle);
                GL.FramebufferTexture2D(
                    FramebufferTarget.Framebuffer,
                    FramebufferAttachment.ColorAttachment0,
                    TextureTarget.Texture2D,
                    TextureHandle,
                    0);
            }

            if (depth)
            {
                if (IsMultisampled)
                {
                    DepthTextureHandle = GL.GenTexture();
                    GL.BindTexture(TextureTarget.Texture2DMultisample, DepthTextureHandle.Value);
                    GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, Samples, PixelInternalFormat.DepthComponent, Size.X, Size.Y, true);
                    GL.FramebufferTexture2D(
                        FramebufferTarget.Framebuffer,
                        FramebufferAttachment.DepthAttachment,
                        TextureTarget.Texture2DMultisample,
                        DepthTextureHandle.Value,
                        0);
                }
                else
                {
                    DepthTextureHandle = GL.GenTexture();
                    GL.BindTexture(TextureTarget.Texture2D, DepthTextureHandle.Value);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, Size.X, Size.Y, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
                    GL.FramebufferTexture2D(
                        FramebufferTarget.Framebuffer,
                        FramebufferAttachment.DepthAttachment,
                        TextureTarget.Texture2D,
                        DepthTextureHandle.Value,
                        0);
                }
            }
            else
                DepthTextureHandle = null;

            var result = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (result != FramebufferErrorCode.FramebufferComplete)
                throw new Exception($"Could not create RenderTexture: {result}");
            RetargetCurrent();
        }

        public void DrawTo(Action action)
        {
            targets.Push(this);
            Activate();
            action();
            targets.Pop();
            RetargetCurrent();
        }
        private static void RetargetCurrent()
        {

            if (targets.Count == 0)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                var windowSize = FlowExplainer.Instance.GetGlobalService<WindowService>().Window.Size;
                GL.Viewport(0, 0, windowSize.X, windowSize.Y);
            }
            else
            {
                targets.Peek().Activate();
            }
        }

        public void Activate()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FramebufferHandle);
            GL.Viewport((int)0, 0, (int)Size.X, (int)Size.Y);
        }

        public void Resize(int width, int height)
        {
            Size = new Vec2i(width, height);
            GL.BindTexture(IsMultisampled ? TextureTarget.Texture2DMultisample : TextureTarget.Texture2D, TextureHandle);
            if (IsMultisampled)
                GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, Samples, PixelInternalFormat.Rgba32f, Size.X, Size.Y, false);
            else
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, Size.X, Size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

            if (DepthTextureHandle.HasValue)
            {
                GL.BindTexture(IsMultisampled ? TextureTarget.Texture2DMultisample : TextureTarget.Texture2D, DepthTextureHandle.Value);
                if (IsMultisampled)
                    GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, Samples, PixelInternalFormat.DepthComponent, Size.X, Size.Y, false);
                else
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, Size.X, Size.Y, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            GL.DeleteFramebuffer(FramebufferHandle);
            if (DepthTextureHandle.HasValue)
                GL.DeleteTexture(DepthTextureHandle.Value);
        }

        public static void ReadAllPixels(byte[] bytes, Vec2i Size)
        {
            GL.ReadPixels(0, 0, Size.X, Size.Y, PixelFormat.Bgra, PixelType.Float, bytes);
            for (int i = 0; i < bytes.Length; i += 4)
            {
                bytes[i + 3] = 255;
            }
        }

        public static void Blit(RenderTexture source, RenderTexture dest)
        {
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, source.FramebufferHandle);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, dest.FramebufferHandle);
            GL.BlitFramebuffer(0, 0, dest.Size.X, dest.Size.Y, 0, 0, dest.Size.X, dest.Size.Y, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
            RetargetCurrent();
        }

        public void SaveToFile(string path, Vec2i size, int scaler = 1)
        {
            DrawTo(() =>
            {
                int pixelCount = size.X * size.Y;
                float[] floats = ArrayPool<float>.Shared.Rent(pixelCount * 4);

                GL.ReadPixels(0, 0, size.X, size.Y, PixelFormat.Rgba, PixelType.Float, floats);

                var pixels = new Rgba32[pixelCount];
                for (int i = 0; i < pixelCount; i++)
                {
                    pixels[i] = new Rgba32(
                        Math.Clamp(floats[i * 4 + 0], 0f, 1f),
                        Math.Clamp(floats[i * 4 + 1], 0f, 1f),
                        Math.Clamp(floats[i * 4 + 2], 0f, 1f),
                        1f
                    );
                }

                ArrayPool<float>.Shared.Return(floats);

                var image = Image.LoadPixelData<Rgba32>(pixels, size.X, size.Y);
                image.Mutate(x => x.Flip(FlipMode.Vertical));


                /*
                var wrapped = new Image<Rgba32>(size.X * 2, size.Y);
                int w = image.Size.Width;
                for (int x = 0; x < wrapped.Size.Width; x++)
                for (int y = 0; y < wrapped.Size.Height; y++)
                {
                    wrapped[x, y] = image[((x + w / 2) % w + w) % w, y];
                }*/

                if (scaler != 1)
                    image.Mutate(x => x.Resize(size.X * scaler, size.Y * scaler,
                        KnownResamplers.NearestNeighbor, false));

                image.Save(path);
                image.Dispose();
            });
        }
    }
}