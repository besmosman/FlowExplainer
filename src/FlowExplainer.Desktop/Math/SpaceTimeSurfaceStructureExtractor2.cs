using System.Numerics;
using ImGuiNET;
using OpenTK.Graphics.ES11;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FlowExplainer;

public class SpaceTimeSurfaceStructureExtractor2 : WorldService
{

    public struct Particle
    {
        public Vec3 Position;
    }

    public IVectorField<Vec3, double> ScalerField;
    public Particle[] Particles = [];
    public double Radius = .01f;
    public int ParticleCount = 10000;
    public double MoveSpeed = .1;
    public double TargetValue = 0;
    public override void Initialize()
    {
        Particles = new Particle[ParticleCount];
        ScalerField = World.GetSelectableVectorFields<Vec3, double>().First().VectorField;

        var domainRectBoundary = ScalerField.Domain.RectBoundary;
        foreach (ref var p in Particles.AsSpan())
        {
            p.Position = Utils.Random(domainRectBoundary);
        }
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        if (GetGlobalService<WindowService>().Window.IsKeyDown(Keys.W))
        {
            /*var v= Vector3.Transform(new Vector3(0, 0, 1), view.Camera.Rotation);
            view.CameraOffset += new Vec3(v.X, v.Y,v.Z)*.1f;*/
        }
        
        GL.Enable(EnableCap.DepthTest);
        var colormap = DataService.ColorGradient;
        foreach (ref var p in Particles.AsSpan())
        {
            var grad = ScalerField.FiniteDifferenceGradientIgnoreLast<Vec3, Vec2>(p.Position, .0001f).NormalizedSafe();
            var distanceToTarget = TargetValue - ScalerField.Evaluate(p.Position);
            p.Position += grad.Up(0) * distanceToTarget * MoveSpeed;
        }
        foreach (var p in Particles)
        {
            if (p.Position.Y > .001f)
                Gizmos.Instanced.RegisterSphere(p.Position, Radius, colormap.Get(p.Position.Y));
        }
        Gizmos.Instanced.DrawSpheresLit(view.Camera);
    }

    public override void DrawImGuiSettings()
    {
        if (ImGui.Button("Reset"))
        {
            Initialize();
        }
        ImGuiHelpers.Slider("Radius", ref Radius, 0, .2);
        ImGuiHelpers.Slider("MoveSpeed", ref MoveSpeed, 0, .5);
        if (ImGuiHelpers.Slider("TargetValue", ref TargetValue, -1, 1))
        {
            var domainRectBoundary = ScalerField.Domain.RectBoundary;
            var ps = Particles.AsSpan();
            for (int i = 0; i < ps.Length; i++)
            {
                ref var p = ref ps[i];
                p.Position = Utils.Random(domainRectBoundary);
                i += Random.Shared.Next(0, 16);
            }
        }
        ImGuiHelpers.SliderInt("ParticleCount", ref ParticleCount, 0, 10000);

        base.DrawImGuiSettings();
    }
}