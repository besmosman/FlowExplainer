using System.Globalization;
using System.Numerics;
using Microsoft.VisualBasic;

namespace FlowExplainer;

public static class SpeetjensSpectralImporterSpectral
{
    public static SpectralField Load(string folderPath, bool noFlow)
    {
        int coefX = 33;
        int coefY = 33;

        Dictionary<string, RegularGrid<Vec2i, Complex>> spectralGrids = new();

        foreach (var p in Directory.GetFiles(folderPath))
        {
            if (p.Contains("NoFlow") == noFlow)
            {
                var range = (p.IndexOf("t=", StringComparison.InvariantCulture) + 2)..(p.IndexOf("EPS", StringComparison.InvariantCulture));
                string tString = p[range];
                var t = double.Parse(tString, CultureInfo.InvariantCulture);

                if (!spectralGrids.ContainsKey(tString))
                {
                    spectralGrids.Add(tString, new(new Vec2i(coefX, coefY)));
                }

                var spectralGrid = spectralGrids[tString];

                var dat = File.ReadLines(p).ToArray();

                bool isRealFile = p.EndsWith("RE.dat");
                for (int x = 0; x < coefX; x++)
                {
                    var splitted = dat[x].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    for (int y = 0; y < coefY; y++)
                    {
                        ref var at = ref spectralGrid.AtCoords(new Vec2i(x, y));
                        var v = double.Parse(splitted[y], CultureInfo.InvariantCulture);
                        if (isRealFile)
                            at = new Complex(v, at.Imaginary);
                        else
                            at = new Complex(at.Real, v);
                    }
                }
            }
        }


        var grids = spectralGrids.Select(o =>
        {
            var t = double.Parse(o.Key, CultureInfo.InvariantCulture);
            return (t, o);
        }).OrderBy(o => o.t).ToArray();

        var stepSize = grids[1].t - grids[0].t;
        for (int i = 0; i < grids.Length; i++)
        {
            var diff = i * stepSize - grids[i].t;
            if (double.Abs(diff) > 0.00001f)
            {
                throw new Exception();
            }
        }

        var gs = grids.Select(s => s.o.Value).ToArray();

        var grid = new RegularGrid<Vec3i, Complex>(new Vec3i(gs[0].GridSize.X, gs[0].GridSize.Y, grids.Length));
        for (int i = 0; i < grids.Length; i++)
        {
            for (int x = 0; x < gs[i].GridSize.X; x++)
            {
                for (int y = 0; y < gs[i].GridSize.Y; y++)
                {
                    grid[new Vec3i(x, y, i)] = gs[i][new Vec2i(x, y)];
                }
            }
        }


        return new SpectralField(grid, new Rect<Vec3>(new Vec3(0, 0, 0), new Vec3(1, .5f, gs.Length * stepSize)));
    }
}