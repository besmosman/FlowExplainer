using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer
{
    public class MaterialTexture
    {
        public Texture Texture;
        public readonly TextureUnit TextureUnit;

        public MaterialTexture(Texture texture, TextureUnit textureUnit)
        {
            Texture = texture;
            TextureUnit = textureUnit;
        }
    }
}