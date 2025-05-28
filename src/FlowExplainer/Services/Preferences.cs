using OpenTK.Mathematics;

namespace FlowExplainer
{
    public record struct Preferences
    {
        public Vector2i WindowSizeOnStartup;
        public float UIScale;
        public bool VSync;
    }
}