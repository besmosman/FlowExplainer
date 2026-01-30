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
            if (!Directory.Exists("temp"))
                Directory.CreateDirectory("temp");
            if (!latexTextures.TryGetValue(latex, out var txtr))
            {
                RenderLatexToPng(latex, "temp/latex.png");
                txtr = new ImageTexture("temp/latex.png")
                {
                    TextureMagFilter = TextureMagFilter.Linear,
                    TextureMinFilter = TextureMinFilter.Linear,
                };
                latexTextures.Add(latex, txtr);
            }

            var ratio = txtr.Size.ToVec2();
            ratio /= ratio.Y;
            presi.Image(txtr, center, lh * ratio.X/2, filePath, lineNumber);
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
        Run("dvipng", "-D 260 -T tight -bg Transparent -fg White formula.dvi -o " + outPng);
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
}