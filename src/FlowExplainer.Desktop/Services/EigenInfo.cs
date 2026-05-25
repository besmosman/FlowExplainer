using System.Numerics;

namespace FlowExplainer;

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