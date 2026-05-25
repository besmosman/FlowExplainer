using System.Numerics;

namespace FlowExplainer;

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