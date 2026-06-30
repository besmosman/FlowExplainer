namespace FlowExplainer;

public interface IVecIntegerEquivalent<TVeci> where TVeci : IVec<TVeci, int>
{
    TVeci FloorInt();
    TVeci RoundInt();
}