using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer
{
    public abstract class ArrayTexture : Texture
    {
        protected ArrayTexture(int width, int height) : base(width, height, false)
        {
        }

        public void Upload()
        {
            GL.BindTexture(TextureTarget.Texture2D, TextureHandle);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

            TexImage2DImpl();
        }

        protected abstract void TexImage2DImpl();
    }
}