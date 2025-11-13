using FlowExplainer;
using FlowExplainer;

namespace FlowExplainer.Msdf
{
    public class MsdfBounds
    {
        public double left;
        public double top;
        public double right;
        public double bottom;
    }

    public class MsdfGlyph
    {
        public MsdfBounds planeBounds;
        public MsdfBounds atlasBounds;
        public double advance;
        public int unicode;
    }

    public class MsdfMetrics
    {
        public double lineHeight;
        public double ascender;
        public double descender;
        public double underlineY;
        public double underlineThickness;
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