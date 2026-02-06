using System.Numerics;

namespace FlowExplainer;

public class SimpleSpectral : WorldService
{
    public class Point
    {
        public int N;
        public float WorldPos;
        public float Temprature;
        public Complex Complex;
    }

    public Point[] GridPoints = [];
    
    public override void Initialize()
    {
        int n = 10;
        GridPoints = new Point[n];
        for (int i = 0; i < n; i++)
        {
            GridPoints[i] = new Point()
            {
                WorldPos = i / (float)n,
                N = i,
                Temprature = 1,
            };
        }
        
        var points = FFT.FFTRealForward(GridPoints.Select(s => s.Temprature).ToArray());
        for (int i = 0; i < GridPoints.Length; i++)
        {
            GridPoints[i].Complex = points[i];
        }
        
        //e^a * k^2 * dt
    }
    
    
    public static class FFT
    {
        public static Complex[] FFTRealForward(float[] input)
        {
            int n = input.Length;
            Complex[] data = new Complex[n];
            for (int i = 0; i < n; i++)
                data[i] = new Complex(input[i], 0);

            Transform(data);
            return data;
        }

        // Inverse FFT using separate real + imag arrays
        public static float[] FFTRealInverse(float[] real, float[] imag)
        {
            int n = real.Length;
            Complex[] data = new Complex[n];
            for (int i = 0; i < n; i++)
                data[i] = new Complex(real[i], imag[i]);

            Transform(data, inverse: true);

            float[] output = new float[n];
            for (int i = 0; i < n; i++)
                output[i] = (float)data[i].Real;
            return output;
        }

        // Internal complex FFT (same as before)
        private static void Transform(Complex[] data, bool inverse = false)
        {
            int n = data.Length;
            if ((n & (n - 1)) != 0)
                throw new ArgumentException("Length must be a power of 2");

            // Bit-reversal
            int j = 0;
            for (int i = 1; i < n; i++)
            {
                int bit = n >> 1;
                while ((j & bit) != 0)
                {
                    j ^= bit;
                    bit >>= 1;
                }
                j ^= bit;
                if (i < j)
                {
                    var tmp = data[i];
                    data[i] = data[j];
                    data[j] = tmp;
                }
            }

            // FFT
            for (int len = 2; len <= n; len <<= 1)
            {
                float ang = 2f * MathF.PI / len * (inverse ? 1 : -1);
                Complex wlen = new Complex(Math.Cos(ang), Math.Sin(ang));
                for (int i = 0; i < n; i += len)
                {
                    Complex w = Complex.One;
                    for (int k = 0; k < len / 2; k++)
                    {
                        Complex u = data[i + k];
                        Complex v = data[i + k + len / 2] * w;
                        data[i + k] = u + v;
                        data[i + k + len / 2] = u - v;
                        w *= wlen;
                    }
                }
            }

            if (inverse)
            {
                for (int i = 0; i < n; i++)
                    data[i] /= n;
            }
        }
    }

    public override void Draw(View view)
    {
    }
}