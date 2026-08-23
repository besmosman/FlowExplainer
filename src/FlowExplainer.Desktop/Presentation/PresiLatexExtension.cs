using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public static class PresiLatexExtension
{
    private static Dictionary<string, Texture> latexTextures = new();

    
    extension(PresiContext presi)
    {
        public void LatexCentered(string latex, Vec2 center, double lh, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber]
            int lineNumber = 0)
        {
            var name = $"latex_{latex.GetDeterministicHashCode()}.png";
            if (!Directory.Exists("temp"))
                Directory.CreateDirectory("temp");

            if (!latexTextures.TryGetValue(latex, out var txtr))
            {
                if (!File.Exists($"temp/{name}"))
                    RenderLatexToPng(latex, $"temp/{name}");
                
                txtr = new ImageTexture($"temp/{name}")
                {
                    TextureMagFilter = TextureMagFilter.Linear,
                    TextureMinFilter = TextureMinFilter.Linear,
                };
                latexTextures.Add(latex, txtr);
            }

            var ratio = txtr.Size.ToVec2();
            ratio /= ratio.Y;
            presi.Image(txtr, center, lh * ratio.X / 2, filePath, lineNumber);
        }
    }

    public static void RenderLatexToPng(string latex, string outPng)
    {
        string texFile = "formula.tex";

        File.WriteAllText(texFile, @$"
\documentclass[preview, border=3pt]{{standalone}}
\usepackage[a4paper, left=1.65in, right=1.65in, top=1.3in, bottom=2in]{{geometry}}
\usepackage{{amsmath}}
\begin{{document}}
{latex}
\end{{document}}
");

        Run("latex", "formula.tex");
        Run("dvipng", "-D 500 -T tight -bg Transparent -fg White formula.dvi -o " + outPng);
    }

    static void Run(string exe, string args)
    {
        Process p = new Process();
        p.StartInfo.FileName = exe;
        p.StartInfo.Arguments = args;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.CreateNoWindow = true;

        p.Start();
        while (true)
        {
            p.WaitForExit(100);
            if (p.HasExited)
            {
                if (p.ExitCode != 0)
                    throw new Exception();
                else return;
            }
        }
    }
    
    //https://stackoverflow.com/questions/41183507/is-there-a-way-to-make-net-core-gethashcode-deterministic
    static int GetDeterministicHashCode(this string str)
    {
        unchecked
        {
            int hash1 = (5381 << 16) + 5381;
            int hash2 = hash1;

            for (int i = 0; i < str.Length; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1)
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }

            return hash1 + (hash2 * 1566083941);
        }
    }
}