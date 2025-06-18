using System.Collections.Concurrent;
using System.Threading;

namespace FlowExplainer;

public static class AssetWatcher
{
    public static event Action<FileSystemEventArgs>? OnChange;

    private static readonly Queue<FileSystemEventArgs> events = new();
    private static readonly ConcurrentBag<string> queuedFilesChanged = new();
    public static readonly FileSystemWatcher watcherBin;
    public static readonly FileSystemWatcher watcherDevAssets;

    private static ManualResetEvent manualResetEvent = new ManualResetEvent(true);
    public static string DevAssetsPath = Path.GetFullPath(Path.Join(Directory.GetCurrentDirectory(), "/../../../Assets"));

    static AssetWatcher()
    {
        watcherBin = SetupWatcher(Directory.GetCurrentDirectory());
        watcherDevAssets = SetupWatcher(DevAssetsPath);
    }

    private static FileSystemWatcher SetupWatcher(string directory)
    {
        var watcher = new FileSystemWatcher(directory)
        {
            IncludeSubdirectories = true,
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
        };

        // watcher.Created += OnFileChange;
        watcher.Renamed += OnFileChange;
        watcher.Changed += OnFileChange;
        //watcher.Deleted += OnFileChange;
        return watcher;
    }

    public static void Try(Action a, int count)
    {
        int c = 0;
        while (c < count)
        {
            try
            {
                a();
                Thread.Sleep(10);
                return;
            }
            catch (Exception e)
            {
                c++;
            }
        }
    }

    private static void OnFileChange(object sender, FileSystemEventArgs e)
    {
        Logger.LogDebug(e.Name);
        // manualResetEvent.WaitOne();
        //manualResetEvent.Reset();
        //stops imgui from crashing randomly.
        if (e.Name == "imgui.ini")
            return;

        if (e.FullPath.Contains(DevAssetsPath) && Path.HasExtension(e.FullPath))
        {
            var tochange = Path.Combine(Directory.GetCurrentDirectory(), "Assets\\" + Path.GetRelativePath(DevAssetsPath, e.FullPath));
            if (File.Exists(tochange))
            {
                File.Delete(tochange);
                int count = 0;
                Try(() => File.Copy(e.FullPath, tochange), 5);
            }

            int c = 5;
        }

        queuedFilesChanged.Add(e.FullPath); 
        events.Enqueue(e);
        // manualResetEvent.Set();
    }

    public static void Execute()
    {
        queuedFilesChanged.Clear();
        while (events.TryDequeue(out var q))
        {
            Thread.Sleep(64);
            OnChange?.Invoke(q);
        }
    }
}