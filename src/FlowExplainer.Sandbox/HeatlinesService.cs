namespace FlowExplainer;

public class HeatlinesService : WorldService
{
    public int samplesX = 5;
    public int samplesY = 3;

    public Artifact<IVectorField<Vec2, double>> H;

    public override void Initialize()
    {
        var t = DataService.SimulationTime;
        var u = DataService.LoadedDataset.GetVectorField<Vec3, Vec2>("Velocity").ReducedSlice<Vec3, Vec2, Vec2>(() => t);
        var T = DataService.LoadedDataset.GetVectorField<Vec3, double>("Total Temperature").ReducedSlice<Vec3, Vec2, double>(() => t);
        var Pe = 100;

        var flux = new ArbitraryField<Vec2, Vec2>(T.Domain, x =>
        {
            var gradT = T.FiniteDifferenceGradient(x, .001f);
            return u.Evaluate(x) * T.Evaluate(x) - (1.0 / Pe) * gradT;
        });

        H = new(SolveHeatFunction(flux), "Heat Function", "");
        Artifacts.RegisterOrUpdate(H);
        var scaler2DGridDiagnostic = new Scaler2DGridDiagnostic();
        GetRequiredWorldService<GridVisualizer>().SetGridDiagnostic(scaler2DGridDiagnostic);
        scaler2DGridDiagnostic.ScalerField = H;
    }

    public RegularGridVectorField<Vec2, Vec2i, double> SolveHeatFunction(IVectorField<Vec2, Vec2> q)
    {
        Vec2i res = new Vec2i(32, 16);
        RegularGridVectorField<Vec2, Vec2i, double> H = new(res, new RectDomain<Vec2>(q.Domain.RectBoundary));


        var d = H.RectDomain.Rect.Size / H.Grid.GridSize;
        /*var qxdx = q
            .SelectOutput(s => s.X)
            .Select((s, x) => s.SpatialDerivative(x, 0, d.X))
            .Discritize(res);

        var qydy = q
            .SelectOutput(s => s.Y)
            .Select((s, x) => s.SpatialDerivative(x, 1, d.Y))
            .Discritize(res);*/

        var step = .01;
        var totalError = 0.0;
        for (int s = 0; s < 1000; s++)
        {
            totalError = 0;
            for (int i = 1; i < H.Grid.GridSize.X - 1; i++)
            for (int j = 1; j < H.Grid.GridSize.Y - 1; j++)
            {
                var pos = q.Domain.RectBoundary.FromRelative(new Vec2(i, j) / H.GridSize);
                ref var at = ref H.AtCoords(new Vec2i(i, j));
                var left = H.AtCoords(new Vec2i(i - 1, j));
                var right = H.AtCoords(new Vec2i(i + 1, j));
                var up = H.AtCoords(new Vec2i(i, j + 1));
                var down = H.AtCoords(new Vec2i(i, j - 1));

                var qAt = q.Evaluate(pos);
                //at = Utils.Lerp(at, right - qAt.X * d.X, step);
                //dHdy = q_x
                //dHdy = (up - at)/d.Y
                //(up - at)/d.Y = c
                //(up-at) = c*d.Y
                //-at =  c*d.y - up;
                //at = up - c*d.y


                //-dHdx = q.y
                //-q.c = (right-at)/d.x
                //-q.c*d.x = right-at
                // -at = -q.c*d.x - right
                //at = right + q.c*d.x
                at = Utils.Lerp(at, up - qAt.X * d.Y, step);
                at = Utils.Lerp(at, right + qAt.Y * d.X, step);


                var Hdx = (right - left) / (2 * d.X);
                var Hdy = (up - down) / (2 * d.Y);
                var error1 = Hdx - -qAt.Y;
                totalError += error1 * error1;

                //var Hdx = (right - at) / d.X;
                /*var Hdx = (right - left) / (2 * d.X);
                var Hdy = (up - down) / (2 * d.Y);
                var qAt = q.Evaluate(pos);
                var error1 = Hdx - -qAt.Y;
                var error2 = Hdy - qAt.X;
                at += error1 * step;
                //at += -error2 * step;
                totalError += error1 * error1;*/
            }

            int c = 4;
        }

        return H;
    }

    public override void Draw(View view)
    {
        var t = DataService.SimulationTime;
        var u = DataService.LoadedDataset.GetVectorField<Vec3, Vec2>("Velocity").ReducedSlice<Vec3, Vec2, Vec2>(() => t);
        var T = DataService.LoadedDataset.GetVectorField<Vec3, double>("Total Temperature").ReducedSlice<Vec3, Vec2, double>(() => t);
        var Pe = 100;

        var flux = new ArbitraryField<Vec2, Vec2>(T.Domain, x =>
        {
            var gradT = T.FiniteDifferenceGradient(x, .001f);
            return u.Evaluate(x) * T.Evaluate(x) - (1.0 / Pe) * gradT;
        });
        var rect = flux.Domain.RectBoundary;

        var fluxNormalized = new ArbitraryField<Vec2, Vec2>(flux.Domain,
            (x) => flux.Evaluate(x).NormalizedSafe());

        for (int i = 0; i < samplesX; i++)
        for (int j = 0; j < samplesY; j++)
        {
            var startPos = rect.FromRelative(new Vec2(i, j) / new Vec2(samplesX, samplesY));
            var lastX = startPos;
            var x = lastX;
            int steps = 256;
            float stepSize = .004f;
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