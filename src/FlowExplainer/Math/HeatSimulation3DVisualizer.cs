using System.Globalization;
using System.Numerics;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;

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
                var t = float.Parse(tString, CultureInfo.InvariantCulture);
                var t_index = (int)float.Round(t * 100);

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
                        var v = float.Parse(splitted[y], CultureInfo.InvariantCulture);
                        if (isRealFile)
                            at = new Complex(v, at.Imaginary);
                        else
                            at = new Complex(at.Real, v);
                    }
                }
            }
        }


        var grids = spectralGrids.OrderBy(o =>
        {
            var t = (int)(float.Round(float.Parse(o.Key, CultureInfo.InvariantCulture) * 100f));
            return t;
        }).Select(s => s.Value).ToArray();
        
        var grid = new RegularGrid<Vec3i, Complex>(new Vec3i(grids[0].GridSize.X, grids[0].GridSize.Y, grids.Length));
        for (int i = 0; i < grids.Length; i++)
        {
            for (int x = 0; x < grids[i].GridSize.X; x++)
            {
                for (int y = 0; y < grids[i].GridSize.Y; y++)
                {
                    grid[new Vec3i(x, y, i)] = grids[i][new Vec2i(x, y)];
                }
            }
        }


        return new SpectralField(grid, new Rect<Vec3>(new Vec3(0, 0, 0), new Vec3(1, .5f, 1)));
    }
}

public static class SpeetjensSpectralImporter
{
    public static float PhysicalSpectral(float x, float Pi5)
    {
        return 2 * x / Pi5 - 1;
    }

    public static float InterpFourCheb(Vec2 pos, RegularGrid<Vec2i, Complex> Usp, float Pi5)
    {
        int N = Usp.GridSize.X - 1;
        int M = 2 * (Usp.GridSize.Y - 1);
        Vec2 spectralPos = new Vec2(pos.X * float.Pi, PhysicalSpectral(pos.Y, Pi5));
        float acosy = float.Acos(spectralPos.Y);
        var exp = Complex.Exp(Complex.ImaginaryOne * spectralPos.X);
        Complex u = Complex.Zero;
        for (int p = 0; p <= N; p++)
        {
            int n = p;
            u += Usp.AtCoords(new Vec2i(p, 0)).Real * float.Cos(n * acosy);
        }

        for (int p = 0; p <= N; p++)
        for (int k = 1; k < M / 2; k++)
        {
            var n = p;
            var m = k;
            var dU = Usp.AtCoords(new Vec2i(p, k)) * Complex.Pow(exp, -m) * float.Cos(n * acosy);
            u += dU + Complex.Conjugate(dU);
        }

        for (int n = 0; n <= N; n++)
        {
            u += Usp.AtCoords(new Vec2i(n, M / 2)) * Complex.Pow(exp, (M / 2f)) * float.Cos(n * acosy);
        }

        return (float)u.Real;
    }

    public static RegularGridVectorField<Vec3, Vec3i, float> Load(string folderPath)
    {
        int M = 33;
        int N = 33;
        int T = 103;

        int Nx = 30; //interpolation grid size???
        int Ny = 30;

        float D = 0.5f;
        float Pi5 = D; // probaly Domain in Y axis right?


        Dictionary<string, RegularGrid<Vec2i, Complex>> spectralGrids = new();


        foreach (var p in Directory.GetFiles(folderPath))
        {
            if (!p.Contains("NoFlow"))
            {
                var range = (p.IndexOf("t=", StringComparison.InvariantCulture) + 2)..(p.IndexOf("EPS", StringComparison.InvariantCulture));
                string tString = p[range];
                var t = float.Parse(tString, CultureInfo.InvariantCulture);
                var t_index = (int)float.Round(t * 100);

                if (!spectralGrids.ContainsKey(tString))
                {
                    spectralGrids.Add(tString, new(new Vec2i(M, N)));
                }

                var spectralGrid = spectralGrids[tString];

                var dat = File.ReadLines(p).ToArray();

                bool isRealFile = p.EndsWith("RE.dat");
                for (int x = 0; x < M; x++)
                {
                    var splitted = dat[x].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    for (int y = 0; y < N; y++)
                    {
                        ref var at = ref spectralGrid.AtCoords(new Vec2i(x, y));
                        var v = float.Parse(splitted[y], CultureInfo.InvariantCulture);
                        if (isRealFile)
                            at = new Complex(v, at.Imaginary);
                        else
                            at = new Complex(at.Real, v);
                    }
                }
            }
        }


        Vec3i gridSize = new Vec3i(33, 33, 103);
        var min = new Vec3(0, 0, 0f);
        var max = new Vec3(1, .5f, 1.03f);
        RegularGridVectorField<Vec3, Vec3i, float> heatGrid = new(gridSize, min, max);
        Parallel.ForEach(spectralGrids, (p) =>
        {
            var t = (int)(float.Round(float.Parse(p.Key, CultureInfo.InvariantCulture) * 100f));
            var spectralGrid = p.Value;
            for (int x = 0; x < p.Value.GridSize.X; x++)
            {
                for (int y = 0; y < p.Value.GridSize.Y; y++)
                {
                    var pos = heatGrid.ToWorldPos(new Vec3(x, y, 0));
                    var h = InterpFourCheb(pos.XY, spectralGrid, Pi5);
                    heatGrid.Grid.AtCoords(new Vec3i(x, y, t)) = h;
                }
            }
        });
        return heatGrid;
    }
}

public class HeatSimulationToField
{
    public static RegularGridVectorField<Vec3, Vec3i, float> Convert(HeatSimulation simulation)
    {
        Vec3i gridSize = new Vec3i(64, 64, 64);
        Vec3 min = new Vec3(0, 0, simulation.States.First().Time);
        Vec3 max = new Vec3(1, .5f, simulation.States.Last().Time);
        RegularGridVectorField<Vec3, Vec3i, float> heatField = new(gridSize, min, max);
        foreach (var state in simulation.States)
        {
            for (int i = 0; i < state.ParticleX.Length; i++)
            {
                var x = state.ParticleX[i];
                var y = state.ParticleY[i];
                var h = state.ParticleHeat[i];
                var t = state.Time;
                var voxelCoord = heatField.ToVoxelCoord(new Vec3(x, y, t)).Round();
                ref var dat = ref heatField.Grid.AtCoords(voxelCoord);
                if (dat == 0)
                {
                    dat = h;
                }
                else
                {
                    dat = Utils.Lerp(dat, h, .5f);
                }
            }
        }

        return heatField;
    }
}

public class FDTest : WorldService
{
    public override void Initialize()
    {
        Temprature = new RegularGrid<Vec2i, float>(new Vec2i(64, 32));
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                if (Vec2.Distance(new Vec2(x, y), new Vec2(32, 16)) < 5f)
                {
                    Temprature.AtCoords(new Vec2i(x, y)) = 0;
                }
                else
                    Temprature.AtCoords(new Vec2i(x, y)) = 0;
            }
        }
    }

    private RegularGrid<Vec2i, float> Temprature;

    public override void Draw(RenderTexture rendertarget, View view)
    {
        var dat = GetRequiredWorldService<DataService>();
        var vel = dat.VectorField;

        for (int x = 0; x < 64; x++)
        {
            Temprature.AtCoords(new Vec2i(x, 0)) = 1;
            Temprature.AtCoords(new Vec2i(x, 31)) = 0;
        }


        for (int i = 0; i < 336; i++)
        {
            FiniteDifferences.Test(vel, Temprature, dat.SimulationTime, dat.DeltaTime / 336f);
        }

        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                Gizmos2D.Instanced.RegisterRectCenterd(new Vec2(x, y) / Temprature.GridSize.ToVec2() * new Vec2(1, .5f), new Vec2(1, 1) / Temprature.GridSize.ToVec2(), dat.ColorGradient.GetCached(Temprature.AtCoords(new Vec2i(x, y))));
            }
        }

        Gizmos2D.Instanced.RenderRects(view.Camera2D);
    }
}

public class HeatSimulation3DVisualizer : WorldService
{
    private HeatSimulation? loaded;
    private float heatfilterMax = 1;
    private float heatfilterMin;
    private float timeFilter;
    public override ToolCategory Category => ToolCategory.Heat;

    public override void Initialize()
    {
    }

    private void LoadHeatSim()
    {
        loaded = BinarySerializer.Load<HeatSimulation>("heat.sim");
    }


    public override void DrawImGuiEdit()
    {
        if (loaded.HasValue)
        {
            ImGuiHelpers.SliderFloat("Heat Filter Min", ref heatfilterMin, 0, 1);
            ImGuiHelpers.SliderFloat("Heat Filter Max", ref heatfilterMax, heatfilterMin, 1);
            var min = loaded.Value.States[0].Time;
            var max = loaded.Value.States.Last().Time;
            ImGuiHelpers.SliderFloat("Time Filter", ref timeFilter, min, max);
        }

        if (ImGui.Button("Load heat.sim"))
        {
            LoadHeatSim();
        }

        base.DrawImGuiEdit();
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        if (!view.Is3DCamera)
            return;

        var dat = GetRequiredWorldService<DataService>();

        if (!loaded.HasValue)
            return;
        float rad = .01f;
        view.CameraOffset = -dat.VectorField.Domain.Boundary.Center;
//            view.CameraOffset = new Vec3(-.5f, .25f, -.25f);

        GL.Enable(EnableCap.DepthTest);
        for (int index = 0; index < loaded.Value.States.Length; index++)
        {
            var state = loaded.Value.States[index];
            /*if(Math.Abs(state.Time - (Math.Sin(FlowExplainer.Time.TotalSeconds)*1 - 1f)) > .1f)
                continue;*/
            if (timeFilter < state.Time)
                continue;

            var grad = dat.ColorGradient;

            for (int i = 0; i < state.ParticleX.Length; i++)
            {
                var pos = new Vec3(state.ParticleX[i], state.ParticleY[i], -index * rad);
                if (state.ParticleHeat[i] > heatfilterMin && state.ParticleHeat[i] < heatfilterMax)
                {
                    Gizmos.Instanced.RegisterSphere(pos, rad, grad.GetCached(state.ParticleHeat[i]));
                }
            }
        }


        Gizmos.Instanced.DrawSpheres(view.Camera);
    }
}