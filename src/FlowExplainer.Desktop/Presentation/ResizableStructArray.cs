namespace FlowExplainer;

public class ResizableStructArray<T> where T : struct
{
    private T[] Entries;

    public T[] Array => Entries;
    public ResizableStructArray(int c)
    {
        Entries = new T[c];
    }

    public ref T this[int index] => ref Entries[index];

    public int Length
    {
        get => Entries.Length;
    }
    
    public bool ResizeIfNeeded(int c, bool reset = false)
    {
        if (Entries.Length != c)
        {
            if (reset)
                Entries = new T[c];
            else
                System.Array.Resize(ref Entries, c);
            return true;
        }

        return false;
    }

    public Span<T> AsSpan()
    {
        return Entries.AsSpan();
    }
}