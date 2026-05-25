using ImGuiNET;

namespace FlowExplainer;

public class VariationalLCS : WorldService
{
    [Input] public Artifact<IVectorField<Vec3, Vec2>>? VelocityField;
    public override string? Name => "Variational LCS";

    public double l_f = 0.2;
    public double l_min = .1;
    public double T = 20;
    double t0 = 0;
    double h = .1;


    private List<Vec2> points = new List<Vec2>();

    public static bool ConditionAValid(EigenInfo eigenInfo)
    {
        return double.Abs(eigenInfo.Lambda1 - eigenInfo.Lambda2) > 0.00000001 && eigenInfo.Lambda2 > 1;
    }


    Vec2 IntegrateRk4(IVectorField<Vec2, Vec2> f, Vec2 initDir, Vec2 p, double dt)
    {
        var k1 = EvaluateDirectionUnawareScaled(f, initDir, p);
        return p + k1 * dt;
        var k2 = EvaluateDirectionUnawareScaled(f, k1, p + (k1 / 2.0) * dt);
        var k3 = EvaluateDirectionUnawareScaled(f, k2, p + (k2 / 2.0) * dt);
        var k4 = EvaluateDirectionUnawareScaled(f, k3, p + (k3) * dt);
        return p + (dt / 6.0) * (k1 + 2.0 * k2 + 2.0 * k3 + k4);
    }


    public override void Initialize()
    {
        VelocityField ??= DataService.Artifacts.Get<IVectorField<Vec3, Vec2>>("Velocity");
        Recompute();
    }

    private List<Trajectory<Vec2>> unfiltered = new();

    private void Recompute()
    {
        h = .000000000001f;
        var velocity = VelocityField!.Value;
        var flowOperator = IFlowOperator<Vec2, Vec3>.Default;

        var cauchyGreenField = CachyGreenStrainField(velocity, flowOperator, t0, T, h);
        var eigenInfoField = EigenInfoField(cauchyGreenField);

        var lambda2Field = eigenInfoField.Select(s => s.Lambda2);
        var eigen2Field = eigenInfoField.Select(s => s.Eigen2);
        var hessianLambda2 = new ArbitraryField<Vec2, Matrix2>(lambda2Field.Domain, x => lambda2Field.Hessian(x, h / 2));

        var conditionBField = new ArbitraryField<Vec2, double>(lambda2Field.Domain, x =>
        {
            var eigenInfo = eigenInfoField.Evaluate(x);
            var cond = Vec2.Dot(
                           eigenInfo.Eigen2,
                           hessianLambda2.Evaluate(x) * eigenInfo.Eigen2) <=
                       0;
            return cond ? 1 : 0;
        });


        var scaled_eigen2 = new ArbitraryField<Vec2, Vec2>(eigenInfoField.Domain, (x) =>
        {
            var info = eigenInfoField.Evaluate(x);
            var lamdbaMax = info.Lambda2;
            var lamdbaMin = info.Lambda1;
            if (lamdbaMax - Double.Sign(lamdbaMax) <= 0)
                return default;
            var alpha = double.Pow((lamdbaMax - lamdbaMin) / (lamdbaMax + lamdbaMin), 2);
            return alpha * info.Eigen1;
        });

        var validSubspace = new ArbitraryField<Vec2, double>(lambda2Field.Domain, x =>
        {
            var eigenInfo = eigenInfoField.Evaluate(x);
            bool conditionB = (Vec2.Dot(
                                   eigenInfo.Eigen2,
                                   hessianLambda2.Evaluate(x) * eigenInfo.Eigen2) <=
                               0);
            return (ConditionAValid(eigenInfo) && conditionB) ? 1 : -1;
        });

        Vec2i seedGridSize = new Vec2i(32, 16) / 2;
        unfiltered.Clear();
        ParallelGrid.For(seedGridSize, CancellationToken.None, (i, j) =>
        {
            var domainRectBoundary = eigenInfoField.Domain.RectBoundary;
            var pos = domainRectBoundary.FromRelative(new Vec2(i, j) / (seedGridSize.ToVec2() - Vec2.One));

            if (pos.X < 0 || pos.X > 2 || pos.Y < 0 || pos.Y > 1) //double gyre only
                return;

            var dir = scaled_eigen2.Evaluate(pos);

            var cur = pos;
            var dt = .006;
            List<Vec2> positions = new();
            if (validSubspace.Evaluate(pos) < 0)
                return;

            positions.Add(cur);

            var invalidLength = 0.0;
            var totalLenght = 0.0;
            for (int k = 0; k < 1; k++)
            {
                int count = 0;
                if (k == 1)
                {
                    dir = -scaled_eigen2.Evaluate(pos);
                    positions.Reverse();
                    cur = positions.Last();
                }

                var last = cur;
                while (true)
                {
                    cur = IntegrateRk4(scaled_eigen2, dir, cur, dt);
                    dir = (last - cur) / dt;
                    positions.Add(cur);
                    var segmentLength = Vec2.Distance(last, cur);

                    count++;

                    if (count > 1250)
                        break;


                    totalLenght += segmentLength;
                    if (validSubspace.Evaluate(pos) <= 0)
                    {
                        invalidLength += segmentLength;
                    }
                    else invalidLength = 0;

                    if (invalidLength > l_f)
                        break;

                    if (Vec2.Distance(cur, pos) < dt * .01 && positions.Count > 5)
                        break;

                    if (!domainRectBoundary.Contains(cur))
                        break;
                }
            }

            if (totalLenght >= l_min)
                unfiltered.Add(new Trajectory<Vec2>(positions.ToArray()));
        });


        /*ParallelGrid.For(seedGridSize, CancellationToken.None, (i, j) =>
        {
            var domainRectBoundary = eigenInfoField.Domain.RectBoundary;
            var pos = domainRectBoundary.FromRelative(new Vec2(i, j) / (seedGridSize.ToVec2() - Vec2.One));
            var points = new List<Vec2>();
            pos
        }*/


        //.Discritize(gridSize);

        Artifacts.Clear();

        Artifacts.Register(new Artifact<IVectorField<Vec2, double>>(
            eigenInfoField.Select(s => s.Lambda1), "λ_1",
            "Lambda 1 of Cauchy-Green Strain Tensor"));

        Artifacts.Register(new Artifact<IVectorField<Vec2, double>>(
            eigenInfoField.Select(s => s.Lambda2), "λ_2",
            "Lambda 2 of Cauchy-Green Strain Tensor"));

        Artifacts.Register(new Artifact<IVectorField<Vec2, Vec2>>(
            eigenInfoField.Select(s => s.Eigen1), "Eigen Vector 1",
            "First eigenvector of Cauchy-Green Strain Tensor"));

        Artifacts.Register(new Artifact<IVectorField<Vec2, Vec2>>(
            eigenInfoField.Select(s => s.Eigen2), "Eigen Vector 2",
            "Second eigenvector of Cauchy-Green Strain Tensor"));

        Artifacts.Register(new Artifact<IVectorField<Vec2, Vec2>>(
            eigenInfoField.Select(s => s.Eigen2), "Eigen Vector 2",
            "Second eigenvector of Cauchy-Green Strain Tensor"));
    }

    private static ArbitraryField<Vec2, EigenInfo> EigenInfoField(ArbitraryField<Vec2, Tensor2D> cauchyGreenField)
    {
        return new ArbitraryField<Vec2, EigenInfo>(cauchyGreenField.Domain, p =>
        {
            var tensor = cauchyGreenField.Evaluate(p);
            double a = tensor.Row0.X;
            double b = tensor.Row0.Y;
            double c = tensor.Row1.X;
            double d = tensor.Row1.Y;

            // Characteristic polynomial:
            // λ² - trace(A) λ + det(A) = 0

            double trace = a + d;
            double det = a * d - b * c;

            double discriminant = Math.Sqrt(trace * trace - 4.0 * det);

            double lambda1 = 0.5 * (trace - discriminant);
            double lambda2 = 0.5 * (trace + discriminant);

            Vec2 v1 = ComputeEigenvector(a, b, c, d, lambda1);
            Vec2 v2 = ComputeEigenvector(a, b, c, d, lambda2);
            return new EigenInfo
            {
                Lambda1 = lambda1,
                Lambda2 = lambda2,
                Eigen1 = v1,
                Eigen2 = v2,
            };
        });
    }

    private static ArbitraryField<Vec2, Tensor2D> CachyGreenStrainField(IVectorField<Vec3, Vec2> velocity, IFlowOperator<Vec2, Vec3> flowOperator, double t0, double T, double h)
    {
        var domain = velocity.Domain.ReducedSlice<Vec3, Vec2>();

        Vec2 FlowOp(Vec2 x)
        {
            return flowOperator.ComputeEnd(t0, t0 + T, x, velocity);
        }

        var flowGradient = new ArbitraryField<Vec2, Tensor2D>(domain, x =>
        {
            var x_l = x + new Vec2(-h, 0);
            var x_r = x + new Vec2(+h, 0);
            var x_d = x + new Vec2(0, -h);
            var x_u = x + new Vec2(0, +h);
            return new Tensor2D()
            {
                Row0 = (FlowOp(x_r) - FlowOp(x_l)) / (2 * h),
                Row1 = (FlowOp(x_u) - FlowOp(x_d)) / (2 * h),
            };
        });

        return new ArbitraryField<Vec2, Tensor2D>(domain,
            x =>
            {
                var F = flowGradient.Evaluate(x);
                var Ft = F.Transpose();
                return F * Ft;
            });
    }

    public List<Trajectory<Vec2>> StrainLines = new();

    Vec2 EvaluateDirectionUnawareScaled(IVectorField<Vec2, Vec2> vectorfield, Vec2 initialDirection, Vec2 x)
    {
        var v = vectorfield.Evaluate(x);
        var dir = double.Sign(Vec2.Dot(v, initialDirection));
        return dir * v;
    }

    /*Vec2 Integrate(IVectorField<Vec2, EigenInfo> eigenField, Vec2 initialDirection, Vec2 x, double stepSize)
    {
        Vec2 Rs = EvaluateDirectionUnawareScaled(eigenField, initialDirection, x);
        return x + Rs * stepSize;
    }*/

    private static Vec2 ComputeEigenvector(
        double a, double b,
        double c, double d,
        double lambda)
    {
        Vec2 v;

        if (Math.Abs(b) > Math.Abs(c))
        {
            if (Math.Abs(b) < 1e-10)
            {
                v = Math.Abs(a - lambda) < Math.Abs(d - lambda)
                    ? new Vec2(1.0, 0.0)
                    : new Vec2(0.0, 1.0);
            }
            else
            {
                v = new Vec2(1.0, -(a - lambda) / b);
            }
        }
        else
        {
            if (Math.Abs(c) < 1e-10)
            {
                v = Math.Abs(a - lambda) < Math.Abs(d - lambda)
                    ? new Vec2(1.0, 0.0)
                    : new Vec2(0.0, 1.0);
            }
            else
            {
                v = new Vec2(-(d - lambda) / c, 1.0);
            }
        }

        return v.NormalizedSafe();
    }

    public override void Draw(View view)
    {
        foreach (var traj in unfiltered)
        foreach (var (start, end) in traj.EnumerateSegments())
        {
            //Gizmos2D.Instanced.RegisterLine(view.Camera2D, FlowOp(start), FlowOp(end), Color.Green, .009f);
            Gizmos2D.Instanced.RegisterLine(start, end, Color.Red, .001f);
        }

        Gizmos2D.Instanced.RenderRects(view.Camera2D);
        /*h = .01;
        Vec2 x = view.MousePosition;
        Vec2 x_prev = view.MousePosition;

        List<Vec2> positions = new List<Vec2>();
        positions.Add(x);

        int count = 0;
        var initialDir = EigenInfoField.Evaluate(x).Eigen1;
        if (!double.IsRealNumber(initialDir.X))
            return;

        for (int i = 0; i < count; i++)
        {
            if (!double.IsRealNumber(x.X))
                return;

            var x_new = IntegrateRk4(EigenInfoField, -initialDir, x, .003f);
            x = x_new;
            positions.Add(x);
        }

        positions.Reverse();
        x = positions.Last();
        for (int i = 0; i < count; i++)
        {
            if (!double.IsRealNumber(x.X))
                return;

            var x_new = IntegrateRk4(EigenInfoField, initialDir, x, .003f);
            x = x_new;
            positions.Add(x);
        }

        var traj = new Trajectory<Vec2>(positions.ToArray());
        var velocity = DataService.VectorField;
        var domain = new RectDomain<Vec2>(velocity.Domain.RectBoundary.Reduce<Vec2>());
        var FlowOp = (Vec2 x) => IFlowOperator<Vec2, Vec3>.Default.ComputeEnd(t0, t0 + T, x, velocity);

        foreach (var (start, end) in traj.EnumerateSegments())
        {
            Gizmos2D.Line(view.Camera2D, FlowOp(start), FlowOp(end), Color.Green, .009f);
            Gizmos2D.Line(view.Camera2D, (start), (end), Color.Red, .009f);
        }*/
    }

    public override void DrawImGuiSettings()
    {
        if (ImGui.Button("Recompute"))
        {
            Recompute();
        }

        base.DrawImGuiSettings();
    }
}

public class InputAttribute : Attribute
{
    public string Name { get; set; }
    public object Min { get; set; }
    public object Max { get; set; }
}

public class OutputAttribute : Attribute
{
    public string Name { get; set; }
}