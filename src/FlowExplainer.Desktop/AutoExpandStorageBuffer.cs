namespace FlowExplainer;

public class AutoExpandStorageBuffer<TData> where TData : struct
{
    public StorageBuffer<TData> buffer = new(64);
    private int cur = 0;

    public AutoExpandStorageBuffer()
    {
    }

    public ref int GetCurrentIndex()
    {
        return ref cur;
    }

    public void Use()
    {
        buffer.Use();
    }

    public void Upload()
    {
        buffer.Upload();
    }


    public void Register(TData data)
    {
        buffer.Data[cur] = data;
        cur++;
        if (cur >= buffer.Data.Length)
        {
            var old = buffer.Data;
            buffer.Resize(buffer.Length * 2);
            Array.Copy(old, buffer.Data, old.Length);
        }
    }

    public void Reset()
    {
        cur = 0;
    }
}