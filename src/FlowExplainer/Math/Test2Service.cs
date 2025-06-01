namespace FlowExplainer;

public class Test2Service : VisualisationService
{
    public override bool HasImGuiEditElements => true;
    private PoincareSectionsComputer poincare;

    public override void Initialize()
    {
        var velocity = new AnalyticalEvolvingVelocityField();
        var integrator = new RungeKutta4Integrator();
        poincare = new();
        poincare.Compute(velocity, integrator, new Vec3(0, 0, 0), new Vec3(2, 1f, 60));
    }

    public override void Draw(RenderTexture rendertarget, View view)
    {
        view.Camera2D.Scale = 800;
        view.Camera2D.Position = -new Vec2(1f, .5f);


        foreach (var p in poincare.tracers)
        {
            foreach (var t in p.Trajectory)
            {
                var pos = t.XY;
              //  if (Math.Abs(t.Z - (MathF.Sin((float)FlowExplainer.Time.TotalSeconds) + 1) / 2 * 30) < 1.9f)
                Gizmos2D.Circle(view.Camera2D, pos, new Color(1, 1, 1), .001f);
            }
        }
    }
}