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

        public const int Samples = 4;

        public RenderTexture(int width, int height, bool depth = true, bool multisample = false) : base(width, height, true)
        {
            IsMultisampled = multisample;
            FramebufferHandle = GL.GenFramebuffer();

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
        }

        public void DrawTo(Action action)
        {
            int[] oldViewport = new int[4];
            GL.GetInteger(GetPName.Viewport, oldViewport);
            GL.GetInteger(GetPName.FramebufferBinding, out int previouslyBound);

            Activate();
            action();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, previouslyBound);
            GL.Viewport(oldViewport[0], oldViewport[1], oldViewport[2], oldViewport[3]);
        }

        public void Activate()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FramebufferHandle);
            GL.Viewport(0, 0, Size.X, Size.Y);
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
            GL.ReadPixels(0, 0, Size.X, Size.Y, PixelFormat.Bgra, PixelType.UnsignedByte, bytes);
            for (int i = 0; i < bytes.Length; i += 4)
            {
                bytes[i+3] = 255;
            }
        }
        
        public static void Blit(RenderTexture source, RenderTexture dest)
        {
            GL.GetInteger(GetPName.FramebufferBinding, out int oldId);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, source.FramebufferHandle);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, dest.FramebufferHandle);
            GL.BlitFramebuffer(0, 0, dest.Size.X, dest.Size.Y, 0, 0, dest.Size.X, dest.Size.Y, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, oldId);
        }
        
        public static void SaveToFile(string path, Vec2i Size, int scaler = 1)
        {
            GL.GetInteger(GetPName.FramebufferBinding, out int currentframebuffer);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            var bytes = ArrayPool<byte>.Shared.Rent(Size.X * Size.Y * 4);
            ReadAllPixels(bytes, Size);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            //Task.Run(() =>
            //{

            var image = Image.LoadPixelData<Bgra32>(bytes, Size.X, Size.Y);
            ArrayPool<byte>.Shared.Return(bytes);
            image.Mutate(x => x.Flip(FlipMode.Vertical));
            image.Save(path);
            if (scaler != 1)
            {
                image.Mutate(x => x.Resize(Size.X * scaler, Size.Y * scaler, KnownResamplers.NearestNeighbor, false));
            }
            image.Save(path);
            image.Dispose();
            //});
        }
    }
}