using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FlowExplainer;

public class VariationalLCS : WorldService
{
    public struct EigenInfo : IMultiplyOperators<EigenInfo, double, EigenInfo>, IAdditionOperators<EigenInfo, EigenInfo, EigenInfo>
    {
        public Vec2 Eigen1;
        public Vec2 Eigen2;
        public double Lambda1;
        public double Lambda2;
        public static EigenInfo operator *(EigenInfo left, double right)
        {
            return new EigenInfo
            {
                Eigen1 = left.Eigen1 * right,
                Eigen2 = left.Eigen2 * right,
                Lambda1 = left.Lambda1 * right,
                Lambda2 = left.Lambda2 * right
            };
        }
        public static EigenInfo operator +(EigenInfo left, EigenInfo right)
        {
            return new EigenInfo
            {
                Eigen1 = left.Eigen1 + right.Eigen1,
                Eigen2 = left.Eigen2 + right.Eigen2,
                Lambda1 = left.Lambda1 + right.Lambda1,
                Lambda2 = left.Lambda2 + right.Lambda2,
            };
        }
    }

    public struct Tensor2D : IMultiplyOperators<Tensor2D, double, Tensor2D>, IAdditionOperators<Tensor2D, Tensor2D, Tensor2D>
    {
        public Vec2 Row0;
        public Vec2 Row1;

        public Tensor2D Transpose()
        {
            return new Tensor2D()
            {
                Row0 = new Vec2(Row0.X, Row1.X),
                Row1 = new Vec2(Row0.Y, Row1.Y),
            };
        }

        public static Tensor2D operator *(Tensor2D left, Tensor2D right)
        {
            return new Tensor2D
            {
                Row0 = new Vec2(
                    left.Row0.X * right.Row0.X + left.Row0.Y * right.Row1.X,
                    left.Row0.X * right.Row0.Y + left.Row0.Y * right.Row1.Y
                ),
                Row1 = new Vec2(
                    left.Row1.X * right.Row0.X + left.Row1.Y * right.Row1.X,
                    left.Row1.X * right.Row0.Y + left.Row1.Y * right.Row1.Y
                )
            };
        }

        public static Tensor2D operator *(Tensor2D left, double right)
        {
            return new Tensor2D
            {
                Row0 = left.Row0 * right,
                Row1 = left.Row1 * right,
            };
        }

        public static Tensor2D operator +(Tensor2D left, Tensor2D right)
        {
            return new Tensor2D
            {
                Row0 = left.Row0 + right.Row0,
                Row1 = left.Row1 + right.Row1,
            };
        }
    }

    public ArbitraryField<Vec2, Tensor2D> CauchyGreenField { get; set; }

    public static bool ConditionAValid(EigenInfo eigenInfo)
    {
        return double.Abs(eigenInfo.Lambda1 - eigenInfo.Lambda2) > 0.00001 && eigenInfo.Lambda2 > 1;
    }

    
    Vec2 IntegrateRk4(IVectorField<Vec2, EigenInfo> f, Vec2 initDir, Vec2 p, double dt)
    {
        var k1 = EvaluateDirectionUnawareScaled(f, initDir, p);
        var k2 = EvaluateDirectionUnawareScaled(f, initDir, p + (k1 / 2.0) * dt);
        var k3 = EvaluateDirectionUnawareScaled(f, initDir, p + (k2 / 2.0) * dt);
        var k4 = EvaluateDirectionUnawareScaled(f, initDir, p + (k3) * dt);
        return p + (dt / 6.0) * (k1 + 2.0 * k2 + 2.0 * k3 + k4);
    }

    double t0 = 0;
    double T = 20.0;
    
    double h = .1;
    public override void Initialize()
    {
        var velocity = DataService.VectorField;
        var domain = new RectDomain<Vec2>(velocity.Domain.RectBoundary.Reduce<Vec2>());
        var FlowOp = (Vec2 x) => IFlowOperator<Vec2, Vec3>.Default.ComputeEnd(t0, t0 + T, x, velocity);
        var flowGradient = new ArbitraryField<Vec2, Tensor2D>(domain, x =>
        {
            /*if (x.Y <=h*10|| x.X <= h*10)
                return default;*/
            
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

        CauchyGreenField = new ArbitraryField<Vec2, Tensor2D>(domain,
            x =>
            {
                var F = flowGradient.Evaluate(x);
                var Ft = F.Transpose();
                return F * Ft;
            });
        var rectDomain = new RectDomain<Vec3>(CauchyGreenField.Domain.RectBoundary.Min.Up(0), CauchyGreenField.Domain.RectBoundary.Max.Up(1));
        var gridSize = new Vec2i(1024/6, 512/6);
        EigenInfoField = new ArbitraryField<Vec2, EigenInfo>(new RectDomain<Vec2>(rectDomain.Rect.Reduce<Vec2>()), p =>
        {
            var tensor = CauchyGreenField.Evaluate(p);
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

        var lambda2Field = EigenInfoField.Select(s => s.Lambda2);
        var hessianLambda2 = new ArbitraryField<Vec2, Matrix2>(lambda2Field.Domain, x => lambda2Field.Hessian(x, h/2));
        
        Vec2 seedGridSize = new Vec2(10, 5);
        for (int i = 0; i < seedGridSize.X; i++)
        for (int j = 0; j < seedGridSize.Y; j++)
        {
            var pos = domain.Rect.FromRelative(new Vec2(i, j) / (seedGridSize - Vec2.One));
        }

        conditionBField = new ArbitraryField<Vec2, double>(lambda2Field.Domain, x =>
        {
            var eigenInfo = EigenInfoField.Evaluate(x);
            var cond = Vec2.Dot(
                           eigenInfo.Eigen2,
                           hessianLambda2.Evaluate(x) * eigenInfo.Eigen2) <=
                       0;
            return cond ? 1 : 0;
        });
        
        //.Discritize(gridSize);

    }

    public List<Trajectory<Vec2>> StrainLines = new();
    private IVectorField<Vec2, EigenInfo> EigenInfoField;
    private IVectorField<Vec2, double> conditionBField;

    public override IEnumerable<ISelectableVectorField<Vec2, Vec2>> GetSelectableVec2Vec2()
    {
        yield return new SelectableVectorField<Vec2, Vec2>("Eigen 1", EigenInfoField.Select(d => d.Eigen1));
        yield return new SelectableVectorField<Vec2, Vec2>("Eigen 2", EigenInfoField.Select(d => d.Eigen2));
        yield return new SelectableVectorField<Vec2, Vec2>("Eigen 2", EigenInfoField.Select(d => d.Eigen2));
    }

    public override IEnumerable<ISelectableVectorField<Vec2, double>> GetSelectableVec2Vec1()
    {
        yield return new SelectableVectorField<Vec2, double>("Lambda 1", EigenInfoField.Select(d => d.Lambda1));
        yield return new SelectableVectorField<Vec2, double>("Lambda 2", EigenInfoField.Select(d => d.Lambda2));
        yield return new SelectableVectorField<Vec2, double>("Condition B", conditionBField);
        yield return new SelectableVectorField<Vec2, double>("Condition B", conditionBField);
        yield return new SelectableVectorField<Vec2, double>("FTLE",
            EigenInfoField.Select(d =>  (1f / double.Abs(T)) * double.Log(double.Sqrt(double.Max(d.Lambda1, d.Lambda2)))));
        
    }

    Vec2 EvaluateDirectionUnawareScaled(IVectorField<Vec2, EigenInfo> eigenField, Vec2 initialDirection, Vec2 x)
    {
        var dat = eigenField.Evaluate(x);
        double alpha = double.Pow((dat.Lambda2 - dat.Lambda1) / (dat.Lambda2 + dat.Lambda1), 2);
        if (!double.IsRealNumber(dat.Eigen1.X))
            return default;
        var dir = double.Sign(Vec2.Dot(dat.Eigen1, initialDirection));
        var Rs = dir * alpha * dat.Eigen1;
        return Rs;
    }

    Vec2 Integrate(IVectorField<Vec2, EigenInfo> eigenField, Vec2 initialDirection, Vec2 x, double stepSize)
    {
        Vec2 Rs = EvaluateDirectionUnawareScaled(eigenField, initialDirection, x);
        return x + Rs * stepSize;
    }

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
        h = .01;
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
            Gizmos2D.Line(view.Camera2D, (start),(end), Color.Red, .009f);
        }
    }
}