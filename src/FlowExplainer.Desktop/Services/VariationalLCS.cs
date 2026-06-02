using System.Numerics;
using ImGuiNET;

namespace FlowExplainer;



public class VariationalLCS : WorldService
{
    [Input] public Artifact<IVectorField<Vec3, Vec2>>? VelocityField;
    public override string? Name => "Variational LCS";

    [Input(Min = 0.0, Max = 1.0)] public double l_f = 0.2;
    [Input(Min = 0.0, Max = 1.0)] public double T = 20;
    [Input(Min = 0.0, Max = 1.0)] public double l_min = .4;
    public double t0 = 0;
    [Input(Min = 0.0, Max = 0.01)] double dF = 10e-4;
    [Input(Min = 0.0, Max = 0.01)] double dHessian = 0.004;


    private bool lowQuality = true;


    private List<Vec2> points = new List<Vec2>();

    public static bool ConditionAValid(EigenInfo eigenInfo)
    {
        return double.Abs(eigenInfo.Lambda1 - eigenInfo.Lambda2) > 0.000000000000000000001 && eigenInfo.Lambda2 > 1;
    }


    static Vec2 IntegrateRk4(IVectorField<Vec2, Vec2> f, Vec2 lastDir, Vec2 p, double dt)
    {
        var k1 = EvaluateDirectionUnawareScaled(f, lastDir, p);
        var k2 = EvaluateDirectionUnawareScaled(f, k1, p + (k1 / 2.0) * dt);
        var k3 = EvaluateDirectionUnawareScaled(f, k2, p + (k2 / 2.0) * dt);
        var k4 = EvaluateDirectionUnawareScaled(f, k3, p + (k3) * dt);
        return p + (dt / 6.0) * (k1 + 2.0 * k2 + 2.0 * k3 + k4);
    }


    public override void Initialize()
    {
        //VelocityField ??= DataService.Artifacts.Get<IVectorField<Vec3, Vec2>>("Velocity");
    }

    private List<Trajectory<Vec2>> trajs = new();

    private bool attracting = false;

    public void Recompute()
    {
        /*Artifacts.RegisterOrUpdate(new Artifact<TrajectoryGroup<Vec2>>(
            new TrajectoryGroup<Vec2>([new Trajectory<Vec2>([Vec2.Zero, Vec2.One])]), "LCS", ""));
        return;*/
        /*dF = 10e-3;
        dHessian = dF / 2;*/
        var velocity = VelocityField!.Value;
        var flowOperator = new IFlowOperator<Vec2, Vec3>.DefaultFlowOperatorUnsteady(64);

        var cauchyGreenField = CachyGreenStrainField(velocity, flowOperator, t0, T, dF);

        var gridSize = new Vec2i(256, 128) * 3;

        IVectorField<Vec2, double> validSubspace;
        IVectorField<Vec2, Vec2> eigenVector1;
        IVectorField<Vec2, Vec2> eigenVector2;
        IVectorField<Vec2, Vec2> scaledEigenVector2;
        IVectorField<Vec2, Vec2> scaledEigenVector2Perp;
        IVectorField<Vec2, double> lambda2Field;
        {
            var info = EigenInfoField(cauchyGreenField);
            lambda2Field = info.Select(s => s.Lambda2);
            var hessianLambda = new ArbitraryField<Vec2, Matrix2>(lambda2Field.Domain, x => lambda2Field.Hessian(x, dHessian));

            eigenVector1 = info.Select(s => s.Eigen1);
            eigenVector2 = info.Select(s => s.Eigen2);

            scaledEigenVector2 = new ArbitraryField<Vec2, Vec2>(lambda2Field.Domain, (x) =>
            {
                var n = info.Evaluate(x);
                var lamdbaMax = n.Lambda2;
                var lamdbaMin = n.Lambda1;
                if (lamdbaMax - Double.Sign(lamdbaMax) <= 0)
                    return default;
                var alpha = double.Pow((lamdbaMax - lamdbaMin) / (lamdbaMax + lamdbaMin), 2);
                return alpha * eigenVector2.Evaluate(x);
            });

            validSubspace = new ArbitraryField<Vec2, double>(lambda2Field.Domain, x =>
            {
                var eigenInfo = info.Evaluate(x);
                bool conditionB = (Vec2.Dot(
                                       eigenInfo.Eigen2,
                                       hessianLambda.Evaluate(x) * eigenInfo.Eigen2) <=
                                   0);

                return (ConditionAValid(eigenInfo) && conditionB) ? 1 : 0;
            });

            var hh = .01;
            var lnLambda2 = lambda2Field.Select(s => double.Log(s));
            eigen2Grad = new ArbitraryField<Vec2, Vec2>(info.Domain, x => lnLambda2.FiniteDifferenceGradient(x, hh));
        }


        if (lowQuality)
        {
            var orientedScaledEigen2 = scaledEigenVector2.Discritize(gridSize);
            orientedScaledEigen2.GridField.Interpolator = new OrientedLinearInterpolation();
            eigenVector2 = eigenVector2.Discritize(gridSize);
            scaledEigenVector2 = orientedScaledEigen2;
            lambda2Field = lambda2Field.Discritize(gridSize);
            validSubspace = validSubspace.Discritize(gridSize);
        }

        scaledEigenVector2Perp = new ArbitraryField<Vec2, Vec2>(scaledEigenVector2.Domain, x =>
        {
            var r = scaledEigenVector2.Evaluate(x);
            return new Vec2(-r.Y, r.X);
        });

        eigen2Grad = eigen2Grad.Discritize(gridSize);


        localMaximaParticles.Clear();

        Vec2i sseedGridSize = new Vec2i(32, 16) * 2;
        for (int i = 0; i < sseedGridSize.X; i++)
        for (int j = 0; j < sseedGridSize.Y; j++)
        {
            var domainRectBoundary = eigen2Grad.Domain.RectBoundary;
            var pos = domainRectBoundary.FromRelative(new Vec2(i, j) / (sseedGridSize.ToVec2() - Vec2.One));
            localMaximaParticles.Add(new Particle()
            {
                pos = pos
            });
        }


        var cellSize = (scaledEigenVector2.Domain.RectBoundary.Size / sseedGridSize);
        var stepSize = .00001;
        double d = .0001;
        //var steps = (double.Max(cellSize.X, cellSize.Y) / stepSize) * 20;
        var steps = 20_00;
        for (int i = 0; i < steps; i++)
        {
            var rk4 = IIntegrator<Vec2, Vec2>.Rk4Steady;
            Parallel.ForEach(localMaximaParticles, p => { p.pos = rk4.Integrate(eigen2Grad, p.pos, stepSize); });
        }

        for (int i = localMaximaParticles.Count - 1; i >= 0; i--)
        {
            var p = localMaximaParticles[i];
            if (double.Abs(Vec2.Dot(eigen2Grad.Evaluate(p.pos), eigenVector2.Evaluate(p.pos))) > d)
            {
                localMaximaParticles.RemoveAt(i);
            }
        }

        if (true)
        {
            for (int i = 0; i < localMaximaParticles.Count; i++)
            {
                for (int j = localMaximaParticles.Count - 1; j > i; j--)
                {
                    if (Vec2.Distance(localMaximaParticles[i].pos, localMaximaParticles[j].pos) < 2 * d)
                    {
                        localMaximaParticles.RemoveAt(j);
                    }
                }
            }

            trajs.Clear();
            localMaximaParticles = localMaximaParticles.Select(s => (s, lambda2Field.Evaluate(s.pos))).OrderBy(o => o.Item2).Select(o => o.s).ToList();
            while (localMaximaParticles.Count > 0)
            {
                var particle = localMaximaParticles[0];
                var traj = IntegrateStrainLineFromLocalMax(particle.pos, scaledEigenVector2Perp, validSubspace, l_f, l_min);
                localMaximaParticles.RemoveAt(0);

                if (traj.HasValue)
                {
                    trajs.Add(traj.Value);
                    for (int i = localMaximaParticles.Count - 1; i >= 0; i--)
                    {
                        var dis = traj.Value.ClosestDistanceToLine(localMaximaParticles[i].pos);
                        if (dis < .01)
                        {
                            localMaximaParticles.RemoveAt(i);
                        }
                    }
                }
            }
        }

        /*
        Artifacts.RegisterOrUpdate(new Artifact<IVectorField<Vec2, double>>(
            eigenInfoField.Select(s => double.Log(s.Lambda1)), "log(λ_1)",
            "Lambda 1 of Cauchy-Green Strain Tensor"));

        Artifacts.RegisterOrUpdate(new Artifact<IVectorField<Vec2, double>>(
            eigenInfoField.Select(s => double.Log(s.Lambda2)), "log(λ_2)",
            "Lambda 2 of Cauchy-Green Strain Tensor"));
            */

        Artifacts.RegisterOrUpdate(new Artifact<IVectorField<Vec2, double>>(
            validSubspace, "Valid Subspace",
            "Conditions A and B"));

        Artifacts.RegisterOrUpdate(new Artifact<IVectorField<Vec2, double>>(
            lambda2Field, "Lambda 1", ""));

        Artifacts.RegisterOrUpdate(new Artifact<IVectorField<Vec2, double>>(
            lambda2Field, "Lambda 2", ""));
        
        Artifacts.RegisterOrUpdate(new Artifact<IVectorField<Vec2, Vec2>>(
            scaledEigenVector2, "Scaled Eigen Vector 2", ""));

        /*Artifacts.RegisterOrUpdate(new Artifact<IVectorField<Vec2, Vec2>>(
            scaledEigenVector2Perp, "Scaled Eigen Vector 2 Perp", ""));*/

        Artifacts.RegisterOrUpdate(new Artifact<TrajectoryGroup<Vec2>>(
            new TrajectoryGroup<Vec2>(trajs.ToArray()), "LCS", ""));
        
        /*
        Artifacts.RegisterOrUpdate(new Artifact<IVectorField<Vec2, Vec2>>(
            eigen2Grad, "Lambda 2 Gradient",
            "Conditions A and B"));
            */


        Artifacts.RegisterOrUpdate(new Artifact<IVectorField<Vec2, Vec2>>(eigenVector1.Discritize(gridSize),
            "Eigen Vector 1", ""));

        Artifacts.RegisterOrUpdate(new Artifact<IVectorField<Vec2, Vec2>>(eigenVector2.Discritize(gridSize),
            "Eigen Vector 2", ""));

        /*Artifacts.RegisterOrUpdate(new Artifact<IVectorField<Vec2, double>>(
            eigenInfoField.Select(s => (1f / double.Abs(T)) * double.Log(double.Sqrt(double.Max(s.Lambda1, s.Lambda2)))), "FTLE",
            "FTLE"));

        Artifacts.RegisterOrUpdate(new Artifact<IVectorField<Vec2, Vec2>>(
            eigenInfoField.Select(s => s.Eigen1), "Eigen Vector 1",
            "First eigenvector of Cauchy-Green Strain Tensor"));

        Artifacts.RegisterOrUpdate(new Artifact<IVectorField<Vec2, Vec2>>(
            eigenInfoField.Select(s => s.Eigen2), "Eigen Vector 2",
            "Second eigenvector of Cauchy-Green Strain Tensor"));*/
    }

    public static Trajectory<Vec2>? IntegrateStrainLineFromLocalMax(Vec2 startPos, IVectorField<Vec2, Vec2> scaled_eigen2, IVectorField<Vec2, double> validSubspace, double l_f, double l_min)
    {
        var rect = scaled_eigen2.Domain.RectBoundary;


        var lastDir = scaled_eigen2.Evaluate(startPos);

        var cur = startPos;
        var dt = .01;
        int maxsteps = 180_000;

        List<Vec2> positions = new();
        if (validSubspace.Evaluate(startPos) < .5)
            return null;

        positions.Add(cur);

        var invalidLength = 0.0;
        var totalLenght = 0.0;
        for (int k = 0; k < 2; k++)
        {
            int count = 0;
            if (k == 1)
            {
                lastDir = -scaled_eigen2.Evaluate(startPos);
                positions.Reverse();
                cur = positions.Last();
            }

            for (int _ = 0; _ < maxsteps; _++)
            {
                var last = cur;
                cur = IntegrateRk4(scaled_eigen2, lastDir, cur, dt);
                lastDir = (cur - last);
                positions.Add(cur);
                var segmentLength = Vec2.Distance(last, cur);

                totalLenght += segmentLength;
                if (validSubspace.Evaluate(cur) < .5)
                {
                    invalidLength += segmentLength;
                }
                else invalidLength = 0;

                if (invalidLength > l_f)
                    break;

                if (Vec2.Distance(cur, startPos) < dt * .01 && positions.Count > 5)
                    break;

                if (!rect.Contains(cur))
                    break;
            }
        }

        if (totalLenght >= l_min)
            return new Trajectory<Vec2>(positions.ToArray());
        else return null;
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
    //private IVectorField<Vec2, EigenInfo> eigenInfoField;

    static Vec2 EvaluateDirectionUnawareScaled(IVectorField<Vec2, Vec2> vectorfield, Vec2 lastDirection, Vec2 x)
    {
        var v = vectorfield.Evaluate(x);
        var dir = double.Sign(Vec2.Dot(v, lastDirection));
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

    class Particle
    {
        public Vec2 pos;
    }

    private List<Particle> localMaximaParticles = new();
    private IVectorField<Vec2, Vec2> eigen2Grad;

    public override void Draw(View view)
    {
        /*var hh = .0001;
        var rk4 = IIntegrator<Vec2, Vec2>.Rk4Steady;
        Parallel.ForEach(localMaximaParticles, p => { p.pos = rk4.Integrate(eigen2Grad, p.pos, hh); });*/
        foreach (var p in localMaximaParticles)
        {
            Gizmos2D.Instanced.RegisterCircle(p.pos, .002f, Color.Red);
        }

        Gizmos2D.Instanced.RenderCircles(view.Camera2D);
        foreach (var traj in trajs)
        foreach (var (start, end) in traj.EnumerateSegments())
        {
            //Gizmos2D.Instanced.RegisterLine(view.Camera2D, FlowOp(start), FlowOp(end), Color.Green, .009f);
            //var lambda2 = eigenInfoField.Evaluate((start + end) / 2).Lambda2;
            Gizmos2D.Instanced.RegisterLine(start, end, Color.White, .002f);
        }

        Gizmos2D.Instanced.RenderRects(view.Camera2D);
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