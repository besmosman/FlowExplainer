using ImGuiNET;
using OpenTK.Graphics.ES11;

namespace FlowExplainer;

public class FlowVisService : WorldService
{
    public override ToolCategory Category => ToolCategory.Flow;

    public RenderTexture RenderTexture;
    private Particle[] Particles;

    struct Particle
    {
        public Vec2 StartPos;
        public Vec2 CurPos;
    }

    public override void Initialize()
    {
        RenderTexture = new RenderTexture(1000, 500);
        Init();
    }

    private void Init()
    {
        Particles = new Particle[10000];
        var dat = GetRequiredWorldService<DataService>();
        var bounds = dat.VectorField.Domain;
        foreach (ref var p in Particles.AsSpan())
        {
            p.StartPos = Utils.Random(bounds.RectBoundary.Reduce<Vec2>());
            p.CurPos = p.StartPos;
        }

        RenderTexture.DrawTo(() =>
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
        });
    }

    public override void Update()
    {
        var dat = GetRequiredWorldService<DataService>();
        var velField = new ArbitraryField<Vec3, Vec2>(dat.VectorField.Domain, (p)=> dat.VectorField.Evaluate(p)*100);

        double dt = 1/100f;
        foreach (ref var p in Particles.AsSpan())
        {
            p.CurPos = IIntegrator<Vec3, Vec2>.Rk4.Integrate(velField, p.CurPos.Up(t), dt);
            /*if (instantField.TryEvaluate(p.CurPos, out var dir))
            {
                p.CurPos += Vec2.Normalize(dir) * dt;
            }*/
        }

        base.Update();
    }

    public double t = 0;
    public override void Draw(RenderTexture rendertarget, View view)
    {
        var dat = GetRequiredWorldService<DataService>();
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);

        t += FlowExplainer.DeltaTime / 100;
        RenderTexture.DrawTo(() =>
        {
            //GL.Clear(ClearBufferMask.ColorBufferBit);
            foreach (var p in Particles)
            {
                var col = dat.ColorGradient.GetCached(t);
                col.A = .01f;
                Gizmos2D.Instanced.RegisterCircle(p.CurPos, .001f, col);
            }

            Gizmos2D.Instanced.RenderCircles(view.Camera2D.RenderTargetRelative(RenderTexture, dat.VectorField.Domain.RectBoundary.Reduce<Vec2>()));
        });
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        Gizmos2D.ImageCenteredInvertedY(view.Camera2D, RenderTexture, dat.VectorField.Domain.RectBoundary.Center.XY, dat.VectorField.Domain.RectBoundary.Size.XY);

    }

    public override void DrawImGuiEdit()
    {
        ImGui.Text("wo");
        if (ImGui.Button("init"))
        {
            t = GetRequiredWorldService<DataService>().SimulationTime;
            Init();
        }
        
        base.DrawImGuiEdit();
    }
}