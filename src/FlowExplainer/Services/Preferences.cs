using OpenTK.Mathematics;

namespace FlowExplainer
{
    public record struct Preferences
    {
        public Vec2i WindowSizeOnStartup;
        public float UIScale;
        public bool VSync;
    }
}