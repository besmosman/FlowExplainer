using System.Numerics;
using ImGuiNET;
using OpenTK.Graphics.ES11;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FlowExplainer;

public class SpaceTimeScalerGradientService : WorldService
{
    public override string? Name => "Spacetime Scaler Gradient";
    public override string? CategoryName => "Structure";
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
        var vectorField = World.GetSelectableVectorFields<Vec3, double>().First().VectorField;
        ScalerField = vectorField;


        var domainRectBoundary = ScalerField.Domain.RectBoundary;
        foreach (ref var p in Particles.AsSpan())
        {
            p.Position = Utils.Random(domainRectBoundary);
        }
    }

    public override void Draw(View view)
    {
        if(!view.Is3DCamera)
            return;
        
        var TotalFlux = DataService.LoadedDataset.VectorFields["Total Flux"];
        var ConvectiveTemp = DataService.LoadedDataset.ScalerFields["Convective Temperature"];
      //  var ScalerField = new ArbitraryField<Vec3, double>(TotalFlux.Domain, x => (TotalFlux.Evaluate(x).Up(ConvectiveTemp.Evaluate(x)).Length()));
      //  var ScalerField = new ArbitraryField<Vec3, double>(TotalFlux.Domain, x => (TotalFlux.Evaluate(x).Up(ConvectiveTemp.Evaluate(x)).Length()));

        if (lastTarget != TargetValue)
        {
            var domainRectBoundary = ScalerField.Domain.RectBoundary;
            var ps = Particles.AsSpan();
            for (int i = 0; i < ps.Length; i++)
            {
                ref var p = ref ps[i];
                p.Position = Utils.Random(domainRectBoundary);
                i += Random.Shared.Next(0, 16);
            }
            lastTarget = TargetValue;
        }
        if (GetGlobalService<WindowService>().Window.IsKeyDown(Keys.W))
        {
            /*var v= Vector3.Transform(new Vector3(0, 0, 1), view.Camera.Rotation);
            view.CameraOffset += new Vec3(v.X, v.Y,v.Z)*.1f;*/
        }

        GL.Enable(EnableCap.DepthTest);
        var colormap = DataService.ColorGradient;
            var domainBounding = ScalerField.Domain.Bounding;
        Parallel.For(0, Particles.Length, i =>
        {
            ref var p = ref Particles[i];
            var grad = ScalerField.FiniteDifferenceGradientIgnoreLast<Vec3, Vec2>(p.Position, .0001f).NormalizedSafe();
            var distanceToTarget = TargetValue - ScalerField.Evaluate(p.Position);
            p.Position += grad.Up(0) * distanceToTarget * MoveSpeed;
            p.Position = domainBounding.Bound(p.Position);
        });
        foreach (var p in Particles)
        {
            if (p.Position.Y > 0.01f )
                Gizmos.Instanced.RegisterSphere(p.Position, Radius, colormap.Get(p.Position.Y));
        }
        Gizmos.Instanced.DrawSpheresLit(view.Camera);
    }

    private double lastTarget;
    public override void DrawImGuiSettings()
    {
        if (ImGui.Button("Reset"))
        {
            Initialize();
        }
        ImGuiHelpers.Slider("Radius", ref Radius, 0, .2);
        ImGuiHelpers.Slider("MoveSpeed", ref MoveSpeed, 0, .5);
        ImGuiHelpers.Slider("TargetValue", ref TargetValue, -1, 1);

        ImGuiHelpers.SliderInt("ParticleCount", ref ParticleCount, 0, 10000);

        base.DrawImGuiSettings();
    }
}