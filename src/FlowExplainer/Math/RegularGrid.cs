using System.Runtime.CompilerServices;
using MemoryPack;

namespace FlowExplainer;


public class RegularGrid<Veci, TData> where Veci : IVec<Veci, int>
{
    public TData[] Data { get; private set; }
    public Veci GridSize { get; private set; }

    private Veci multipliers;

    public ref TData this[Veci index]
    {
        get => ref AtCoords(index);
    }

    public RegularGrid(Veci gridSize)
    {
        GridSize = gridSize;
        Data = new TData[GridSize.Volume()];
        multipliers = ComputeMultipliers();
    }

    public RegularGrid(TData[] data, Veci gridSize)
    {
        GridSize = gridSize;
        Data = data;
        multipliers = ComputeMultipliers();
    }

    private Veci ComputeMultipliers()
    {
        Veci m = Veci.Zero;
        m[0] = 1;
        for (int i = 1; i < GridSize.ElementCount; i++)
            m[i] = m[i - 1] * GridSize[i - 1];
        return m;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref TData AtCoords(Veci x)
    {
        //x = Utils.Clamp<Veci, int>(x, Veci.Zero, GridSize - Veci.One);
        if (!IsWithin(x))
        {
            throw new Exception("Not within bounds");
        }
            
        int index = GetCoordsIndex(x);
        return ref Data[index];
    }
    


    public int GetCoordsIndex(Veci x)
    {
        return (x * multipliers).Sum();
    }

    public Veci GetIndexCoords(int i)
    {
        Veci coords = default!;
        int remaining = i;

        for (int d = 0; d < GridSize.ElementCount; d++)
        {
            int value = remaining % GridSize[d];
            coords[d] = value;
            remaining /= GridSize[d];
        }

        return coords;
    }

    public bool IsWithin(Veci coord)
    {
        for (int i = 0; i < GridSize.ElementCount; i++)
            if (coord[i] < 0 || coord[i] >= GridSize[i])
                return false;

        return true;
    }

    public void Resize(Veci gridSize)
    {
        GridSize = gridSize;
        Data = new TData[GridSize.Volume()];
        multipliers = ComputeMultipliers();
    }
    

    private static void ValidateSize(RegularGrid<Veci,TData> a, RegularGrid<Veci, TData> b)
    {
        if (!a.GridSize.Equals(b.GridSize))
            throw new Exception("Different sizes");
    }
}