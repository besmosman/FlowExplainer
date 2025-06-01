using System.Runtime.CompilerServices;

namespace FlowExplainer;

/*public unsafe struct Matrix4x4 : IEquatable<Matrix4x4>
{
    public fixed float values[16];


    public float M11
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => values[0];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => values[0] = value;
    }

    public float M12
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => values[1];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => values[1] = value;
    }

    public float M13
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => values[2];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => values[2] = value;
    }

    public float M14
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => values[3];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => values[3] = value;
    }

    public float M21
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => values[4];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => values[4] = value;
    }

    public float M22
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => values[5];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => values[5] = value;
    }

    public float M23
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => values[6];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => values[6] = value;
    }

    public float M24
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => values[7];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => values[7] = value;
    }

    public float M31
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => values[8];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => values[8] = value;
    }

    public float M32
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => values[9];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => values[9] = value;
    }

    public float M33
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => values[10];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => values[10] = value;
    }

    public float M34
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => values[11];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => values[11] = value;
    }

    public float M41
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => values[12];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => values[12] = value;
    }

    public float M42
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => values[13];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => values[13] = value;
    }

    public float M43
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => values[14];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => values[14] = value;
    }

    public float M44
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => values[15];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => values[15] = value;
    }


    public static Matrix4x4 Identity { get; } = new Matrix4x4
    (
        1f, 0f, 0f, 0f,
        0f, 1f, 0f, 0f,
        0f, 0f, 1f, 0f,
        0f, 0f, 0f, 1f
    );


    public bool IsIdentity
    {
        get
        {
            return values[0] == 1f && values[6] == 1f && values[12] == 1f && values[15] == 1f && // Check diagonal element first for early out.
                   values[1] == 0f && values[2] == 0f && values[3] == 0f &&
                   values[4] == 0f && values[6] == 0f && values[7] == 0f &&
                   values[8] == 0f && values[9] == 0f && values[11] == 0f &&
                   values[12] == 0f && values[13] == 0f && values[14] == 0f;
        }
    }

    public Vec2 Translation
    {
        get { return new Vec2(values[12], values[13]); }
        set
        {
            values[12] = value.X;
            values[13] = value.Y;
        }
    }

    public float TranslationZ
    {
        get => values[14];
        set => values[14] = value;
    }


    public Matrix4x4(float M11, float M12, float M13, float M14,
        float M21, float M22, float M23, float M24,
        float M31, float M32, float M33, float M34,
        float M41, float M42, float M43, float M44)
    {
        values[0] = M11;
        values[1] = M12;
        values[2] = M13;
        values[3] = M14;

        values[4] = M21;
        values[5] = M22;
        values[6] = M23;
        values[7] = M24;

        values[8] = M31;
        values[9] = M32;
        values[10] = M33;
        values[11] = M34;

        values[12] = M41;
        values[13] = M42;
        values[14] = M43;
        values[15] = M44;
    }

    /*public Matrix4x4(Matrix3x2 value)
    {
        M11 = value.M11;
        M12 = value.M12;
        M13 = 0f;
        M14 = 0f;
        M21 = value.M21;
        M22 = value.M22;
        M23 = 0f;
        M24 = 0f;
        M31 = 0f;
        M32 = 0f;
        M33 = 1f;
        M34 = 0f;
        M41 = value.M31;
        M42 = value.M32;
        M43 = 0f;
        M44 = 1f;
    }#1#


    public static Matrix4x4 CreateTranslation(float xPosition, float yPosition, float zPosition)
    {
        return new Matrix4x4(
            1.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 1.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 1.0f, 0.0f,
            xPosition, yPosition, zPosition, 1.0f);
    }

    public static Matrix4x4 CreateScale(float xScale, float yScale, float zScale)
    {
        return new Matrix4x4(
            xScale, 0.0f, 0.0f, 0.0f,
            0.0f, yScale, 0.0f, 0.0f,
            0.0f, 0.0f, zScale, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f);
    }

    public static Matrix4x4 CreateScale(float scale)
    {
        return new Matrix4x4(
            scale, 0.0f, 0.0f, 0.0f,
            0.0f, scale, 0.0f, 0.0f,
            0.0f, 0.0f, scale, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f);
    }


    public static Matrix4x4 CreateRotationZ(float radians)
    {
        Matrix4x4 result;

        float c = (float)Math.Cos(radians);
        float s = (float)Math.Sin(radians);
        result.values[0] = c;
        result.values[1] = s;
        result.values[2] = 0.0f;
        result.values[3] = 0.0f;
        result.values[4] = -s;
        result.values[5] = c;
        result.values[6] = 0.0f;
        result.values[7] = 0.0f;
        result.values[8] = 0.0f;
        result.values[9] = 0.0f;
        result.values[10] = 1.0f;
        result.values[11] = 0.0f;
        result.values[12] = 0.0f;
        result.values[13] = 0.0f;
        result.values[14] = 0.0f;
        result.values[15] = 1.0f;

        return result;
    }

    public static Matrix4x4 CreateOrthographic(float width, float height, float zNearPlane, float zFarPlane)
    {
        Matrix4x4 result;

        result.values[0] = 2.0f / width;
        result.values[1] = result.values[2] = result.values[3] = 0.0f;

        result.values[5] = 2.0f / height;
        result.values[4] = result.values[6] = result.values[7] = 0.0f;

        result.values[10] = 1.0f / (zNearPlane - zFarPlane);
        result.values[8] = result.values[9] = result.values[11] = 0.0f;

        result.values[12] = result.values[13] = 0.0f;
        result.values[14] = zNearPlane / (zNearPlane - zFarPlane);
        result.values[15] = 1.0f;

        return result;
    }

    public static Matrix4x4 CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNearPlane, float zFarPlane)
    {
        Matrix4x4 result;

        result.values[0] = 2.0f / (right - left);
        result.values[1] = result.values[2] = result.values[3] = 0.0f;

        result.values[5] = 2.0f / (top - bottom);
        result.values[4] = result.values[6] = result.values[7] = 0.0f;

        result.values[10] = 1.0f / (zNearPlane - zFarPlane);
        result.values[8] = result.values[9] = result.values[11] = 0.0f;

        result.values[12] = (left + right) / (left - right);
        result.values[13] = (top + bottom) / (bottom - top);
        result.values[14] = zNearPlane / (zNearPlane - zFarPlane);
        result.values[15] = 1.0f;

        return result;
    }

    public static Matrix4x4 operator -(Matrix4x4 value)
    {
        Matrix4x4 m;

        m.values[0] = -value.values[0];
        m.values[1] = -value.values[1];
        m.values[2] = -value.values[2];
        m.values[3] = -value.values[3];
        m.values[4] = -value.values[4];
        m.values[5] = -value.values[5];
        m.values[6] = -value.values[6];
        m.values[7] = -value.values[7];
        m.values[8] = -value.values[8];
        m.values[9] = -value.values[9];
        m.values[10] = -value.values[10];
        m.values[11] = -value.values[11];
        m.values[12] = -value.values[12];
        m.values[13] = -value.values[13];
        m.values[14] = -value.values[14];
        m.values[15] = -value.values[15];

        return m;
    }


    public static Matrix4x4 operator +(Matrix4x4 v1, Matrix4x4 v2)
    {
        Matrix4x4 m;

        m.values[0] = v1.values[0] + v2.values[0];
        m.values[1] = v1.values[1] + v2.values[1];
        m.values[2] = v1.values[2] + v2.values[2];
        m.values[3] = v1.values[3] + v2.values[3];
        m.values[4] = v1.values[4] + v2.values[4];
        m.values[5] = v1.values[5] + v2.values[5];
        m.values[6] = v1.values[6] + v2.values[6];
        m.values[7] = v1.values[7] + v2.values[7];
        m.values[8] = v1.values[8] + v2.values[8];
        m.values[9] = v1.values[9] + v2.values[9];
        m.values[10] = v1.values[10] + v2.values[10];
        m.values[11] = v1.values[11] + v2.values[11];
        m.values[12] = v1.values[12] + v2.values[12];
        m.values[13] = v1.values[13] + v2.values[13];
        m.values[14] = v1.values[14] + v2.values[14];
        m.values[15] = v1.values[15] + v2.values[15];

        return m;
    }


    public static Matrix4x4 operator -(Matrix4x4 v1, Matrix4x4 v2)
    {
        Matrix4x4 m;

        m.values[0] = v1.values[0] - v2.values[0];
        m.values[1] = v1.values[1] - v2.values[1];
        m.values[2] = v1.values[2] - v2.values[2];
        m.values[3] = v1.values[3] - v2.values[3];
        m.values[4] = v1.values[4] - v2.values[4];
        m.values[5] = v1.values[5] - v2.values[5];
        m.values[6] = v1.values[6] - v2.values[6];
        m.values[7] = v1.values[7] - v2.values[7];
        m.values[8] = v1.values[8] - v2.values[8];
        m.values[9] = v1.values[9] - v2.values[9];
        m.values[10] = v1.values[10] - v2.values[10];
        m.values[11] = v1.values[11] - v2.values[11];
        m.values[12] = v1.values[12] - v2.values[12];
        m.values[13] = v1.values[13] - v2.values[13];
        m.values[14] = v1.values[14] - v2.values[14];
        m.values[15] = v1.values[15] - v2.values[15];

        return m;
    }

    public static Matrix4x4 operator *(Matrix4x4 v1, Matrix4x4 v2)
    {
        Matrix4x4 m = new();
        m.values[0] = (v1.values[0] * v2.values[0]) + (v1.values[1] * v2.values[4]) + (v1.values[2] * v2.values[8]) + (v1.values[3] * v2.values[12]);
        m.values[1] = (v1.values[0] * v2.values[1]) + (v1.values[1] * v2.values[5]) + (v1.values[2] * v2.values[9]) + (v1.values[3] * v2.values[13]);
        m.values[2] = (v1.values[0] * v2.values[2]) + (v1.values[1] * v2.values[6]) + (v1.values[2] * v2.values[10]) + (v1.values[3] * v2.values[14]);
        m.values[3] = (v1.values[0] * v2.values[3]) + (v1.values[1] * v2.values[7]) + (v1.values[2] * v2.values[11]) + (v1.values[3] * v2.values[15]);

        m.values[4] = (v1.values[4] * v2.values[0]) + (v1.values[5] * v2.values[4]) + (v1.values[6] * v2.values[8]) + (v1.values[7] * v2.values[12]);
        m.values[5] = (v1.values[4] * v2.values[1]) + (v1.values[5] * v2.values[5]) + (v1.values[6] * v2.values[9]) + (v1.values[7] * v2.values[13]);
        m.values[6] = (v1.values[4] * v2.values[2]) + (v1.values[5] * v2.values[6]) + (v1.values[6] * v2.values[10]) + (v1.values[7] * v2.values[14]);
        m.values[7] = (v1.values[4] * v2.values[3]) + (v1.values[5] * v2.values[7]) + (v1.values[6] * v2.values[11]) + (v1.values[7] * v2.values[15]);

        m.values[8] = (v1.values[8] * v2.values[0]) + (v1.values[9] * v2.values[4]) + (v1.values[10] * v2.values[8]) + (v1.values[11] * v2.values[12]);
        m.values[9] = (v1.values[8] * v2.values[1]) + (v1.values[9] * v2.values[5]) + (v1.values[10] * v2.values[9]) + (v1.values[11] * v2.values[13]);
        m.values[10] = (v1.values[8] * v2.values[2]) + (v1.values[9] * v2.values[6]) + (v1.values[10] * v2.values[10]) + (v1.values[11] * v2.values[14]);
        m.values[11] = (v1.values[8] * v2.values[3]) + (v1.values[9] * v2.values[7]) + (v1.values[10] * v2.values[11]) + (v1.values[11] * v2.values[15]);

        m.values[12] = (v1.values[12] * v2.values[0]) + (v1.values[13] * v2.values[4]) + (v1.values[14] * v2.values[8]) + (v1.values[15] * v2.values[12]);
        m.values[13] = (v1.values[12] * v2.values[1]) + (v1.values[13] * v2.values[5]) + (v1.values[14] * v2.values[9]) + (v1.values[15] * v2.values[13]);
        m.values[14] = (v1.values[12] * v2.values[2]) + (v1.values[13] * v2.values[6]) + (v1.values[14] * v2.values[10]) + (v1.values[15] * v2.values[14]);
        m.values[15] = (v1.values[12] * v2.values[3]) + (v1.values[13] * v2.values[7]) + (v1.values[14] * v2.values[11]) + (v1.values[15] * v2.values[15]);

        return m;
    }


    public static Matrix4x4 operator *(Matrix4x4 v1, float v2)
    {
        Matrix4x4 m;

        m.values[0] = v1.values[0] * v2;
        m.values[1] = v1.values[1] * v2;
        m.values[2] = v1.values[2] * v2;
        m.values[3] = v1.values[3] * v2;
        m.values[4] = v1.values[4] * v2;
        m.values[5] = v1.values[5] * v2;
        m.values[6] = v1.values[6] * v2;
        m.values[7] = v1.values[7] * v2;
        m.values[8] = v1.values[8] * v2;
        m.values[9] = v1.values[9] * v2;
        m.values[10] = v1.values[10] * v2;
        m.values[11] = v1.values[11] * v2;
        m.values[12] = v1.values[12] * v2;
        m.values[13] = v1.values[13] * v2;
        m.values[14] = v1.values[14] * v2;
        m.values[15] = v1.values[15] * v2;
        return m;
    }


    public static bool operator ==(Matrix4x4 v1, Matrix4x4 v2)
    {
        return v1.values[0] == v2.values[0] && v1.values[5] == v2.values[5] && v1.values[10] == v2.values[10] && v1.values[15] == v2.values[15] &&
               v1.values[1] == v2.values[1] && v1.values[2] == v2.values[2] && v1.values[3] == v2.values[3] && v1.values[4] == v2.values[4] &&
               v1.values[6] == v2.values[6] && v1.values[7] == v2.values[7] && v1.values[8] == v2.values[8] && v1.values[9] == v2.values[9] &&
               v1.values[11] == v2.values[11] && v1.values[12] == v2.values[12] && v1.values[13] == v2.values[13] && v1.values[14] == v2.values[14];
    }


    public static bool operator !=(Matrix4x4 v1, Matrix4x4 v2)
    {
        return v1.values[0] != v2.values[0] || v1.values[1] != v2.values[1] || v1.values[2] != v2.values[2] || v1.values[3] != v2.values[3] ||
               v1.values[4] != v2.values[4] || v1.values[5] != v2.values[5] || v1.values[6] != v2.values[6] || v1.values[7] != v2.values[7] ||
               v1.values[8] != v2.values[8] || v1.values[9] != v2.values[9] || v1.values[10] != v2.values[10] || v1.values[11] != v2.values[11] ||
               v1.values[12] != v2.values[12] || v1.values[13] != v2.values[13] || v1.values[14] != v2.values[14] || v1.values[15] != v2.values[15];
    }


    public Vec2 MultiplyPoint(Vec2 v)
    {
        Vec2 result;

        result.X = (v.X * this.M11) + (v.Y * this.M21) + this.M41;
        result.Y = (v.X * this.M12) + (v.Y * this.M22) + this.M42;
        //  position.X* matrix.M13 + position.Y * matrix.M23 + matrix.M43,
        //position.X* matrix.M14 + position.Y * matrix.M24 + matrix.M44

        // result.X = this.M11 * v.X + this.M12 * v.Y + this.M13 * v.Z + this.M14 * 1;
        // result.Y = this.M21 * v.X + this.M22 * v.Y + this.M23 * v.Z + this.M24 * 1;

        //float num = this.M41 * v.X + this.M42 * v.Y + this.M43 * v.Z + this.M44 * 1;
        // num = 1  / num; 
        // result *= num; 
        return result;
    }

    public static Matrix4x4 Invert(Matrix4x4 matrix)
    {
        var result = new Matrix4x4();
        float a = matrix.M11, b = matrix.M12, c = matrix.M13, d = matrix.M14;
        float e = matrix.M21, f = matrix.M22, g = matrix.M23, h = matrix.M24;
        float i = matrix.M31, j = matrix.M32, k = matrix.M33, l = matrix.M34;
        float m = matrix.M41, n = matrix.M42, o = matrix.M43, p = matrix.M44;

        float kp_lo = (k * p) - (l * o);
        float jp_ln = (j * p) - (l * n);
        float jo_kn = (j * o) - (k * n);
        float ip_lm = (i * p) - (l * m);
        float io_km = (i * o) - (k * m);
        float in_jm = (i * n) - (j * m);

        float a11 = +((f * kp_lo) - (g * jp_ln) + (h * jo_kn));
        float a12 = -((e * kp_lo) - (g * ip_lm) + (h * io_km));
        float a13 = +((e * jp_ln) - (f * ip_lm) + (h * in_jm));
        float a14 = -((e * jo_kn) - (f * io_km) + (g * in_jm));

        float det = (a * a11) + (b * a12) + (c * a13) + (d * a14);

        if (Math.Abs(det) < float.Epsilon)
        {
            throw new Exception();
        }

        float invDet = 1.0f / det;

        result.M11 = a11 * invDet;
        result.M21 = a12 * invDet;
        result.M31 = a13 * invDet;
        result.M41 = a14 * invDet;

        result.M12 = -((b * kp_lo) - (c * jp_ln) + (d * jo_kn)) * invDet;
        result.M22 = +((a * kp_lo) - (c * ip_lm) + (d * io_km)) * invDet;
        result.M32 = -((a * jp_ln) - (b * ip_lm) + (d * in_jm)) * invDet;
        result.M42 = +((a * jo_kn) - (b * io_km) + (c * in_jm)) * invDet;

        float gp_ho = (g * p) - (h * o);
        float fp_hn = (f * p) - (h * n);
        float fo_gn = (f * o) - (g * n);
        float ep_hm = (e * p) - (h * m);
        float eo_gm = (e * o) - (g * m);
        float en_fm = (e * n) - (f * m);

        result.M13 = +((b * gp_ho) - (c * fp_hn) + (d * fo_gn)) * invDet;
        result.M23 = -((a * gp_ho) - (c * ep_hm) + (d * eo_gm)) * invDet;
        result.M33 = +((a * fp_hn) - (b * ep_hm) + (d * en_fm)) * invDet;
        result.M43 = -((a * fo_gn) - (b * eo_gm) + (c * en_fm)) * invDet;

        float gl_hk = (g * l) - (h * k);
        float fl_hj = (f * l) - (h * j);
        float fk_gj = (f * k) - (g * j);
        float el_hi = (e * l) - (h * i);
        float ek_gi = (e * k) - (g * i);
        float ej_fi = (e * j) - (f * i);

        result.M14 = -((b * gl_hk) - (c * fl_hj) + (d * fk_gj)) * invDet;
        result.M24 = +((a * gl_hk) - (c * el_hi) + (d * ek_gi)) * invDet;
        result.M34 = -((a * fl_hj) - (b * el_hi) + (d * ej_fi)) * invDet;
        result.M44 = +((a * fk_gj) - (b * ek_gi) + (c * ej_fi)) * invDet;
        return result;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public bool Equals(Matrix4x4 other) => this == other;

    public override bool Equals(object obj) => (obj is Matrix4x4 other) && (this == other);
}*/