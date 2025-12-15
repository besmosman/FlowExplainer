using MemoryPack;

namespace FlowExplainer;

[MemoryPackable]
public partial struct RegularGridVectorFieldSave<Vec, Veci, TData> where Vec : IVec<Vec, double>
{
    public string Name;
    public TData[] Data;
    public Veci GridSize;
    public Vec Min;
    public Vec Max;
    public GenBounding<Vec> Boundings;
}