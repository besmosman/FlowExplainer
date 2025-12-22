namespace FlowExplainer;

public struct Matrix2
{
    private Vec2 Row1;
    private Vec2 Row2;

    public double M11
    {
        get => Row1.X;
        set => Row1.X = value;
    }
    public double M12
    {
        get => Row1.Y;
        set => Row1.Y = value;
    }

    public double M21
    {
        get => Row2.X;
        set => Row2.X = value;
    }
    public double M22
    {
        get => Row2.Y;
        set => Row2.Y = value;
    }
    
    public Matrix2(Vec2 row1, Vec2 row2)
    {
        Row1 = row1;
        Row2 = row2;
    }
    
    public double Determinant => (Row1.X * Row2.Y) - (Row1.Y * Row2.X);
    public double Trace => Row1.X + Row2.Y;

    public Matrix2 AddOuterProduct(Vec2 u, Vec2 v)
    {
        return new Matrix2
        {
            Row1 = Row1 + u.X * v,
            Row2 = Row2 + u.Y * v,
        };
    }

    public static Matrix2 operator *(Matrix2 A, Matrix2 B)
    {
        double m11 = A.Row1.X * B.Row1.X + A.Row1.Y * B.Row2.X; // Row1 * Col1
        double m12 = A.Row1.X * B.Row1.Y + A.Row1.Y * B.Row2.Y; // Row1 * Col2
    
        double m21 = A.Row2.X * B.Row1.X + A.Row2.Y * B.Row2.X; // Row2 * Col1
        double m22 = A.Row2.X * B.Row1.Y + A.Row2.Y * B.Row2.Y; // Row2 * Col2

        return new Matrix2
        {
            Row1 = new Vec2(m11, m12),
            Row2 = new Vec2(m21, m22)
        };
    }

    public static Matrix2 operator *(Matrix2 A, double d)
    {
        return new Matrix2
        {
            Row1 = A.Row1 * d,
            Row2 = A.Row2 * d,
        };
    }
    public Matrix2 Inverse()
    {
        double det = Determinant;

        if (Math.Abs(det) < 1e-10f) det = 1e-10f; // Regularization
        var invDet = 1.0 / det;

        return new Matrix2
        {
            Row1 = new Vec2(+Row2.Y, -Row1.Y) * invDet,
            Row2 = new Vec2(-Row2.X, +Row1.X) * invDet,
        };
    }

    public Matrix2 Transpose()
    {
        return new Matrix2
        {
            Row1 = new Vec2(Row1.X, Row2.X),
            Row2 = new Vec2(Row1.Y, Row2.Y),
        };
    }
}