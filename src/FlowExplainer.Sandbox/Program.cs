using System.Globalization;
using FlowExplainer;



while (true)
{
    var t = 3;
    var u = IVectorField<Vec2, Vec2>.Arbitrary(new RectDomain<Vec2>(new Vec2(0,0), new Vec2(1,1)), x => new Vec2(1, 2));
    var T = IVectorField<Vec2, double>.Arbitrary(u.Domain, x => 1);
    var Pe = 100;
    var flux = new ArbitraryField<Vec2, Vec2>(T.Domain, x =>
    {
        var gradT = T.FiniteDifferenceGradient(x, .001f);
        return u.Evaluate(x) * T.Evaluate(x) - (1.0 / Pe) * gradT;
    });
    var rect = flux.Domain.RectBoundary;

    var fluxNormalized = new ArbitraryField<Vec2, Vec2>(flux.Domain,
        (x) => flux.Evaluate(x).NormalizedSafe());

    for (int i = 0; i < 20; i++)
    for (int j = 0; j < 10; j++)
    {
        var startPos = rect.FromRelative(new Vec2(i, j) / new Vec2(20, 10));
        var lastX = startPos;
        var x = lastX;
        int steps = 132;
        float stepSize = .01f;
        for (int k = 0; k < steps; k++)
        {
            x = IIntegrator<Vec2, Vec2>.Rk4Steady.Integrate(fluxNormalized, x, stepSize);
            // Gizmos2D.Instanced.RegisterLine(lastX, x, Color.White, .001f);
            lastX = x;
        }
    }
// Gizmos2D.Instanced.RenderRects(view.Camera2D);
}

DedicatedGraphics.InitializeDedicatedGraphics();
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
Config.Load("config.json");
var app = new FlowExplainer.FlowExplainer();
app.AddDefaultGlobalServices();
Scripting.Startup(app.GetGlobalService<WorldManagerService>().Worlds[0]);
app.Run();