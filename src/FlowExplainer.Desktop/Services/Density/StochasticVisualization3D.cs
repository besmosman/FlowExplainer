using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class DensityVisualization3D : WorldService
{
    public override void Initialize()
    {

    }
    public override void Draw(View view)
    {
        
    }
}

public class StochasticVisualization3D : WorldService
{
    struct Particle
    {
        public Vec3 Phase;
        public double TimeAlive;
    }


    public int Count = 1000;
    public double Radius = .01f;
    private Particle[] Particles;
    public double dt = .01f;
    public double maxAlpha = 1;
    public override string? Name { get; } = "Stochastic 3D";
    public override string? CategoryName { get; } = "Structure";
    public override string? Description { get; } = "Stochastic structures 3D version";
    public double ReseedChance = .01;
    public double FadeInFactor = 1.0 / 8.0;
    public bool reverse;
    public double depthScaling = 50;

    public bool VolumeRender;
    public bool UseRandomizedField = false;
    public IVectorField<Vec3, Vec3> vectorfield;
    private Material mat;
    private StorageBuffer<float> StorageBuffer;
    private RegularGrid<Vec3i, float> Grid;
    private RegularGrid<Vec3i, float> GridTemp;
    public double SmoothFactor = .3f;
    public int SmoothSteps = 0;
    private Vec3i VolumeGridSize = new Vec3i(32, 16, 32) * 4;
    public double lerpFactor = .8f;


    public Mode mode = Mode.Combined;

    public enum Mode
    {
        Forewords,
        Backwords,
        Combined,
    }

    public override void Initialize()
    {
        mat = new Material(Shader.DefaultWorldSpaceVertex, new Shader("Assets/Shaders/stochastic-volume.frag", ShaderType.FragmentShader));
        vectorfield = StructuredFlowGenerator.Generate3D();
        StorageBuffer = new StorageBuffer<float>(VolumeGridSize.Volume());
        Grid = new RegularGrid<Vec3i, float>(StorageBuffer.Data, VolumeGridSize);
        GridTemp = new RegularGrid<Vec3i, float>(StorageBuffer.Data, VolumeGridSize);
        Init();
    }
    private void Init()
    {
        var dat = GetRequiredWorldService<DataService>();

        Particles = new Particle[Count];
        foreach (ref var p in Particles.AsSpan())
        {
            p = new Particle()
            {
                Phase = Utils.Random(vectorfield.Domain.RectBoundary),
            };
        }
    }
    public double threshold;

    public override void DrawImGuiSettings()
    {
        ImGui.Checkbox("Use Random 3D Field", ref UseRandomizedField);
        ImGuiHelpers.SliderInt("Particle Count", ref Count, 1, 100000);
        ImGuiHelpers.Slider("Radius", ref Radius, .001f, .1f);
        //ImGui.Checkbox("Reverse", ref reverse);
        ImGuiHelpers.Slider("FadeIn Factor", ref FadeInFactor, 0, 1f);
        ImGuiHelpers.Slider("Reseed Chance", ref ReseedChance, 0, 1f);
        ImGuiHelpers.Slider("Max Alpha", ref maxAlpha, 0, 1f);
        ImGuiHelpers.Slider("dt", ref dt, .001f, .1f);

        ImGuiHelpers.Slider("Threshold", ref threshold, 0, 1);
        ImGui.Checkbox("Volume Render", ref VolumeRender);
        ImGuiHelpers.Slider("Lerp factor", ref lerpFactor, 0, 1);
        ImGuiHelpers.Slider("Smooth Factor", ref SmoothFactor, 0, 1);
        ImGuiHelpers.SliderInt("Smooth Steps", ref SmoothSteps, 0, 32);
        ImGuiHelpers.Slider("Depth Scaling", ref depthScaling, 1, 10_0);
        ImGuiHelpers.Combo("Mode", ref mode);

        base.DrawImGuiSettings();
    }

    public override void Draw(View view)
    {
        if (!view.Is3DCamera)
            return;

        if (Count != Particles.Length)
            Init();


        // Initialize();
        var dat = GetRequiredWorldService<DataService>();
        var datVectorField = dat.VectorField;
        var dom = ((RectDomain<Vec3>)datVectorField.Domain);
        dom.MakeFinalAxisPeriodic();
        IVectorField<Vec3, Vec3> vectorfield = new IncreasedDimensionVectorField<Vec3, Vec2, Vec3>(datVectorField, IVectorField<Vec3, double>.Constant(1))
        {
            Domain = dom
        };
        if (UseRandomizedField)
            vectorfield = this.vectorfield;
        //  if (reverse)
        //      vectorfield = new IncreasedDimensionVectorField<Vec3, Vec2, Vec3>(new ArbitraryField<Vec3, Vec2>(datVectorField.Domain, p => -datVectorField.Evaluate(p)), IVectorField<Vec3, Vec1>.Constant(1));
        var domainRectBoundary = vectorfield.Domain.Bounding;
        var domainRect = vectorfield.Domain.RectBoundary;

        var rk4 = IIntegrator<Vec3, Vec3>.Rk4Steady;

        if (VolumeRender)
        {
            foreach (ref var p in StorageBuffer.Data.AsSpan())
            {
                p /= 1.4f;
            }
            for (int i = 0; i < Particles.Length; i++)
            {
                var sign = i < Particles.Length / 2 ? 1 : -1;
                var p = Particles[i];
                var v = (domainRect.ToRelative(p.Phase) * VolumeGridSize.ToVec3()).FloorInt();
                if (Grid.Contains(v))
                    Grid[v] = Utils.Lerp(Grid[v], i < Particles.Length / 2 ? 1 : 1, (float)lerpFactor);
            }

            for (int s = 0; s < SmoothSteps; s++)
            {
                for (int x = 1; x < Grid.GridSize.X - 1; x++)
                for (int y = 1; y < Grid.GridSize.Y - 1; y++)
                for (int z = 1; z < Grid.GridSize.Z - 1; z++)
                {
                    var neighAvg = /*Grid[new Vec3i(x - 1, y, z)] +
                                   Grid[new Vec3i(x + 1, y, z)] +
                                   Grid[new Vec3i(x, y + 1, z)] +
                                   Grid[new Vec3i(x, y - 1, z)] +*/
                        Grid[new Vec3i(x, y, z - 1)] +
                        Grid[new Vec3i(x, y, z + 1)];
                    neighAvg /= 2.0f;
                    GridTemp[new Vec3i(x, y, z)] = Utils.Lerp(Grid[new Vec3i(x, y, z)], neighAvg, (float)SmoothFactor);
                }
                Array.Copy(GridTemp.Data, Grid.Data, Grid.Data.Length);
            }
            // Array.Fill(StorageBuffer.Data, 10);
            mat.Use();

            StorageBuffer.Use();
            StorageBuffer.Upload();
            mat.SetUniform("tint", Color.White);
            mat.SetUniform("cameraPosUni", view.Camera.Position);
            var heat3dMaxCellPos = domainRect.Max;
            var heat3dMinCellPos = domainRect.Min;
            view.CameraOffset = -(heat3dMaxCellPos + heat3dMinCellPos) / 2;
            mat.SetUniform("volumeMin", heat3dMinCellPos);
            mat.SetUniform("threshold", threshold);
            mat.SetUniform("volumeMax", heat3dMaxCellPos);
            mat.SetUniform("depthScaling", depthScaling);
            mat.SetUniform("gridSize", VolumeGridSize.ToVec3());
            mat.SetUniform("view", view.Camera.GetViewMatrix());
            mat.SetUniform("projection", view.Camera.GetProjectionMatrix());
            var size = heat3dMaxCellPos - heat3dMinCellPos;
            mat.SetUniform("model", Matrix4x4.CreateScale(size) * Matrix4x4.CreateTranslation(size / 2));
            mat.SetUniform("colorgradient", GetRequiredWorldService<DataService>().ColorGradient.Texture.Value);
            Gizmos.UnitCube.Draw();
        }

        var advectionField = vectorfield;

        /*if (reverse)
            integrationT *= -1;*/
        if (mode == Mode.Combined)
        {

            Parallel.For(0, Particles.Length / 2, (i) => { Particles[i].Phase = rk4.Integrate(advectionField, Particles[i].Phase, dt); });
            advectionField = new ArbitraryField<Vec3, Vec3>(vectorfield.Domain, p => -vectorfield.Evaluate(p));
            Parallel.For(Particles.Length / 2, Particles.Length, (i) => { Particles[i].Phase = rk4.Integrate(advectionField, Particles[i].Phase, dt); });
        }
        else if (mode == Mode.Forewords)
        {
            Parallel.For(0, Particles.Length, (i) => { Particles[i].Phase = rk4.Integrate(advectionField, Particles[i].Phase, dt); });
        }
        else
        {
            advectionField = new ArbitraryField<Vec3, Vec3>(vectorfield.Domain, p => -vectorfield.Evaluate(p));
            Parallel.For(0, Particles.Length, (i) => { Particles[i].Phase = rk4.Integrate(advectionField, Particles[i].Phase, dt); });
        }

        Parallel.For(0, Particles.Length, (i) =>
        {
            ref var p = ref Particles[i];
            p.Phase = domainRectBoundary.Bound(p.Phase);
            p.TimeAlive += dt;

            if ( /*(p.Phase.Last >= domainRect.Max.Last || p.Phase.Last <= domainRect.Min.Last)*/ /*||*/ Random.Shared.NextSingle() < ReseedChance * dt)
            {
                p.Phase = Utils.Random(domainRect);
                p.TimeAlive = 0;
            }
        });

        if (maxAlpha > 0)
        {
            GL.BlendFuncSeparate(
                BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha,
                BlendingFactorSrc.One, BlendingFactorDest.One
            );


            for (int i = 0; i <= (Particles.Length - 1); i++)
            {
                ref var p = ref Particles[i];
                var alpha = maxAlpha * Math.Min(1, p.TimeAlive * FadeInFactor);
                Color color = new Color(0, 1, 0, alpha);
                switch (mode)
                {

                    case Mode.Forewords:
                        color = new Color(0, 1, 0, alpha);
                        break;
                    case Mode.Backwords:
                        color = new Color(1, 0, 0, alpha);
                        break;
                    case Mode.Combined:
                        if (i > Particles.Length / 2)
                            color = new Color(1, 0, 0, alpha);
                        else
                            color = new Color(0, 1, 0, alpha);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                Gizmos.Instanced.RegisterSphere(p.Phase, Radius, color);
            }
            Gizmos.Instanced.DrawSpheres(view.Camera);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

    }
    public void Drawdd(RenderTexture rendertarget, View view)
    {
        if (!view.Is3DCamera)
            return;

        if (Count != Particles.Length)
            Init();

        // Initialize();
        var dat = GetRequiredWorldService<DataService>();
        var vectorfield = dat.VectorField;
        var domainRectBoundary = vectorfield.Domain.Bounding;
        var domainRect = vectorfield.Domain.RectBoundary;



        var rk4 = IIntegrator<Vec3, Vec2>.Rk4;
        GL.BlendFuncSeparate(
            BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha,
            BlendingFactorSrc.One, BlendingFactorDest.One
        );

        var advectionField = vectorfield;
        if (reverse)
            dt *= -1;

        foreach (ref var p in Particles.AsSpan())
        {
            p.Phase = domainRectBoundary.Bound(rk4.Integrate(advectionField, p.Phase, dt));
            p.TimeAlive += double.Abs(dt);

            if (p.Phase.Last >= domainRect.Max.Last || Random.Shared.NextSingle() < ReseedChance * dt)
            {
                p.Phase = Utils.Random(domainRect);
                p.TimeAlive = 0;
            }
        }

        var ps = Particles.AsSpan();
        for (int i = 0; i < ps.Length; i++)
        {
            ref var p = ref ps[i];
            var alpha = maxAlpha * Math.Min(1, p.TimeAlive * FadeInFactor);
            Color color = new Color(0, 1, 0, alpha);
            if (i > ps.Length / 2)
                color = new Color(1, 0, 0, alpha);
            Gizmos.Instanced.RegisterSphere(p.Phase, Radius, color);
        }
        Gizmos.Instanced.DrawSpheres(view.Camera);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

    }
}