namespace FlowExplainer;

public struct Vec1 : IVec<Vec1, float>
{
    public float X;

    public Vec1(float x)
    {
        X = x;
    }

    public int Dimensions => 1;
    public float Last => X;

    public float this[int n]
    {
        get
        {
            if (n != 0)
                throw new Exception();
            return X;
        }
        set
        {
            if (n != 0)
                throw new Exception();
            X = value;
        }
    }

    public static Vec1 operator *(Vec1 left, float right) => left.X * right;
    public static Vec1 operator -(Vec1 left, Vec1 right) => left.X * right;
    public static Vec1 operator /(Vec1 left, float right) => left.X / right;
    public static Vec1 operator +(Vec1 left, Vec1 right) => left.X + right.X;
    
    public static Vec1 operator *(float left, Vec1 right) => left * right.X;

    public Vec1 Max(Vec1 b) => float.Max(X, b.X);

    public static implicit operator float(Vec1 v) => v.X;
    public static implicit operator Vec1(float v) => new Vec1(v);


}