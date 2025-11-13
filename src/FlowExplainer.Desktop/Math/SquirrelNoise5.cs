namespace FlowExplainer;

/// <summary>
/// SquirrelNoise5 - Squirrel's Raw Noise utilities (version 5)
///
/// This code is made available under the Creative Commons attribution 3.0 license (CC-BY-3.0 US):
/// Attribution in source code comments (even closed-source/commercial code) is sufficient.
/// License summary and text available at: https://creativecommons.org/licenses/by/3.0/us/
///
/// These noise functions were written by Squirrel Eiserloh as a cheap and simple substitute for
/// the [sometimes awful] bit-noise sample code functions commonly found on the web, many of which
/// are hugely biased or terribly patterned, e.g. having bits which are on (or off) 75% or even
/// 100% of the time (or are excessively overkill/slow for our needs, such as MD5 or SHA).
///
/// Note: This is work in progress; not all functions have been tested.  Use at your own risk.
/// Please report any bugs, issues, or bothersome cases to SquirrelEiserloh at gmail.com.
///
/// The following functions are all based on a simple bit-noise hash function which returns an
/// unsigned integer containing 32 reasonably-well-scrambled bits, based on a given (signed)
/// integer input parameter (position/index) and [optional] seed.  Kind of like looking up a
/// value in an infinitely large [non-existent] table of previously rolled random numbers.
///
/// These functions are deterministic and random-access / order-independent (i.e. state-free),
/// so they are particularly well-suited for use in smoothed/fractal/simplex/Perlin noise
/// functions and out-of-order (or on-demand) procedural content generation (i.e. that mountain
/// village is the same whether you generated it first or last, ahead of time or just now).
///
/// The N-dimensional variations simply hash their multidimensional coordinates down to a single
/// 32-bit index and then proceed as usual, so while results are not unique they should
/// (hopefully) not seem locally predictable or repetitive.
/// </summary>
public static class SquirrelNoise5
{
    const double ONE_OVER_MAX_UINT = 1.0 / uint.MaxValue;
    const double ONE_OVER_MAX_INT = 1.0 / int.MaxValue;

    /// <summary>
    /// Fast hash of an int32 into a different (unrecognizable) uint32.
    /// Returns an unsigned integer containing 32 reasonably-well-scrambled bits, based on the hash
    /// of a given (signed) integer input parameter (position/index) and [optional] seed. Kind of
    /// like looking up a value in an infinitely large table of previously generated random numbers.
    /// I call this particular approach SquirrelNoise5 (5th iteration of my 1D raw noise function).
    /// Many thanks to Peter Schmidt-Nielsen whose outstanding analysis helped identify a weakness
    /// in the SquirrelNoise3 code I originally used in my GDC 2017 talk, "Noise-based RNG".
    /// Version 5 avoids a noise repetition found in version 3 at extremely high position values
    /// caused by a lack of influence by some of the high input bits onto some of the low output bits.
    /// The revised SquirrelNoise5 function ensures all input bits affect all output bits, and to
    /// (for me) a statistically acceptable degree. I believe the worst-case here is in the amount
    /// of influence input position bit #30 has on output noise bit #0 (49.99%, vs. 50% ideal).
    /// </summary>
    static uint Hash(int position, uint seed)
    {
        const uint SQ5_BIT_NOISE1 = 0b11010010101010000000101000111111; // 11010010101010000000101000111111
        const uint SQ5_BIT_NOISE2 = 0b10101000100001001111000110010111; // 10101000100001001111000110010111
        const uint SQ5_BIT_NOISE3 = 0b01101100011100110110111101001011; // 01101100011100110110111101001011
        const uint SQ5_BIT_NOISE4 = 0b10110111100111110011101010111011; // 10110111100111110011101010111011
        const uint SQ5_BIT_NOISE5 = 0b00011011010101101100010011110101; // 00011011010101101100010011110101

        uint mangledBits = (uint)position;
        mangledBits *= SQ5_BIT_NOISE1;
        mangledBits += seed;
        mangledBits ^= mangledBits >> 9;
        mangledBits += SQ5_BIT_NOISE2;
        mangledBits ^= mangledBits >> 11;
        mangledBits *= SQ5_BIT_NOISE3;
        mangledBits ^= mangledBits >> 13;
        mangledBits += SQ5_BIT_NOISE4;
        mangledBits ^= mangledBits >> 15;
        mangledBits *= SQ5_BIT_NOISE5;
        mangledBits ^= mangledBits >> 17;

        return mangledBits;
    }

    /// <summary> Returns a random double in the range [-1, 1] based on the given 1D position and seed. </summary>
    public static double Noise1D(int x, uint seed = 0)
        => (double)(ONE_OVER_MAX_INT * (int)Hash(x, seed));

    /// <summary> Returns a random double in the range [-1, 1] based on the given 2D position and seed. </summary>
    public static double Noise2D(int x, int y, uint seed = 0)
        => (double)(ONE_OVER_MAX_INT * (int)UInt.Noise2D(x, y, seed));

    /// <summary> Returns a random double in the range [-1, 1] based on the given 3D position and seed. </summary>
    public static double Noise3D(int x, int y, int z, uint seed = 0)
        => (double)(ONE_OVER_MAX_INT * (int)UInt.Noise3D(x, y, z, seed));

    /// <summary> Returns a random double in the range [-1, 1] based on the given 4D position and seed. </summary>
    public static double Noise4D(int x, int y, int z, int w, uint seed = 0)
        => (double)(ONE_OVER_MAX_INT * (int)UInt.Noise4D(x, y, z, w, seed));

    /// <summary>
    /// Variants of Squirrel5 which return an unsigned integer instead of a doubleing point number.
    /// </summary>
    public static class UInt
    {
        /// <summary> Returns a random unsigned integer based on the given 1D position and seed. </summary>
        public static uint Noise1D(int x, uint seed = 0)
            => Hash(x, seed);

        /// <summary> Returns a random unsigned integer based on the given 2D position and seed. </summary>
        public static uint Noise2D(int x, int y, uint seed = 0)
        {
            const int PRIME_NUMBER = 198491317; // Large prime number with non-boring bits
            return Hash(x + PRIME_NUMBER * y, seed);
        }

        /// <summary> Returns a random unsigned integer based on the given 3D position and seed. </summary>
        public static uint Noise3D(int x, int y, int z, uint seed = 0)
        {
            const int PRIME1 = 198491317; // Large prime number with non-boring bits
            const int PRIME2 = 6542989;   // Large prime number with distinct and non-boring bits
            return Hash(x + PRIME1 * y + PRIME2 * z, seed);
        }

        /// <summary> Returns a random unsigned integer based on the given 4D position and seed. </summary>
        public static uint Noise4D(int x, int y, int z, int w, uint seed = 0)
        {
            const int PRIME1 = 198491317; // Large prime number with non-boring bits
            const int PRIME2 = 6542989;   // Large prime number with distinct and non-boring bits
            const int PRIME3 = 357239;    // Large prime number with distinct and non-boring bits
            return Hash(x + PRIME1 * y + PRIME2 * z + PRIME3 * w, seed);
        }
    }

    /// <summary>
    /// Variants of Squirrel5 which return a double in the range [0, 1] instead of [-1, 1].
    /// </summary>
    public static class ZeroToOne
    {
        /// <summary> Returns a random double in the range [0, 1] based on the given 1D position and seed. </summary>
        public static double Noise1D(int x, uint seed = 0)
            => (double)(ONE_OVER_MAX_UINT * Hash(x, seed));

        /// <summary> Returns a random double in the range [0, 1] based on the given 2D position and seed. </summary>
        public static double Noise2D(int x, int y, uint seed = 0)
            => (double)(ONE_OVER_MAX_UINT * UInt.Noise2D(x, y, seed));

        /// <summary> Returns a random double in the range [0, 1] based on the given 3D position and seed. </summary>
        public static double Noise3D(int x, int y, int z, uint seed = 0)
            => (double)(ONE_OVER_MAX_UINT * UInt.Noise3D(x, y, z, seed));

        /// <summary> Returns a random double in the range [0, 1] based on the given 4D position and seed. </summary>
        public static double Noise4D(int x, int y, int z, int w, uint seed = 0)
            => (double)(ONE_OVER_MAX_UINT * UInt.Noise4D(x, y, z, w, seed));
    }
}