using System.Numerics;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using ShaderType = OpenTK.Graphics.OpenGL4.ShaderType;

namespace FlowExplainer;

public static class SpeetjensSpectralImporter
{
    public static float PhysicalSpectral(float x, float Pi5)
    {
        return 2 * x / Pi5 - 1;
    }

    public static float InterpFourCheb(Vec2 pos, DataGrid<Vec2i, Complex> Usp, float Pi5)
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
            u += Usp.AtCoords(new Vec2i(p, 0)).Real * float.Cos(n * acosy); // Assuming 0-indexed for m=0       
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


        Dictionary<string, DataGrid<Vec2i, Complex>> spectralGrids = new();


        foreach (var p in Directory.GetFiles(folderPath))
        {
            if (!p.Contains("NoFlow"))
            {
                var range = (p.IndexOf("t=", StringComparison.InvariantCulture) + 2)..(p.IndexOf("EPS", StringComparison.InvariantCulture));
                string tString = p[range];
                var t = float.Parse(tString);
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
                        var v = float.Parse(splitted[y]);
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
            var t = (int)(float.Round(float.Parse(p.Key) * 100f));
            var spectralGrid = p.Value;
            for (int x = 0; x < p.Value.GridSize.X; x++)
            {
                for (int y = 0; y < p.Value.GridSize.Y; y++)
                {
                    var pos = heatGrid.ToWorldPos(new Vec3(x, y, 0));
                    var h = InterpFourCheb(pos.XY, spectralGrid, Pi5);
                    heatGrid.DataGrid.AtCoords(new Vec3i(x, y, t)) = h;
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
                ref var dat = ref heatField.DataGrid.AtCoords(voxelCoord);
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
        Temprature = new DataGrid<Vec2i, float>(new Vec2i(64, 32));
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

    private DataGrid<Vec2i, float> Temprature;

    public override void Draw(RenderTexture rendertarget, View view)
    {
        var dat = GetRequiredWorldService<DataService>();
        var vel = dat.VelocityField;

        for (int x = 0; x < 64; x++)
        {
            Temprature.AtCoords(new Vec2i(x, 0)) = 1;
            Temprature.AtCoords(new Vec2i(x, 31)) = 0;
        }


        for (int i = 0; i < 336; i++)
        {
            FiniteDifferences.Test(vel, Temprature, dat.SimulationTime, dat.DeltaTime/336f);
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

public class Heat3DViewer : WorldService
{
    private List<(Vec3, float h)> particles = new();
    private RegularGridVectorField<Vec3, Vec3i, float> heat3d;
    private StorageBuffer<float> StorageBuffer;

    private Material mat = new Material(Shader.DefaultWorldSpaceVertex, new Shader("Assets/Shaders/volume.frag", ShaderType.FragmentShader));

    public override void Initialize()
    {
        //   heat3d = HeatSimulationToField.Convert(BinarySerializer.Load<HeatSimulation>("heat.sim"));
        heat3d = SpeetjensSpectralImporter.Load("C:\\Users\\osman\\Downloads\\ScalarTransportBasicVersion\\ScalarTransportBasicVersion\\DataSet1");
        StorageBuffer = new StorageBuffer<float>(heat3d.GridSize.Volume());
        StorageBuffer.Data = heat3d.DataGrid.Data;
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        if (!view.Is3DCamera)
            return;

        // Update();

        var grad = GetRequiredWorldService<DataService>().ColorGradient;

        float c = (MathF.Sin((float)FlowExplainer.Time.TotalSeconds * .8f) + 1) / 2f;

        var th = .02f;

        GL.Enable(EnableCap.DepthTest);
        Gizmos.DrawLine(view, Vec3.Zero, new Vec3(1, 0, 0), th, new Color(1, 0, 0));
        Gizmos.DrawLine(view, Vec3.Zero, new Vec3(0, 1, 0), th, new Color(0, 1, 0));
        Gizmos.DrawLine(view, Vec3.Zero, new Vec3(0, 0, 1), th, new Color(0, 0, 1));
        
        GL.Disable(EnableCap.CullFace);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthMask(false);
        
        /*GL.Enable(EnableCap.DepthTest);
        GL.DepthMask(false);
        GL.DepthFunc(DepthFunction.Less);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);*/
        mat.Use();
        StorageBuffer.Use();
        StorageBuffer.Upload();
        mat.SetUniform("tint", Color.White);
        mat.SetUniform("cameraPosUni", view.Camera.Position);
        mat.SetUniform("volumeMin", heat3d.MinCellPos);
        mat.SetUniform("volumeMax", heat3d.MaxCellPos);
        mat.SetUniform("gridSize", heat3d.GridSize.ToVec3());
        mat.SetUniform("view", view.Camera.GetViewMatrix());
        mat.SetUniform("projection", view.Camera.GetProjectionMatrix());
        var size = heat3d.MaxCellPos - heat3d.MinCellPos;
        mat.SetUniform("model", Matrix4x4.CreateScale(size) * Matrix4x4.CreateTranslation(size / 2));
        mat.SetUniform("colorgradient", GetRequiredWorldService<DataService>().ColorGradient.Texture.Value);
        Gizmos.UnitCube.Draw();
        /*

        GL.DepthMask(true);
        GL.DepthFunc(DepthFunction.Less);*/

        /*
        for (int x = 0; x < heat3d.GridSize.X; x++)
        for (int y = 0; y < heat3d.GridSize.Y; y++)
        for (int t = 0; t < heat3d.GridSize.Z; t++)
        {
            var coords = new Vec3(x, y, t);
            var worldPos = heat3d.ToWorldPos(coords);

            if (heat3d.Evaluate(worldPos) > .4f)
                Gizmos.Instanced.RegisterSphere(new Vec3(worldPos.X, worldPos.Y, worldPos.Z), 1f / heat3d.GridSize.X / 1.4f, grad.GetCached(worldPos.Z));
        }
        */


     


        foreach (var p in particles)
        {
            var z = p.Item1;
            Gizmos.Instanced.RegisterSphere(z, .01f, grad.GetCached(p.h));
        }

        Gizmos.Instanced.DrawSpheres(view.Camera);
    }

    /*private void Update()
    {
        particles.Clear();
        //thresh *= .001f;
        for (int s = 1; s < loaded.Value.States.Length - 1; s++)
        {
            var states = loaded.Value.States;
            var state = states[s];
            for (int i = 0; i < state.ParticleHeat.Length; i++)
            {
                var flux = (states[s].ParticleHeat[i] - states[s - 1].ParticleHeat[i]) / (states[s].Time - states[s - 1].Time);
                var fluxNext = (states[s + 1].ParticleHeat[i] - states[s].ParticleHeat[i]) / (states[s + 1].Time - states[s].Time);
                var thresh = .8f;

                if (flux >= thresh
                   )
                {
                    particles.Add((new Vec3(states[s].ParticleX[i], states[s].ParticleY[i], states[s].Time), flux));
                }
            }
        }
    }*/
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
        view.CameraOffset = -dat.VelocityField.Domain.Size.Up(-(loaded.Value.States.Length * rad)) / 2;
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