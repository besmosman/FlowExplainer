using System.Numerics;
using OpenTK.Graphics.OpenGL4;

namespace FlowExplainer;

public class DensityParticlesDistanceFieldVisualizer : WorldService
{

    private Texture3D DataTexture;
    private Material distanceVolumeMaterial;
    public Vec3i GridSize = new Vec3i(32, 16, 32)*4;

    public override void Initialize()
    {
        DataTexture = new Texture3D(GridSize);

        distanceVolumeMaterial = new(new Shader("Assets/Shaders/sdf3d.frag", ShaderType.FragmentShader),
            Shader.DefaultWorldSpaceVertex);
    }

    public double InfluenceRadius = .1f;
    public double fade = .1f;
    public override void Draw(View view)
    {
        foreach (ref var c in DataTexture.Pixels.AsSpan())
        {
            c.R *= (float)fade;
        }

        var pdata = GetRequiredWorldService<DensityParticlesData>();
        var domain = GetRequiredWorldService<DensityParticlesData>().VelocityField.Domain;

        Parallel.For(0, pdata.Particles.Length, pi =>
        {
            ref var p = ref pdata.Particles[pi];
            var voxelCenter = (domain.RectBoundary.ToRelative(p.Phase) * DataTexture.Size.ToVec3()).FloorInt();
            int r = 2;
            for (int i = -r; i <= r; i++)
            for (int j = -r; j <= r; j++)
            for (int k = -r; k <= r; k++)
            {
                var disSqrt = Vec3.DistanceSquared(p.Phase, domain.RectBoundary.FromRelative((voxelCenter.ToVec3() + new Vec3(i + .5f, j + .5f, k + .5f)) / DataTexture.Size.ToVec3()));
                var accum = Accum((float)disSqrt, 1, float.Min(1,(float)p.TimeAlive)/10);
                //if(disSqrt < .0003f)
                DataTexture.GetPixelAt(voxelCenter + new Vec3i(i, j, k)).R += accum;
            }
        });

        DataTexture.UpdateData();
        distanceVolumeMaterial.Use();
        distanceVolumeMaterial.SetUniform("data", DataTexture);
        distanceVolumeMaterial.SetUniform("gridSize", GridSize);
        distanceVolumeMaterial.SetUniform("volumeMin", domain.RectBoundary.Min);
        distanceVolumeMaterial.SetUniform("volumeMax", domain.RectBoundary.Max);
        distanceVolumeMaterial.SetUniform("view", view.Camera.GetViewMatrix());
        distanceVolumeMaterial.SetUniform("projection", view.Camera.GetProjectionMatrix());
        var size = domain.RectBoundary.Size;
        distanceVolumeMaterial.SetUniform("model", Matrix4x4.CreateScale(size) * Matrix4x4.CreateTranslation(size / 2));
        distanceVolumeMaterial.SetUniform("colorgradient", GetRequiredWorldService<DataService>().ColorGradient.Texture.Value);
        Gizmos.UnitCube.Draw();
    }

    public override void DrawImGuiSettings()
    {
        ImGuiHelpers.Slider("InfluenceRadius", ref InfluenceRadius, 0, .1);
        ImGuiHelpers.Slider("Fade", ref fade, 0, 1);
        base.DrawImGuiSettings();
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
}