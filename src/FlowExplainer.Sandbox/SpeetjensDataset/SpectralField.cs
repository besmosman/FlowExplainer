using System.Numerics;

namespace FlowExplainer;

public class SpectralField : IVectorField<Vec3, double>
{
    public RegularGrid<Vec3i, Complex> Usps;
    public Rect<Vec3> Rect { get; set; }
    public IDomain<Vec3> Domain => new RectDomain<Vec3>(Rect);
    public IBounding<Vec3> Bounding { get; set; }

    double Pi5 = 0.5f; // probaly Domain in Y axis right?

    public SpectralField(RegularGrid<Vec3i, Complex> usps, Rect<Vec3> rect)
    {
        Usps = usps;
        Rect = rect;
        Bounding = BoundingFunctions.Build(
            [BoundaryType.Periodic, BoundaryType.Fixed, BoundaryType.Fixed], Rect);
    }

    public static double PhysicalSpectral(double x, double Pi5)
    {
        return 2 * x / Pi5 - 1;
    }


    public double Evaluate(Vec3 x)
    {
        return InterpFourCheb(x);
    }

    public bool TryEvaluate(Vec3 x, out double value)
    {
        value = InterpFourCheb(x);
        return true;
    }


    static Complex Lerp(Complex a, Complex b, double t) => a + (b - a) * t;


    private double InterpFourCheb(Vec3 pos)
    {
        var t = pos.Last;
        var stepF = (t - Rect.Min.Z) / Rect.Size.Z * Usps.GridSize.Z;
        int t0 = (int)(double.Floor(stepF));
        int t1 = t0 + 1;
        if (t1 >= Usps.GridSize.Z)
            t1 = Usps.GridSize.Z - 1;

        double c = stepF % 1;
        int N = Usps.GridSize.X - 1;
        int M = 2 * (Usps.GridSize.Y - 1);
        Vec2 spectralPos = new Vec2(pos.X * double.Pi, PhysicalSpectral(pos.Y, Pi5));
        double acosy = double.Acos(spectralPos.Y);
        var exp = Complex.Exp(Complex.ImaginaryOne * spectralPos.X);
        Complex u = Complex.Zero;
        for (int p = 0; p <= N; p++)
        {
            int n = p;
            var usp = Lerp(Usps[new Vec3i(p, 0, t0)], Usps[new Vec3i(p, 0, t1)], c);
            u += usp * double.Cos(n * acosy);
        }

        for (int p = 0; p <= N; p++)
        for (int k = 1; k < M / 2; k++)
        {
            var n = p;
            var m = k;
            var usp = Lerp(Usps[new Vec3i(p, k, t0)], Usps[new Vec3i(p, k, t1)], c);
            var dU = usp * Complex.Pow(exp, -m) * double.Cos(n * acosy);
            u += dU + Complex.Conjugate(dU);
        }

        for (int n = 0; n <= N; n++)
        {
            var usp = Lerp(Usps[new Vec3i(n, M / 2, t0)], Usps[new Vec3i(n, M / 2, t1)], c);
            u += usp * Complex.Pow(exp, (M / 2f)) * double.Cos(n * acosy);
        }


        return (double)u.Real;
    }
    
}