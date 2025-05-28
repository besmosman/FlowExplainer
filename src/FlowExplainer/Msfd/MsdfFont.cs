namespace FlowExplainer.Msdf;

public class MsdfFont
{
    public MsdfFontInfo MsdfFontInfo { get; set; }
    public Texture Texture { get; set; }
    public char[] Charset { get; set; }
    private Dictionary<char, MsdfGlyph> GlyphInfos = new();

    public MsdfFont(MsdfFontInfo msdfFontInfo)
    {
        MsdfFontInfo = msdfFontInfo;
        GlyphInfos = new Dictionary<char, MsdfGlyph>();
        foreach (var g in msdfFontInfo.Glyphs)
        {
            GlyphInfos.Add((char)g.unicode, g);
        }
    }

    public MsdfGlyph GetGlyphInfo(char c)
    {
        GlyphInfos.TryGetValue(c, out var glyph);
        return glyph;
    }
}