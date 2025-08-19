using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer.Msdf;

public static class MsdfRenderer
{
    public static Material Material;
    public static Mesh textMesh = new Mesh(new Geometry(ExactArrayPool<Vertex>.Rent(0), []), true, true);
    public static Dictionary<int, MsdfFont> fonts = new();
    private static bool forceRegenerate = false;

    static MsdfRenderer()
    {
        Init();
    }

    public static MsdfFont GetClosestFont(ICamera cam, float lh)
    {
        var target = lh * 0;
        var minDis = float.MaxValue;
        MsdfFont minFont = null;
        foreach (var p in fonts)
        {
            if (minDis > float.Abs(p.Key - target))
            {
                minDis = float.Abs(p.Key - target);
                minFont = p.Value;
            }
        }

        return minFont!;
    }

    public static void Init()
    {
        Material = new Material(
            new Shader("Assets/Shaders/msdf.frag", ShaderType.FragmentShader),
            Shader.DefaultWorldSpaceVertex);

        int size = 96;
        string folderPath = "Assets/Fonts/OpenSans-Medium";
        string genFolderPath = $"{folderPath}/generated";
        if (Directory.Exists(genFolderPath))
        {
            string genCharsetFilePath = $"{genFolderPath}/charset.txt";
            string charsetFilePath = $"{folderPath}/charset.txt";
            File.Copy(charsetFilePath, genCharsetFilePath);
            Directory.Delete(genFolderPath, true);
        }
        GenererateFont(32);
    }

    private static void GenererateFont(int size)
    {
        string folderPath = "Assets/Fonts/OpenSans-Medium";
        string fontName = new DirectoryInfo(Path.GetDirectoryName(folderPath + "/")).Name;
        string genFolderPath = $"{folderPath}/generated";
        string ttfFilePath = $"{folderPath}/{fontName}.ttf";
        string genImagePath = $"{genFolderPath}/texture-{size}.png";
        string genInfoPath = $"{genFolderPath}/info-{size}.json";
        string md5Path = $"{genFolderPath}-{size}/.md5";


        if (!forceRegenerate && Directory.Exists(genFolderPath))
        {
            fonts.Add(size, new MsdfFont(JsonConvert.DeserializeObject<MsdfFontInfo>(File.ReadAllText(genInfoPath)))
            {
                Texture = new ImageTexture(genImagePath),
            });
            return;
        }

        string charsetFilePath = $"{folderPath}/charset.txt";

        var call = new StringBuilder();
        call.Append(" -font ");
        call.Append($"\"{ttfFilePath}\"");
        call.Append(" -charset ");
        call.Append($"\"{charsetFilePath}\"");
        call.Append(" -imageout \"");
        call.Append(genImagePath + "\"");
        call.Append(" -json \"" + genInfoPath + "\"");
        call.Append($" -size {size}");
        Directory.CreateDirectory(Path.GetRelativePath(Directory.GetCurrentDirectory(), genFolderPath));
        ProcessStartInfo psi = new()
        {
            WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
            Arguments = call.ToString(),
            FileName = "msdf-atlas-gen",
            CreateNoWindow = true
        };
        var process = Process.Start(psi);
        process.WaitForExit();

        fonts.Add(size, new MsdfFont(JsonConvert.DeserializeObject<MsdfFontInfo>(File.ReadAllText(genInfoPath)))
        {
            Texture = new ImageTexture(genImagePath),
        });
    }


    public static void Render()
    {
        textMesh.Draw();
    }

    public static float LastMaxPos = 0;

    public static float CalcTextWidth(MsdfFont font, string text)
    {
        float currentX = 0;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            var fontChar = font.GetGlyphInfo(c);
            if (fontChar == null)
                fontChar = font.GetGlyphInfo('?');

            currentX += fontChar.advance;
        }

        float m = 1f / font.MsdfFontInfo.Metrics.lineHeight;
        return currentX * m;
    }

    public static void UpdateMesh(string text, ICamera cam, MsdfFont font, bool centered = false)
    {
        var vertices = ExactArrayPool<Vertex>.Rent(text.Length * 6);
        float currentX = 0;
        bool invertY = !cam.InvertedY();

        var color = new Vec4(1, 1, 1, 1);
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            var fontChar = font.GetGlyphInfo(c);
            if (fontChar == null)
                fontChar = font.GetGlyphInfo('?');
            if (fontChar.atlasBounds != null)
            {
                float lh = font.MsdfFontInfo.Metrics.lineHeight;
                float baseHeight = font.MsdfFontInfo.Metrics.descender;

                Vec2 uvSize = new(1f / font.Texture.Size.X, 1f / font.Texture.Size.Y);

                var ab = fontChar.atlasBounds;
                var pb = fontChar.planeBounds;
                int maxY = font.MsdfFontInfo.Atlas.height;
                var uvLeftTop = new Vec2(ab.left, maxY - ab.bottom) * uvSize;
                var uvLeftBot = new Vec2(ab.left, maxY - ab.top) * uvSize;
                var uvRightTop = new Vec2(ab.right, maxY - ab.bottom) * uvSize;
                var uvRightBot = new Vec2(ab.right, maxY - ab.top) * uvSize;

                vertices[(i * 6) + 0] = new Vertex(new(currentX + pb.left, baseHeight + lh - pb.top), color, uvLeftBot);
                vertices[(i * 6) + 1] = new Vertex(new(currentX + pb.left, baseHeight + lh - pb.bottom), color, uvLeftTop);
                vertices[(i * 6) + 2] = new Vertex(new(currentX + pb.right, baseHeight + lh - pb.top), color, uvRightBot);
                vertices[(i * 6) + 3] = new Vertex(new(currentX + pb.right, baseHeight + lh - pb.bottom), color, uvRightTop);
                vertices[(i * 6) + 4] = new Vertex(new(currentX + pb.left, baseHeight + lh - pb.bottom), color, uvLeftTop);
                vertices[(i * 6) + 5] = new Vertex(new(currentX + pb.right, baseHeight + lh - pb.top), color, uvRightBot);

                if (invertY)
                {
                    var planeLeftTop = new Vec2(pb.left, pb.top);
                    var planeLeftBot = new Vec2(pb.left, pb.bottom);
                    var planeRightTop = new Vec2(pb.right, pb.top);
                    var planeRightBot = new Vec2(pb.right, pb.bottom);

                    vertices[(i * 6) + 0] = new Vertex(new Vec2(currentX, baseHeight) + planeLeftTop, color, uvLeftBot);
                    vertices[(i * 6) + 1] = new Vertex(new Vec2(currentX, baseHeight) + planeLeftBot, color, uvLeftTop);
                    vertices[(i * 6) + 2] = new Vertex(new Vec2(currentX, baseHeight) + planeRightTop, color, uvRightBot);
                    vertices[(i * 6) + 3] = new Vertex(new Vec2(currentX, baseHeight) + planeRightBot, color, uvRightTop);
                    vertices[(i * 6) + 4] = new Vertex(new Vec2(currentX, baseHeight) + planeLeftBot, color, uvLeftTop);
                    vertices[(i * 6) + 5] = new Vertex(new Vec2(currentX, baseHeight) + planeRightTop, color, uvRightBot);
                }
            }

            currentX += fontChar.advance;
        }

        if (centered)
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Position.X += -currentX / 2;
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].Normal.X = vertices[i].Position.X / currentX;
            vertices[i].Normal.Y = vertices[i].Position.Y / vertices[vertices.Length - 1].Position.Y;
        }

        float m = 1f / font.MsdfFontInfo.Metrics.lineHeight;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].Position = new Vec3(vertices[i].Position.X * m, vertices[i].Position.Y * m, 0);
        }

        LastMaxPos = currentX * m;

        var indices = new uint[vertices.Length];
        for (uint i = 0; i < indices.Length; i++)
        {
            indices[i] = i;
        }

        ExactArrayPool<Vertex>.Return(textMesh.Vertices);
        textMesh.Vertices = vertices;
        textMesh.Indices = indices;
        textMesh.Upload();
    }
}