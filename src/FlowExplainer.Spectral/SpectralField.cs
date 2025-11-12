using System.Numerics;

namespace FlowExplainer;

public class SpectralField : IVectorField<Vec3, float>
{
    public RegularGrid<Vec3i, Complex> Usps;
    public Rect<Vec3> Rect { get; set; }
    public IDomain<Vec3> Domain => new RectDomain<Vec3>(Rect);
    public IBounding<Vec3> Bounding { get; set; }

    float Pi5 = 0.5f; // probaly Domain in Y axis right?

    public SpectralField(RegularGrid<Vec3i, Complex> usps, Rect<Vec3> rect)
    {
        Usps = usps;
        Rect = rect;
        Bounding = BoundingFunctions.Build(
            [BoundaryType.Periodic, BoundaryType.Fixed, BoundaryType.Fixed], Rect);
    }

    public static float PhysicalSpectral(float x, float Pi5)
    {
        return 2 * x / Pi5 - 1;
    }


    public float Evaluate(Vec3 x)
    {
        return InterpFourCheb(x);
    }

    public bool TryEvaluate(Vec3 x, out float value)
    {
        value = InterpFourCheb(x);
        return true;
    }


    static Complex Lerp(Complex a, Complex b, float t) => a + (b - a) * t;


    private float InterpFourCheb(Vec3 pos)
    {
        var t = pos.Last;
        var stepF = (t - Rect.Min.Z) / Rect.Size.Z * Usps.GridSize.Z;
        int t0 = (int)(float.Floor(stepF));
        int t1 = t0 + 1;
        if (t1 >= Usps.GridSize.Z)
            t1 = Usps.GridSize.Z - 1;

        float c = stepF % 1;
        int N = Usps.GridSize.X - 1;
        int M = 2 * (Usps.GridSize.Y - 1);
        Vec2 spectralPos = new Vec2(pos.X * float.Pi, PhysicalSpectral(pos.Y, Pi5));
        float acosy = float.Acos(spectralPos.Y);
        var exp = Complex.Exp(Complex.ImaginaryOne * spectralPos.X);
        Complex u = Complex.Zero;
        for (int p = 0; p <= N; p++)
        {
            int n = p;
            var usp = Lerp(Usps[new Vec3i(p, 0, t0)], Usps[new Vec3i(p, 0, t1)], c);
            u += usp * float.Cos(n * acosy);
        }

        for (int p = 0; p <= N; p++)
        for (int k = 1; k < M / 2; k++)
        {
            var n = p;
            var m = k;
            var usp = Lerp(Usps[new Vec3i(p, k, t0)], Usps[new Vec3i(p, k, t1)], c);
            var dU = usp * Complex.Pow(exp, -m) * float.Cos(n * acosy);
            u += dU + Complex.Conjugate(dU);
        }

        for (int n = 0; n <= N; n++)
        {
            var usp = Lerp(Usps[new Vec3i(n, M / 2, t0)], Usps[new Vec3i(n, M / 2, t1)], c);
            u += usp * Complex.Pow(exp, (M / 2f)) * float.Cos(n * acosy);
        }


        return (float)u.Real;
    }

    //So this may be dumb. not sure if these complex numbers can simply be linearly interpolated like this
    private Complex Lerp(Complex a, Complex b, double f)
    {
        return new Complex(
            Utils.Lerp(a.Real, b.Real, f),
            Utils.Lerp(a.Imaginary, b.Imaginary, f));
    }
}