namespace FlowExplainer;

public class HeatlinesService : WorldService
{

    public int samplesX = 10;
    public int samplesY = 5;
    public override void Initialize()
    {
    }


    public override void Draw(View view)
    {
        var t = DataService.SimulationTime;
        var u = DataService.LoadedDataset.VectorFields["Velocity"].ReducedSlice<Vec3, Vec2, Vec2>(() => t);
        var T = DataService.LoadedDataset.ScalerFields["Total Temperature"].ReducedSlice<Vec3,Vec2, double>(() => t);
        var Pe = 100;
        var flux = new ArbitraryField<Vec2, Vec2>(T.Domain, x =>
        {
            var gradT = T.FiniteDifferenceGradient(x, .001f);
            return u.Evaluate(x) * T.Evaluate(x) -(1.0 / Pe) * gradT;
        });
        var rect = flux.Domain.RectBoundary;
        
        var fluxNormalized =  new ArbitraryField<Vec2, Vec2>(flux.Domain, 
            (x) => flux.Evaluate(x).NormalizedSafe()); 
        
        for (int i = 0; i < samplesX; i++)
        for (int j = 0; j < samplesY; j++)
        {
            var startPos = rect.FromRelative(new Vec2(i, j) / new Vec2(samplesX, samplesY));
            var lastX = startPos;
            var x = lastX;
            int steps = 132;
            float stepSize = .01f;
            for (int k = 0; k < steps; k++)
            {
                x = IIntegrator<Vec2, Vec2>.Rk4Steady.Integrate(fluxNormalized, x, stepSize);
                Gizmos2D.Instanced.RegisterLine(lastX, x, Color.White, .001f);
                lastX = x;
            }
        }
        Gizmos2D.Instanced.RenderRects(view.Camera2D);
    }
}