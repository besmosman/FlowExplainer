using System.Numerics;
using ImGuiNET;

namespace FlowExplainer;

public interface IVecUpDimension<THigh>
{
    THigh Up(float t);
}