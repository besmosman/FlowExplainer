using System.Numerics;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class DensityPathStructuresSpaceTimeData : WorldService, IAxisTitle
{
    private Sample[] Samples;

    private StorageBuffer<Sample> SampleBuffer;

    public struct Sample
    {
        public float Accumulation;
        public float LIC;
        public float Count;
        public float padding1;
    }




    public Vec2i SampleGridSize = new Vec2i(32, 16) * 16;

    private Rect<Vec2> WorldRect;

    public double InfluenceRadius = .01f;
    public double AccumelationFactor = .1f;
    public double Decay = .04f;
    public double reseedRate = .005f;
    public bool Normalize = true;
    public double TimeRange;

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



    public override void Draw(View view)
    {  
        var dat = GetRequiredWorldService<DataService>();
        var flux = GetRequiredWorldService<DataService>().VectorField;
        var bounding = flux.Domain.Bounding;
        double f = TimeRange/2;

        if (!view.Is2DCamera)
        {
            double t = bounding.BoundLastAxis(dat.SimulationTime);
            Gizmos.DrawLine(view, new Vec3(.0,.0, t-f), new Vec3(.0,.0,t+f), .03, new Color(1,1,0));
            return;
        }
        
        var Particles = GetRequiredWorldService<DensityParticlesData>().Particles;

        var T = GetRequiredWorldService<DataService>().LoadedDataset.ScalerFields["Convective Temperature"];

        var rk = IIntegrator<Vec3, Vec2>.Rk4;
      
        var dt = dat.MultipliedDeltaTime;

        var seedRect = dat.VectorField.Domain.RectBoundary.Reduce<Vec2>();

        foreach (ref var s in Samples.AsSpan())
        {
            s.Accumulation /= (1f + (float)Decay);
            // s.Count = 0;
            //s.Accumulation =0;
            //s.Accumulation +=0.0001f;
            //s.Accumulation *= 2.1f;
            //s.MinDistance = float.MaxValue;
        }
        int k = 0;
        double influenceRadius2 = InfluenceRadius * InfluenceRadius;
        int radX = (int)double.Ceiling(InfluenceRadius / (WorldRect.Size.X / SampleGridSize.X)) + 0;
        int radY = (int)double.Ceiling(InfluenceRadius / (WorldRect.Size.Y / SampleGridSize.Y)) + 0;
        float circleRadius = (float)InfluenceRadius;


        double maxTime = bounding.BoundLastAxis(dat.SimulationTime + f);
        double minTime =  bounding.BoundLastAxis(dat.SimulationTime - f);
        Parallel.For(0, Particles.Length, c =>
                //for (int c = 0; c < Particles.Length; c++)
            {
                ref var p = ref Particles[c];
                double particleTime = p.Phase.Z;
                double particleInfluence = 0;
                if (particleTime < minTime && particleTime > maxTime)
                {
                    return;
                }
                particleInfluence = 1f - double.Abs(particleTime - (maxTime + minTime) / 2) / ((maxTime - minTime)/2);
                particleInfluence = double.Clamp(particleInfluence, 0, 1);
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
                    var samplePos = WorldRect.FromRelative((gridCoord.ToVec2() + new Vec2(.5f, .5f)) / SampleGridSize.ToVec2());

                    //var disSqrt = (float)bounding.ShortestSpatialDistanceSqrt(p.Position.Up(0), samplePos.Up(0));
                    //var dis = float.Sqrt(disSqrt);
                    var disSqrt = DistancePointSegmentSq(samplePos, p.LastPhase.XY, p.Phase.XY);
                    //var dis = double.Sqrt(disSqrt);
                    //if (dis < InfluenceRadius)
                    {
                        GetSampleInfoAt(gridCoord).Accumulation += Accum((float)disSqrt, (float)p.TimeAlive, (float)(AccumelationFactor * particleInfluence));
                        //GetSampleInfoAt(gridCoord).LIC += Accum((float)disSqrt,  (float)p.TimeAlive, p.Noise);
                        GetSampleInfoAt(gridCoord).Count = 1;
                        //float lifeTimeFactor = float.Min(1, p.TimeAlive);
                        //float distanceToCircle = (dis - circleRadius);
                        //sample.Accumulation = float.Min(sample.Accumulation, distanceToCircle);
                        //sample.MinDistance = lifeTimeFactor;
                    }
                    ;
                }
            })
            ;

        def.Accumulation = 0;
        material.Use();
        material.SetUniform("tint", new Color(1, 0, 1, 1));

        SampleBuffer.Upload();
        SampleBuffer.Use();
        material.SetUniform("WorldViewMin", WorldRect.Min);
        material.SetUniform("WorldViewMax", WorldRect.Max);
        material.SetUniform("GridSize", SampleGridSize.ToVec2());
        material.SetUniform("view", view.Camera2D.GetViewMatrix());
        material.SetUniform("colorgradient", GetRequiredWorldService<DataService>().ColorGradient.Texture.Value);
        material.SetUniform("projection", view.Camera2D.GetProjectionMatrix());
        var model = Matrix4x4.CreateScale((float)WorldRect.Size.X, (float)WorldRect.Size.Y, .4f) * Matrix4x4.CreateTranslation((float)WorldRect.Min.X, (float)WorldRect.Min.Y, 0);
        material.SetUniform("model", model);
        Gizmos2D.imageQuadInvertedY.Draw();
    }


    private float Accum(float dis, float timeAlive, float accum)
    {
        var timeFactor = float.Clamp(timeAlive, 0, 1);
        float sigma = (float)InfluenceRadius / 3.3f * timeFactor;
        float spatialFactor = MathF.Exp(-(dis) / (2f * sigma * sigma));
        return timeFactor * spatialFactor * accum;
        /*if (spatialFactor > .5f)
            spatialFactor *= 5;*/
        //else spatialFactor = 0;

    }
    public override void DrawImGuiSettings()
    {
        ImGuiHelpers.Slider("InfluenceRadius", ref InfluenceRadius, 0, .01f);
        ImGuiHelpers.Slider("AccumulationFactor", ref AccumelationFactor, 0, 1f);
        ImGuiHelpers.Slider("Reseed Rate", ref reseedRate, 0, .1f);
        ImGuiHelpers.Slider("TimeRange", ref TimeRange, 0, .3f);
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
        var type = dat.TimeMultiplier > 0 ? "Attracting" : "Repelling";
        return $"{type} {(dat.currentSelectedVectorField)} [Double Gyre Pe={dat.LoadedDataset.Properties["Pe"]},eps={dat.LoadedDataset.Properties["EPS"]}]";
    }

    //gpt
    static double DistancePointSegmentSq(Vec2 p, Vec2 a, Vec2 b)
    {
        Vec2 ab = b - a;
        double abLenSq = Vec2.Dot(ab, ab);

        // Degenerate segment
        if (abLenSq == 0f)
            return Vec2.Dot(p - a, p - a);

        double t = Vec2.Dot(p - a, ab) / abLenSq;
        t = Math.Clamp(t, 0f, 1f);

        Vec2 closest = a + ab * t;
        Vec2 d = p - closest;
        return Vec2.Dot(d, d);
    }




}