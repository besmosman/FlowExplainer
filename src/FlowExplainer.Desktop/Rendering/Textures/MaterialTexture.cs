using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer
{
    public class MaterialTexture
    {
        public int TextureIndex;
        public readonly TextureUnit TextureUnit;

        public MaterialTexture(int textureIndex, TextureUnit textureUnit)
        {
            TextureIndex = textureIndex;
            TextureUnit = textureUnit;
        }
    }
}