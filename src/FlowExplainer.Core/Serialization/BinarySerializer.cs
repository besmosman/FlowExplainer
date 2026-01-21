using MemoryPack;

namespace FlowExplainer;

public static class BinarySerializer
{
    public static void Save<T>(string path, T s)
    {
        if (File.Exists(path))
            File.Delete(path);
        
        using var fileStream = new FileStream(path, FileMode.CreateNew);
        MemoryPackSerializer.SerializeAsync(fileStream, s).AsTask().Wait();
    }

    public static T Load<T>(string path) where T : new()
    {
        using var fileStream = new FileStream(path, FileMode.Open);
        return MemoryPackSerializer.DeserializeAsync<T>(fileStream).Result!;
    }


    public static T CloneBySerialization<T>(T s)
    {
        using var mem = new MemoryStream();
        MemoryPackSerializer.SerializeAsync(mem, s).AsTask().Wait();
        mem.Position = 0;
        return MemoryPackSerializer.DeserializeAsync<T>(mem).Result!;
    }
}