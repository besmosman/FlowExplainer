using System.Numerics;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class DensityStructuresSpaceTime3DUI : WorldService
{
    public override string? Name => "UI";
    
    public override void Initialize()
    {
    }

    public override void Draw(View view)
    {
        if (!view.Is2DCamera)
        {
            var dat = GetRequiredWorldService<DataService>();
            var flux = GetRequiredWorldService<DataService>().VectorField;
            var bounding = flux.Domain.Bounding;
            double t_bounded = bounding.BoundLastAxis(dat.SimulationTime);
            var s = GetRequiredWorldService<DensityParticlesData>().SeedTimeRange;
            var structures = GetRequiredWorldService<DensityPathStructuresSpaceTime>();
            Gizmos.DrawLine(view, new Vec3(.0, .0, t_bounded - structures.Tau), new Vec3(.0, .0, t_bounded + structures.Tau), .03, new Color(1, 1, 0));
            Gizmos.DrawLine(view, new Vec3(.0, .0, t_bounded - s), new Vec3(.0, .0, t_bounded + s), .022, new Color(0, 1, 0));
        }
    }
}

public class DensityPathStructuresSpaceTime : WorldService, IAxisTitle
{
    private Sample[] Samples;

    private StorageBuffer<Sample> SampleBuffer;
    private RenderTexture RenderTexture;

    public struct Sample
    {
        public float Accumulation;
        public float SignedZDistance;
        public float Count;
        public float padding1;
    }


    public int SampleGridSizeX = 1280;
    public int SampleToTextureMultiple = 1;
    public Vec2i SampleGridSize => new Vec2i(SampleGridSizeX, SampleGridSizeX / 2);
    public Vec2i TextureSize => SampleGridSize * SampleToTextureMultiple;
    private Rect<Vec2> WorldRect;
    private Rect<Vec2> RenderWorldRect;

    private bool Extend = true;
    public double InfluenceRadius = .005f;
    public double AccumelationFactor = .1f;
    public double Decay = .04f;
    public bool Normalize = true;
    public double Power = 1 / 2f;
    public double Tau = 0.01;

    public override string? Name => "Density Path Structures";
    public override string? Description => "Visualization of attracting and repelling structures in transport fields";

    public Vec2 WorldToGrid(Vec2 world)
    {
        return WorldRect.ToRelative(world) * SampleGridSize.ToVec2();
    }

    private Sample def;

    public ref Sample GetSampleInfoAt(Vec2i cell)
    {
        if (cell.X < 0 || cell.Y < 0 || cell.X >= SampleGridSize.X || cell.Y >= SampleGridSize.Y)
            return ref def;
        return ref Samples[cell.Y * SampleGridSize.X + cell.X];
    }

    private Material material;

    public override void Initialize()
    {
        material = new Material(new Shader("Assets/Shaders/sdf.frag", ShaderType.FragmentShader), Shader.DefaultWorldSpaceVertex);
        Reset();
        RenderTexture = new RenderTexture(1, 1);
    }

    FastNoise noise = new FastNoise();

    private void Reset()
    {
        var rect = DataService.VectorField.Domain.RectBoundary;
        WorldRect = rect.Reduce<Vec2>();

        Samples = new Sample[SampleGridSize.Volume()];
        SampleBuffer = new StorageBuffer<Sample>(Samples);
        ref var Particles = ref GetRequiredWorldService<DensityParticlesData>().Particles;
        GetRequiredWorldService<DensityParticlesData>().Initialize();
    }

    private float EvalNoise(Vec2 pos)
    {
        return (float)((noise.GetNoise((float)pos.X * 4000, (float)pos.Y * 4000)) + 1) * 0.5f;
    }

    public override void PreDraw()
    {
        if (Samples.Length != SampleGridSize.Volume())
        {
            Array.Resize(ref Samples, SampleGridSize.Volume());
            Array.Clear(Samples);
            SampleBuffer = new StorageBuffer<Sample>(Samples);
        }


        if (RenderTexture.Size != TextureSize)
            RenderTexture.Resize(TextureSize.X, TextureSize.Y);


        RenderTexture.DrawTo(() =>
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            var cam = new Camera2D()
            {
                Position = -new Vec2(.5, .25),
                RenderTargetSize = RenderTexture.Size.ToVec2(),
                Scale = RenderTexture.Size.ToVec2().X,
            };
            var dat = GetRequiredWorldService<DataService>();
            var flux = GetRequiredWorldService<DataService>().VectorField;
            var bounding = flux.Domain.Bounding;
            double t_bounded = bounding.BoundLastAxis(dat.SimulationTime);

            var Particles = GetRequiredWorldService<DensityParticlesData>().Particles;


            float decayMulti = 1 / (1f + (float)Decay);

            foreach (ref var s in Samples.AsSpan())
            {
                s.Accumulation *= decayMulti;
            }

            var worldRect = WorldRect;
            int radX = (int)double.Ceiling(InfluenceRadius / (worldRect.Size.X / SampleGridSize.X)) + 0;
            int radY = (int)double.Ceiling(InfluenceRadius / (worldRect.Size.Y / SampleGridSize.Y)) + 0;

            if (true)
            {
                //worldRect.Min -= new Vec2(0.5, 0);
                // worldRect.Max += new Vec2(0.5, 0);
            }

            Parallel.For(0, Particles.Length, c =>
            {
                ref var p = ref Particles[c];
                double particleTime = p.Phase.Z;
                if (particleTime < t_bounded - Tau || particleTime > t_bounded + Tau)
                    return;

                var centerA = WorldToGrid(p.LastPhase.XY).RoundInt();
                var centerB = WorldToGrid(p.Phase.XY).RoundInt();

                if (Vec2i.DistanceSquared(centerA, centerB) > SampleGridSize.X / 2)
                {
                    centerB = centerA; //bound issue
                }

                var minCenter = new Vec2i(int.Min(centerA.X, centerB.X), int.Min(centerA.Y, centerB.Y));
                var maxCenter = new Vec2i(int.Max(centerA.X, centerB.X), int.Max(centerA.Y, centerB.Y));


                for (int i = -radX + minCenter.X; i < radX + maxCenter.X; i++)
                for (int j = -radY + minCenter.Y; j < radY + maxCenter.Y; j++)
                {
                    var gridCoord = new Vec2i(i, j);
                    var samplePos = worldRect.FromRelative((gridCoord.ToVec2() + new Vec2(.0f, .0f)) / SampleGridSize.ToVec2());
                    var disSqrt = DistancePointSegmentSq(samplePos, p.LastPhase.XY, p.Phase.XY, out var t);

                    var signedDistanceToSliceZ = (Utils.Lerp(p.LastPhase, p.Phase, t).Z - t_bounded);
                    var timeAlive = double.Lerp(p.LastTimeAlive, p.TimeAlive, t);
                    double particleInfluence = 1 - double.Abs(signedDistanceToSliceZ / Tau);
                    //particleInfluence *= double.Abs(p.Phase.Z - p.LastPhase.Z);
                    particleInfluence = Math.Clamp(particleInfluence, 0, 1);

                    ref var sampleInfoAt = ref GetSampleInfoAt(gridCoord);
                   // particleInfluence = 1;
                   // disSqrt = 0;
                    sampleInfoAt.Accumulation += Accum((float)disSqrt, 1, (float)(AccumelationFactor * particleInfluence));
                    sampleInfoAt.Count++;
                }
            });

            def.Accumulation = 0;
            material.Use();
            material.SetUniform("tint", new Color(1, 0, 1, 1));

            SampleBuffer.Upload();
            SampleBuffer.Use();
            
            material.SetUniform("WorldViewMin", worldRect.Min);
            material.SetUniform("WorldViewMax", worldRect.Max);
            material.SetUniform("GridSize", SampleGridSize.ToVec2());
            material.SetUniform("Power", Power);
            material.SetUniform("view", cam.GetViewMatrix());
            material.SetUniform("colorgradient", GetRequiredWorldService<DataService>().ColorGradient.Texture.Value);
            material.SetUniform("projection", cam.GetProjectionMatrix());


            var model = Matrix4x4.CreateScale((float)worldRect.Size.X, (float)worldRect.Size.Y, .4f) * Matrix4x4.CreateTranslation((float)worldRect.Min.X, (float)worldRect.Min.Y, 0);
            material.SetUniform("model", model);
            Gizmos2D.imageQuadInvertedY.Draw();
        });
        base.PreDraw();
    }

    public override void Draw(View view)
    {
        if (view.Is3DCamera)
            return;

        var dat = GetRequiredWorldService<DataService>();
        var flux = GetRequiredWorldService<DataService>().VectorField;
        var bounding = flux.Domain.Bounding;
        double t_bounded = bounding.BoundLastAxis(dat.SimulationTime);
        
        Gizmos2D.ImageCenteredInvertedY(view.Camera2D, RenderTexture, new Vec2(0.5, 0.25), new Vec2(1, .5));
    }


    private float Accum(float dis, float timeFactor, float accum)
    {
        float sigma = (float)InfluenceRadius / 3.3f * timeFactor;
        float spatialFactor = MathF.Exp(-(dis) / (2f * sigma * sigma));
        return timeFactor * spatialFactor * accum;
    }

    public override void DrawImGuiSettings()
    {
        ImGuiHelpers.Slider("InfluenceRadius", ref InfluenceRadius, 0, .01f);
        ImGuiHelpers.Slider("Sample Grid Size X", ref SampleGridSizeX, 1, 2048);
        ImGuiHelpers.Slider("Power", ref Power, 0, 2f);
        ImGuiHelpers.Slider("AccumulationFactor", ref AccumelationFactor, 0, 1f);
        ImGuiHelpers.Slider("TimeRange", ref Tau, 0, .3f);
        ImGuiHelpers.Slider("Decay", ref Decay, 0, 1f);
        if (ImGui.Button("Reset"))
        {
            Reset();
        }

        base.DrawImGuiSettings();
    }

    public string GetTitle()
    {
        var dat = World.GetWorldService<DataService>();
        var type = World.GetWorldService<DensityParticlesData>().dt > 0? "Attracting" : "Repelling";
        return $"{type} {(dat.currentSelectedVectorField)} [Double Gyre Pe={dat.LoadedDataset.Properties["Pe"]},eps={dat.LoadedDataset.Properties["EPS"]}]";
    }

    //gpt
    static double DistancePointSegmentSq(Vec2 p, Vec2 a, Vec2 b, out double t)
    {
        Vec2 ab = b - a;
        double abLenSq = Vec2.Dot(ab, ab);

        // Degenerate segment
        if (abLenSq == 0f)
        {
            t = .5;
            return Vec2.Dot(p - a, p - a);
        }

        t = Vec2.Dot(p - a, ab) / abLenSq;
        t = Math.Clamp(t, 0f, 1f);

        Vec2 closest = a + ab * t;
        Vec2 d = p - closest;
        return Vec2.Dot(d, d);
    }
}