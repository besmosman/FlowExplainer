using System.Numerics;
using ImGuiNET;

namespace FlowExplainer;

public interface IAddDimension<TIn, TOut>
{
    TOut Up(float f);
}

public interface IVec
{
    public float Get(int i);
}

public struct Vec1
{
    public float X;
}