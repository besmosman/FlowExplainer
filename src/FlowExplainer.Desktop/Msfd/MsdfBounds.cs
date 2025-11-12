using FlowExplainer;
using FlowExplainer;

namespace FlowExplainer.Msdf
{
    public class MsdfBounds
    {
        public float left;
        public float top;
        public float right;
        public float bottom;
    }

    public class MsdfGlyph
    {
        public MsdfBounds planeBounds;
        public MsdfBounds atlasBounds;
        public float advance;
        public int unicode;
    }

    public class MsdfMetrics
    {
        public float lineHeight;
        public float ascender;
        public float descender;
        public float underlineY;
        public float underlineThickness;
    }

    public class MsdfAtlas
    {
        public string type;
        public int height;
        public int width;
        public int size;
    }


    public class MsdfFontInfo
    {
        public MsdfAtlas Atlas;
        public MsdfMetrics Metrics;
        public List<MsdfGlyph> Glyphs;
    }
}