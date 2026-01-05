using System.Diagnostics;

namespace FlowExplainer;

//Source: https://gitlab.com/Nurose/Nurose is it stealing if I steal from myself?
public static class Profiler
{
    public enum EntryType
    {
        None,
        Unfinished,
        Finished,
        Fake,
    }

    public struct Entry
    {
        public EntryType Type;
        public TimeSpan Duration;
        public int EntriesCount;
        public int Depth;
        public string Category;
    }

    public static int CurrentEntries = 0;
    public static Entry[] Entries = new Entry[5276 / 4];
    private static List<int> Parents = new();
    private static Stopwatch stopwatch = Stopwatch.StartNew();
    public static bool IsPaused { get; private set; }
    public static bool IsRunning;

    public static void Cleanup()
    {
#if DEBUG

        for (int i = (Entries.Length / 2) + 1; i < Entries.Length; i++)
        {
            if (Entries[i].Type == EntryType.Finished && Entries[i].Depth == 0)
            {
                int s = 0;
                for (int j = i; j < Entries.Length; j++)
                {
                    Entries[s] = Entries[j];
                    s++;
                }

                CurrentEntries = s;
                break;
            }
        }


        for (int i = 0; i < Parents.Count; i++)
        {
            Entries[CurrentEntries++] = new Entry
            {
                Type = EntryType.Fake,
                Category = Entries[Parents[i]].Category,
                Depth = Parents.Count,
                EntriesCount = -3,
                Duration = default,
            };
        }
#endif
    }

    public static void Begin()
    {
    }

    public static void Begin(string categorie)
    {
#if DEBUG
        if (!IsRunning || IsPaused)
            return;

        if (CurrentEntries > Entries.Length - 1)
            Cleanup();


        Entries[CurrentEntries++] = new Entry
        {
            Type = EntryType.Unfinished,
            Category = categorie,
            Depth = Parents.Count,
            EntriesCount = -1,
            Duration = stopwatch.Elapsed,
        };
        Parents.Add(CurrentEntries - 1);
#endif
    }


    public static void End(string categorie)
    {
#if DEBUG
        if (!IsRunning || IsPaused)
            return;

        if(Parents.Count == 0) //Start Running issue
            return;
        int parentId = Parents.Last();
        Parents.RemoveAt(Parents.Count - 1);
        var parent = Entries[parentId];

        Entries[parentId] = new Entry
        {
            Category = parent.Category,
            EntriesCount = CurrentEntries - parentId,
            Depth = parent.Depth,
            Type = EntryType.Finished,
            Duration = stopwatch.Elapsed - parent.Duration,
        };

        if (categorie != parent.Category)
            throw new Exception();

#endif
    }

    public static void Pause()
    {
#if DEBUG
        Begin("Profiler pause");
        IsPaused = true;
#endif
    }

    public static void Unpause()
    {
#if DEBUG
        IsPaused = false;
        End("Profiler pause");
#endif
    }
}